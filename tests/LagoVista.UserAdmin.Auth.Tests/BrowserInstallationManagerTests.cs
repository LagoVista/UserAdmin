using LagoVista.Core;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class BrowserInstallationManagerTests
    {
        private const string User = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string OtherUser = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private const string App = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private const string OtherApp = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";
        private const string Browser = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";
        private const string OtherBrowser = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";

        private static AppUser Human(string id = User) => new AppUser { Id = new NormalizedId32(id), LoginType = LoginTypes.AppUser };

        private static (BrowserInstallationManager Manager, ConcurrentDictionary<(string, string), AppInstance> Records, Mock<IAppInstanceRepo> Repo) Harness()
        {
            var records = new ConcurrentDictionary<(string, string), AppInstance>();
            var repo = new Mock<IAppInstanceRepo>(MockBehavior.Strict);
            repo.Setup(x => x.GetAppInstanceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string user, string id) => records.TryGetValue((user, id), out var record) ? record : null);
            repo.Setup(x => x.AddAppInstanceAsync(It.IsAny<AppInstance>()))
                .Returns((AppInstance record) => records.TryAdd((record.PartitionKey, record.RowKey), record)
                    ? Task.CompletedTask : Task.FromException(new InvalidOperationException("Duplicate storage key.")));
            return (new BrowserInstallationManager(repo.Object), records, repo);
        }

        [Test]
        public async Task SameBrowserAndApplicationReusesCanonicalInstance()
        {
            var h = Harness();
            var first = await h.Manager.BindProvenUserAsync(Human(), App, Browser);
            var again = await h.Manager.BindProvenUserAsync(Human(), App.ToLowerInvariant(), Browser.ToLowerInvariant());
            Assert.That(again.RowKey, Is.EqualTo(first.RowKey));
            Assert.That(again.AppId, Is.EqualTo(App));
            Assert.That(again.DeviceId, Is.EqualTo(Browser));
            Assert.That(again.UserId, Is.EqualTo(User));
            Assert.That(again.ClientType, Is.EqualTo("browser"));
            Assert.That(h.Records.Count, Is.EqualTo(1));
            h.Repo.Verify(x => x.UpdateAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
        }

        [Test]
        public async Task DistinctProfilesAndApplicationsAndUsersAreIsolated()
        {
            var h = Harness();
            var first = await h.Manager.BindProvenUserAsync(Human(), App, Browser);
            var otherProfile = await h.Manager.BindProvenUserAsync(Human(), App, OtherBrowser);
            var otherApp = await h.Manager.BindProvenUserAsync(Human(), OtherApp, Browser);
            var otherUser = await h.Manager.BindProvenUserAsync(Human(OtherUser), App, Browser);
            Assert.That(otherProfile.RowKey, Is.Not.EqualTo(first.RowKey));
            Assert.That(otherApp.RowKey, Is.Not.EqualTo(first.RowKey));
            Assert.That(otherUser.PartitionKey, Is.Not.EqualTo(first.PartitionKey));
            Assert.That(h.Records.Count, Is.EqualTo(4));
        }

        [Test]
        public async Task ConcurrentInsertWinnerIsReusedWithoutOverwrite()
        {
            var h = Harness();
            var firstReads = 0;
            var bothRead = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            h.Repo.Setup(x => x.GetAppInstanceAsync(User, It.IsAny<string>()))
                .Returns(async (string user, string id) =>
                {
                    if (System.Threading.Interlocked.Increment(ref firstReads) <= 2)
                    {
                        if (firstReads >= 2) bothRead.TrySetResult(true);
                        await bothRead.Task;
                        return null;
                    }
                    return h.Records.TryGetValue((user, id), out var row) ? row : null;
                });
            var results = await Task.WhenAll(h.Manager.BindProvenUserAsync(Human(), App, Browser),
                h.Manager.BindProvenUserAsync(Human(), App, Browser));
            Assert.That(results.Select(r => r.RowKey).Distinct().Count(), Is.EqualTo(1));
            Assert.That(h.Records.Count, Is.EqualTo(1));
            h.Repo.Verify(x => x.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Exactly(2));
        }

        [TestCase("user")]
        [TestCase("app")]
        [TestCase("device")]
        [TestCase("client")]
        [TestCase("row")]
        [TestCase("partition")]
        public void ExistingMismatchedBindingIsRejected(string field)
        {
            var h = Harness();
            var id = BrowserInstallationManager.InstanceId(App, Browser);
            var record = new AppInstance(field == "row" ? OtherBrowser : id, field == "partition" ? OtherUser : User)
            {
                UserId = field == "user" ? OtherUser : User,
                AppId = field == "app" ? OtherApp : App,
                DeviceId = field == "device" ? OtherBrowser : Browser,
                ClientType = field == "client" ? "native" : "browser"
            };
            h.Records[(User, id)] = record;
            Assert.ThrowsAsync<InvalidOperationException>(async () => await h.Manager.BindProvenUserAsync(Human(), App, Browser));
            h.Repo.Verify(x => x.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
        }

        [TestCase("disabled")]
        [TestCase("deleted")]
        [TestCase("anonymous")]
        [TestCase("device")]
        [TestCase("kiosk")]
        public void RestrictedUserCannotBind(string kind)
        {
            var h = Harness();
            var user = Human();
            if (kind == "disabled") user.IsAccountDisabled = true;
            if (kind == "deleted") user.IsDeleted = true;
            if (kind == "anonymous") user.IsAnonymous = true;
            if (kind == "device") user.IsUserDevice = true;
            if (kind == "kiosk") user.LoginType = LoginTypes.Kiosk;
            Assert.ThrowsAsync<InvalidOperationException>(async () => await h.Manager.BindProvenUserAsync(user, App, Browser));
            h.Repo.VerifyNoOtherCalls();
        }

        [TestCase("browser")]
        [TestCase("")]
        [TestCase("00000000000000000000000000000000")]
        public void InvalidBrowserIdentifierRejected(string browser)
        {
            var h = Harness();
            Assert.ThrowsAsync<ArgumentException>(async () => await h.Manager.BindProvenUserAsync(Human(), App, browser));
            h.Repo.VerifyNoOtherCalls();
        }

        [Test]
        public void BrowserIdentifiersAreRandomAndCanonical()
        {
            var ids = Enumerable.Range(0, 50).Select(_ => BrowserInstallationManager.NewBrowserId()).ToArray();
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Length));
            Assert.That(ids.All(id => Guid.TryParseExact(id, "N", out _)), Is.True);
        }
    }
}
