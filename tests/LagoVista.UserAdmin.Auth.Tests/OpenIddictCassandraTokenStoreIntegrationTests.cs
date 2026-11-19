using Cassandra;
using LagoVista.AspNetCore.AuthorizationServer;
using LagoVista.AspNetCore.AuthorizationServer.Persistence.Cassandra;
using LagoVista.CloudStorage.Exceptions;
using LagoVista.CloudStorage.Storage.StorageProviders.Cassandra;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    /// <summary>
    /// Opt-in, real-Cassandra regression fixture for OpenIddict's create-then-attach-payload path.
    /// Never targets the application keyspace: the operator must provision a dedicated
    /// oauth_repro_* keyspace and expressly enable these tests.
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public sealed class OpenIddictCassandraTokenStoreIntegrationTests
    {
        private const string ConfirmValue = "RUN_ISOLATED_OAUTH_REPRO";
        private ICluster _cluster;
        private ISession _session;
        private OpenIddictCassandraTokenStore _store;

        [OneTimeSetUp]
        public async Task SetUpAsync()
        {
            if (Environment.GetEnvironmentVariable("OAUTH_REPRO_CONFIRM") != ConfirmValue)
                Assert.Ignore("Real Cassandra reproduction is opt-in; set OAUTH_REPRO_CONFIRM.");

            var keyspace = Environment.GetEnvironmentVariable("OAUTH_REPRO_KEYSPACE") ?? String.Empty;
            if (!Regex.IsMatch(keyspace, @"^oauth_repro_[a-z0-9_]+$", RegexOptions.CultureInvariant))
                Assert.Fail("Use an independently provisioned test-only keyspace named oauth_repro_*.");

            var hosts = (Environment.GetEnvironmentVariable("OAUTH_REPRO_HOSTS") ?? String.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(host => host.Trim())
                .Where(host => host.Length > 0)
                .ToArray();
            if (hosts.Length == 0)
                Assert.Fail("OAUTH_REPRO_HOSTS must specify the designated test Cassandra nodes.");

            var portText = Environment.GetEnvironmentVariable("OAUTH_REPRO_PORT");
            var port = String.IsNullOrWhiteSpace(portText) ? 9042 : Int32.Parse(portText);
            var builder = Cluster.Builder().AddContactPoints(hosts).WithPort(port);
            var user = Environment.GetEnvironmentVariable("OAUTH_REPRO_USER");
            var password = Environment.GetEnvironmentVariable("OAUTH_REPRO_PASSWORD");
            if (!String.IsNullOrWhiteSpace(user))
            {
                if (String.IsNullOrWhiteSpace(password))
                    Assert.Fail("OAUTH_REPRO_PASSWORD is required when OAUTH_REPRO_USER is set.");
                builder = builder.WithCredentials(user, password);
            }

            var dc = Environment.GetEnvironmentVariable("OAUTH_REPRO_LOCAL_DC");
            if (!String.IsNullOrWhiteSpace(dc))
                builder = builder.WithLoadBalancingPolicy(new DCAwareRoundRobinPolicy(dc));

            _cluster = builder.Build();
            _session = await _cluster.ConnectAsync(keyspace).ConfigureAwait(false);
            var factory = new Mock<ICassandraSessionFactory>();
            factory.Setup(f => f.GetSessionAsync()).ReturnsAsync(_session);
            _store = new OpenIddictCassandraTokenStore(factory.Object, new AuthorizationServerOptions());
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _session?.Dispose();
            _cluster?.Dispose();
        }

        [Test]
        public async Task FreshAuthorizationCode_CreateThenAttachPayload_AdvancesVersionZeroToOne()
        {
            var token = NewToken();
            try
            {
                await _store.CreateAsync(token, CancellationToken.None);
                Assert.That(token.Version, Is.EqualTo(0));
                var created = await _store.FindByIdAsync(token.Id, CancellationToken.None);
                Assert.That(created, Is.Not.Null, "The ordinary INSERT must be visible before the conditional UPDATE.");
                Assert.That(created.Version, Is.EqualTo(0));

                await _store.SetPayloadAsync(token, "synthetic-authorization-code-payload", CancellationToken.None);
                await _store.UpdateAsync(token, CancellationToken.None);
                Assert.That(token.Version, Is.EqualTo(1));

                var stored = await _store.FindByIdAsync(token.Id, CancellationToken.None);
                Assert.That(stored.Version, Is.EqualTo(1));
                Assert.That(stored.Payload, Is.EqualTo("synthetic-authorization-code-payload"));
            }
            finally
            {
                await _store.DeleteAsync(token, CancellationToken.None);
            }
        }

        [Test]
        public async Task StaleWriter_IsRejected_AndCannotOverwritePayload()
        {
            var token = NewToken();
            try
            {
                await _store.CreateAsync(token, CancellationToken.None);
                var first = await _store.FindByIdAsync(token.Id, CancellationToken.None);
                var stale = await _store.FindByIdAsync(token.Id, CancellationToken.None);
                Assert.That(first.Version, Is.EqualTo(0));
                Assert.That(stale.Version, Is.EqualTo(0));

                await _store.SetPayloadAsync(first, "winner-synthetic-payload", CancellationToken.None);
                await _store.UpdateAsync(first, CancellationToken.None);
                await _store.SetPayloadAsync(stale, "loser-synthetic-payload", CancellationToken.None);

                Assert.ThrowsAsync<ContentModifiedException>(
                    async () => await _store.UpdateAsync(stale, CancellationToken.None));

                var stored = await _store.FindByIdAsync(token.Id, CancellationToken.None);
                Assert.That(stored.Version, Is.EqualTo(1));
                Assert.That(stored.Payload, Is.EqualTo("winner-synthetic-payload"),
                    "Rejected stale writes must not modify token state.");
            }
            finally
            {
                await _store.DeleteAsync(token, CancellationToken.None);
            }
        }

        [Test]
        public async Task UpdateOfMissingToken_IsRejected_WithoutCreatingToken()
        {
            var missing = NewToken();
            Assert.ThrowsAsync<ContentModifiedException>(
                async () => await _store.UpdateAsync(missing, CancellationToken.None));
            var stored = await _store.FindByIdAsync(missing.Id, CancellationToken.None);
            Assert.That(stored, Is.Null, "An UPDATE must never resurrect an absent token.");
        }

        [Test]
        public async Task RepeatedFreshAuthorizationCodeIssuance_ProducesNoVersionConflicts()
        {
            const int attempts = 30; // Sequential, bounded; not a load test.
            for (var i = 0; i < attempts; i++)
            {
                var token = NewToken();
                try
                {
                    await _store.CreateAsync(token, CancellationToken.None);
                    await _store.SetPayloadAsync(token, "synthetic-" + i, CancellationToken.None);
                    try
                    {
                        await _store.UpdateAsync(token, CancellationToken.None);
                    }
                    catch (ContentModifiedException ex)
                    {
                        Assert.Fail($"Unexpected fresh-token CAS conflict on attempt {i + 1} of {attempts}, " +
                            $"expected version 0. {ex.GetType().Name}");
                    }

                    Assert.That(token.Version, Is.EqualTo(1));
                }
                finally
                {
                    await _store.DeleteAsync(token, CancellationToken.None);
                }
            }
        }

        private static OpenIddictProtocolToken NewToken()
            => new OpenIddictProtocolToken
            {
                Id = Guid.NewGuid().ToString("N"),
                ApplicationId = "oauth-repro-synthetic-client",
                Subject = "oauth-repro-synthetic-subject",
                Type = "authorization_code",
                Status = "valid",
                CreationDateUtc = DateTimeOffset.UtcNow.ToString("O"),
                ExpirationDateUtc = DateTimeOffset.UtcNow.AddMinutes(5).ToString("O")
            };
    }
}
