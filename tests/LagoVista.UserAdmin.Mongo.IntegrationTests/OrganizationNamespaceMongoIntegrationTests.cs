using LagoVista.CloudStorage.Interfaces;
using LagoVista.CloudStorage.Storage.ConnectionSettings;
using LagoVista.CloudStorage.Storage.StorageProviders;
using LagoVista.CloudStorage.Storage.StorageProviders.Mongo;
using LagoVista.Core.AI.Interfaces;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.Core.Models;
using LagoVista.Core.Repos;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Repos.Orgs;
using LagoVista.UserAdmin.Repos;
using LagoVista.Models;
using Moq;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            $"--name {_containerName} public.ecr.aws/docker/library/mongo:8.0");

        Assert.That(RunDocker($"inspect --format={{{{.HostConfig.Memory}}}} {_containerName}").Output.Trim(),
            Is.EqualTo("536870912"), "Mongo integration container must be limited to 512 MB.");
        Assert.That(RunDocker($"inspect --format={{{{.HostConfig.NanoCpus}}}} {_containerName}").Output.Trim(),
            Is.EqualTo("1000000000"), "Mongo integration container must be limited to one CPU.");
        Assert.That(RunDocker($"inspect --format={{{{.HostConfig.RestartPolicy.Name}}}} {_containerName}").Output.Trim(),
            Is.EqualTo("no"), "Mongo integration container must never restart automatically.");

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

        if (!String.IsNullOrWhiteSpace(_containerName))
        {
            var containerInspect = RunDocker($"inspect {_containerName}", throwOnError: false);
            Assert.That(containerInspect.ExitCode, Is.Not.EqualTo(0),
                "Mongo integration container must not remain after the test invocation.");
        }

        if (!String.IsNullOrWhiteSpace(_volumeName))
        {
            var volumeInspect = RunDocker($"volume inspect {_volumeName}", throwOnError: false);
            Assert.That(volumeInspect.ExitCode, Is.Not.EqualTo(0),
                "Mongo integration volume must not remain after the test invocation.");
        }

        TestContext.Progress.WriteLine(
            $"Mongo integration cleanup verified for run {RunId}: container and disposable volume removed.");
    }

    private static void StartExternalWatchdog(string containerName, string volumeName, int lifetimeSeconds = 180)
    {

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

    internal static void VerifyIndependentWatchdogCleanup()
    {
        var probeId = $"watchdog-{Guid.NewGuid():N}";
        var containerName = $"nuviot-useradmin-{probeId}";
        var volumeName = $"nuviot-useradmin-{probeId}-data";
        var sentinelVolumeName = $"nuviot-useradmin-{probeId}-sentinel";

        try
        {
            RunDocker($"volume create --label {LabelKey}={probeId} {volumeName}");
            RunDocker($"volume create {sentinelVolumeName}");
            RunDocker(
                "run -d --rm --restart=no --memory=64m --cpus=0.25 " +
                $"--label {LabelKey}={probeId} " +
                $"-v {volumeName}:/probe " +
                $"--name {containerName} --entrypoint sh public.ecr.aws/docker/library/mongo:8.0 -c \"sleep 60\"");

            StartExternalWatchdog(containerName, volumeName, lifetimeSeconds: 3);

            var removed = false;
            for (var attempt = 0; attempt < 15; attempt++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                var containerInspect = RunDocker($"inspect {containerName}", throwOnError: false);
                var volumeInspect = RunDocker($"volume inspect {volumeName}", throwOnError: false);
                if (containerInspect.ExitCode != 0 && volumeInspect.ExitCode != 0)
                {
                    removed = true;
                    break;
                }
            }

            Assert.That(removed, Is.True,
                "External watchdog must remove an orphaned probe container and its disposable volume after the bounded lifetime.");
            Assert.That(RunDocker($"volume inspect {sentinelVolumeName}", throwOnError: false).ExitCode, Is.EqualTo(0),
                "Watchdog cleanup must not remove an unrelated sentinel volume.");
        }
        finally
        {
            RunDocker($"rm -f {containerName}", throwOnError: false);
            RunDocker($"volume rm -f {volumeName}", throwOnError: false);
            RunDocker($"volume rm -f {sentinelVolumeName}", throwOnError: false);
        }
    }

    internal static DockerResult RunDocker(string arguments, bool throwOnError = true)
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

    internal sealed record DockerResult(int ExitCode, string Output, string Error);
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
        _databaseName = $"ua_ns_{Guid.NewGuid():N}";

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
        services.SetupGet(service => service.RagIndexingServices).Returns((IRagIndexingServices)null);
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
    public async Task NamespaceLookup_StoredAsRealBson_ReturnsExistingTrueAndMissingFalse()
    {
        var org = new Organization
        {
            Id = "A1234567890B1234567890C123456789",
            Name = "Namespace Integration Test",
            Namespace = OrgNamespace.Parse("namespaceintegration"),
            OwnerOrganization = EntityHeader.Create("A1234567890B1234567890C123456789", "Namespace Integration Test"),
            DatabaseName = _databaseName,
            EntityType = nameof(Organization)
        };

        await _storageClient.CreateDocumentAsync(org);

        var database = _cleanupClient.GetDatabase(_databaseName);
        using var collectionCursor = await database.ListCollectionNamesAsync();
        var collectionNames = await collectionCursor.ToListAsync();
        Assert.That(collectionNames, Has.Count.EqualTo(1),
            $"Expected exactly one collection after persisting the test Organization, found: {String.Join(", ", collectionNames)}.");

        var rawCollection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionNames.Single());
        var persisted = await rawCollection.Find(MongoDB.Bson.BsonDocument.Parse("{ }")).FirstOrDefaultAsync();
        Assert.That(persisted, Is.Not.Null, "Expected real BSON to be persisted before exercising OrganizationRepo.");
        Assert.That(persisted["Namespace"].IsString, Is.True,
            $"Expected Namespace to be serialized as a BSON string but got: {persisted["Namespace"].BsonType}.");
        Assert.That(persisted["Namespace"].AsString, Is.EqualTo("namespaceintegration"),
            "Expected the stored Namespace BSON scalar to contain the canonical namespace value.");
        Assert.That(persisted["EntityType"].AsString, Is.EqualTo(nameof(Organization)),
            "The integration document must carry the same EntityType metadata produced by DocumentDBRepoBase before querying it.");

        var existingInUse = await _repo.QueryNamespaceInUseAsync("namespaceintegration");
        var missingInUse = await _repo.QueryNamespaceInUseAsync("anothernamespace");

        Assert.That(existingInUse, Is.True,
            "Existing namespace persisted by the Mongo provider must be found through OrganizationRepo.QueryNamespaceInUseAsync.");
        Assert.That(missingInUse, Is.False,
            "A valid namespace that is not persisted must not be reported as in use.");
    }
}

[TestFixture]
[Category("Integration")]
[Category("MongoIntegration")]
[Explicit("Opt-in Docker-backed Mongo integration suite.")]
[NonParallelizable]
public sealed class MongoAssemblyFixtureSharingIntegrationTests
{
    [Test]
    public async Task SecondTestClass_ReusesRunScopedMongo_AndKeepsDatabaseIsolated()
    {
        var runId = MongoAssemblyFixture.RunId;
        Assert.That(runId, Is.Not.Null.And.Not.Empty,
            "Assembly fixture must have started exactly one run-scoped Mongo service before this test class executes.");
        Assert.That(MongoAssemblyFixture.HostPort, Is.GreaterThan(0));

        var settings = new MongoDocumentStorageConnectionSettings
        {
            Hosts = new List<string> { "localhost" }.AsReadOnly(),
            Port = MongoAssemblyFixture.HostPort,
            UserName = "nuviot-test",
            Password = "nuviot-test-password",
            AuthenticationDatabase = "admin",
            DatabaseName = $"ua_share_{Guid.NewGuid():N}"
        };

        var client = new MongoClient(settings.BuildConnectionString());
        var database = client.GetDatabase(settings.DatabaseName);
        try
        {
            var command = new MongoDB.Bson.BsonDocument("ping", 1);
            var ping = await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(command);
            Assert.That(ping["ok"].ToDouble(), Is.EqualTo(1.0));

            var collection = database.GetCollection<MongoDB.Bson.BsonDocument>("fixture-sharing");
            await collection.InsertOneAsync(new MongoDB.Bson.BsonDocument("runId", runId));
            Assert.That(await collection.CountDocumentsAsync(MongoDB.Bson.BsonDocument.Parse("{ }")), Is.EqualTo(1));
            Assert.That(MongoAssemblyFixture.RunId, Is.EqualTo(runId),
                "Both integration test classes must share the same assembly-scoped Mongo run.");
        }
        finally
        {
            await client.DropDatabaseAsync(settings.DatabaseName);
        }
    }

    [Test]
    public void ExternalWatchdog_RemovesOrphanedProbe_WithoutTouchingUnrelatedResource()
    {
        MongoAssemblyFixture.VerifyIndependentWatchdogCleanup();
    }
}
