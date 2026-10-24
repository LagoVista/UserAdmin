using LagoVista.CloudStorage.Interfaces;
using LagoVista.CloudStorage.Storage.ConnectionSettings;
using LagoVista.CloudStorage.Storage.StorageProviders;
using LagoVista.CloudStorage.Storage.StorageProviders.Mongo;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Repos.Orgs;
using Moq;
using MongoDB.Driver;
using NUnit.Framework;
using System.Diagnostics;

namespace LagoVista.UserAdmin.Mongo.IntegrationTests;

[SetUpFixture]
public sealed class MongoAssemblyFixture
{
    private const string UserName = "nuviot-test";
    private const string Password = "nuviot-test-password";
    private const string LabelKey = "nuviot.useradmin.mongo-integration.run";
    private static string _containerName;
    private static string _volumeName;

    internal static int HostPort { get; private set; }
    internal static string RunId { get; private set; }

    [OneTimeSetUp]
    public void StartMongo()
    {
        RunId = $"ua-{Guid.NewGuid():N}";
        _containerName = $"nuviot-useradmin-mongo-{RunId}";
        _volumeName = $"nuviot-useradmin-mongo-data-{RunId}";

        RunDocker("info");
        RunDocker($"volume create --label {LabelKey}={RunId} {_volumeName}");
        RunDocker(
            "run -d --rm --restart=no " +
            "--memory=512m --cpus=1.0 " +
            $"--label {LabelKey}={RunId} " +
            $"-e MONGO_INITDB_ROOT_USERNAME={UserName} " +
            $"-e MONGO_INITDB_ROOT_PASSWORD={Password} " +
            "-p 127.0.0.1::27017 " +
            $"-v {_volumeName}:/data/db " +
            $"--name {_containerName} mongo:8.0");

        StartExternalWatchdog(_containerName, _volumeName);

        for (var attempt = 0; attempt < 45; attempt++)
        {
            var probe = RunDocker(
                $"exec {_containerName} mongosh --quiet --username {UserName} --password {Password} " +
                "--authenticationDatabase admin --eval \"db.adminCommand('ping').ok\"",
                throwOnError: false);

            if (probe.ExitCode == 0 && probe.Output.Contains("1"))
            {
                var port = RunDocker($"port {_containerName} 27017/tcp").Output.Trim();
                HostPort = Int32.Parse(port[(port.LastIndexOf(':') + 1)..]);
                return;
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        throw new TimeoutException($"Mongo container {_containerName} did not become ready.");
    }

    [OneTimeTearDown]
    public void StopMongo()
    {
        if (!String.IsNullOrWhiteSpace(_containerName))
            RunDocker($"rm -f {_containerName}", throwOnError: false);

        if (!String.IsNullOrWhiteSpace(_volumeName))
            RunDocker($"volume rm -f {_volumeName}", throwOnError: false);
    }

    private static void StartExternalWatchdog(string containerName, string volumeName)
    {
        const int lifetimeSeconds = 180;

        if (OperatingSystem.IsWindows())
        {
            var command =
                $"Start-Sleep -Seconds {lifetimeSeconds}; " +
                $"docker rm -f {containerName} 2>$null; " +
                $"docker volume rm -f {volumeName} 2>$null";

            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                ArgumentList = { "-NoProfile", "-NonInteractive", "-Command", command },
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        else
        {
            var command =
                $"sleep {lifetimeSeconds}; " +
                $"docker rm -f {containerName} >/dev/null 2>&1 || true; " +
                $"docker volume rm -f {volumeName} >/dev/null 2>&1 || true";

            Process.Start(new ProcessStartInfo
            {
                FileName = "bash",
                ArgumentList = { "-lc", command },
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }

    private static DockerResult RunDocker(string arguments, bool throwOnError = true)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException("Unable to start Docker CLI.");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(30_000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"Docker command timed out: docker {arguments}");
        }

        if (throwOnError && process.ExitCode != 0)
            throw new InvalidOperationException($"Docker command failed ({process.ExitCode}): docker {arguments}\n{error}");

        return new DockerResult(process.ExitCode, output, error);
    }

    private sealed record DockerResult(int ExitCode, string Output, string Error);
}

[TestFixture]
[Category("Integration")]
[Category("MongoIntegration")]
[Explicit("Opt-in Docker-backed Mongo integration suite.")]
[NonParallelizable]
public sealed class OrganizationNamespaceMongoIntegrationTests
{
    private MongoClient _cleanupClient;
    private string _databaseName;
    private MongoDocumentStorageClient _storageClient;
    private OrganizationRepo _repo;

    [SetUp]
    public void SetUp()
    {
        _databaseName = $"useradmin_namespace_{MongoAssemblyFixture.RunId}_{Guid.NewGuid():N}";

        var settings = new MongoDocumentStorageConnectionSettings
        {
            Hosts = new List<string> { "localhost" }.AsReadOnly(),
            Port = MongoAssemblyFixture.HostPort,
            UserName = "nuviot-test",
            Password = "nuviot-test-password",
            AuthenticationDatabase = "admin",
            DatabaseName = _databaseName
        };

        _storageClient = new MongoDocumentStorageClient(
            settings,
            new DocumentCollectionNameResolver(),
            new MongoStorageClientFactory());

        _cleanupClient = new MongoClient(settings.BuildConnectionString());

        var clientProvider = new Mock<IDocumentStorageClientProvider>();
        clientProvider.Setup(provider => provider.GetClient()).Returns(_storageClient);

        var services = new Mock<IDocumentCloudCachedServices>();
        services.SetupGet(service => service.AdminLogger).Returns(new Mock<IAdminLogger>().Object);
        services.SetupGet(service => service.DocumentStorageClientProvider).Returns(clientProvider.Object);
        services.SetupGet(service => service.CacheProvider).Returns((ICacheProvider)null);
        services.SetupGet(service => service.CacheAborter).Returns((ICacheAborter)null);
        services.SetupGet(service => service.EntityListCacheInvalidator).Returns((IEntityListCacheInvalidator)null);
        services.SetupGet(service => service.DependencyManager).Returns((IDependencyManager)null);
        services.SetupGet(service => service.FkIndexTableWriter).Returns((IFkIndexTableWriterBatched)null);
        services.SetupGet(service => service.ProducedArtifactService).Returns((IProducedArtifactService)null);
        services.SetupGet(service => service.UserNotificationService).Returns((IUserNotificationService)null);
        services.SetupGet(service => service.RagIndexingServices).Returns((LagoVista.Core.AI.Interfaces.IRagIndexingServices)null);
        services.SetupGet(service => service.SystemUsers).Returns(new Mock<ISystemUsers>().Object);

        _repo = new OrganizationRepo(
            new Mock<IUserAdminSettings>().Object,
            new Mock<IOrganizationRelationalRepo>().Object,
            new Mock<IAppUserRelationalRepo>().Object,
            services.Object,
            new Mock<ISystemUsers>().Object,
            new Mock<ILagoVistaAutoMapper>().Object);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_cleanupClient != null && !String.IsNullOrWhiteSpace(_databaseName))
            await _cleanupClient.DropDatabaseAsync(_databaseName);
    }

    [Test]
    public async Task ExistingNamespace_StoredAsRealBson_IsReportedInUse()
    {
        var org = new Organization
        {
            Id = "A1234567890B1234567890C123456789",
            Name = "Namespace Integration Test",
            Namespace = OrgNamespace.Parse("namespaceintegration"),
            OwnerOrganization = EntityHeader.Create("A1234567890B1234567890C123456789", "Namespace Integration Test")
        };

        await _storageClient.CreateDocumentAsync(org);

        var rawCollection = _cleanupClient
            .GetDatabase(_databaseName)
            .GetCollection<MongoDB.Bson.BsonDocument>("organization");

        var persisted = await rawCollection.Find(MongoDB.Bson.BsonDocument.Parse("{ }")).FirstOrDefaultAsync();
        Assert.That(persisted, Is.Not.Null, "Expected real BSON to be persisted before exercising OrganizationRepo.");
        Assert.That(persisted["Namespace"].IsString, Is.True,
            $"Expected Namespace to be serialized as a BSON string but got: {persisted["Namespace"].BsonType}.");

        var inUse = await _repo.QueryNamespaceInUseAsync("namespaceintegration");

        Assert.That(inUse, Is.True,
            "Existing namespace persisted by the Mongo provider must be found through OrganizationRepo.QueryNamespaceInUseAsync.");
    }
}
