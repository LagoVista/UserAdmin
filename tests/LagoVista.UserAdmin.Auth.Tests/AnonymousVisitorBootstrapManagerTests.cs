using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class AnonymousVisitorBootstrapManagerTests
    {
        private const string SharedAppUserId = "shared-app-user";
        private const string SharedOrganizationId = "shared-organization";

        private Mock<IAnonymousVisitorRepo> _visitorRepo;
        private Mock<IAppUserLoaderRepo> _appUserRepo;
        private Mock<IOrganizationLoaderRepo> _organizationRepo;
        private Mock<IAnonymousVisitorBootstrapOptions> _options;
        private Mock<ITokenAuthOptions> _tokenOptions;
        private Mock<ITokenHelper> _tokenHelper;

        [SetUp]
        public void Setup()
        {
            _visitorRepo = new Mock<IAnonymousVisitorRepo>();
            _appUserRepo = new Mock<IAppUserLoaderRepo>();
            _organizationRepo = new Mock<IOrganizationLoaderRepo>();
            _options = new Mock<IAnonymousVisitorBootstrapOptions>();
            _tokenOptions = new Mock<ITokenAuthOptions>();
            _tokenHelper = new Mock<ITokenHelper>();

            _options.SetupGet(item => item.AppUserId).Returns(SharedAppUserId);
            _options.SetupGet(item => item.OrganizationId).Returns(SharedOrganizationId);
            _options.SetupGet(item => item.ActiveLifetime).Returns(TimeSpan.FromHours(24));
            _tokenOptions.SetupGet(item => item.AccessExpiration).Returns(TimeSpan.FromMinutes(15));

            _appUserRepo.Setup(repo => repo.FindByIdAsync(SharedAppUserId)).ReturnsAsync(new AppUser("anonymous@system.local", "anonymous") { Id = SharedAppUserId });
            _organizationRepo.Setup(repo => repo.GetOrganizationAsync(SharedOrganizationId)).ReturnsAsync(new Organization { Id = SharedOrganizationId, Name = "Anonymous Visitors", Namespace = "anonymousvisitors" });
            _tokenHelper.Setup(helper => helper.GetAnonymousVisitorJWToken(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns("visitor-access-token");
        }

        [Test]
        public async Task BootstrapAsync_Should_Create_Visitor_And_Issue_Access_Token()
        {
            AnonymousVisitor createdVisitor = null;
            _visitorRepo.Setup(repo => repo.CreateAsync(It.IsAny<AnonymousVisitor>())).Callback<AnonymousVisitor>(visitor => createdVisitor = visitor).Returns(Task.CompletedTask);

            var result = await CreateManager().BootstrapAsync(new AnonymousVisitorBootstrapRequest { InstallationId = "installation-id", BootstrapContext = "initial context", AgentKey = "sales-agent" });

            Assert.That(result.Successful, Is.True);
            Assert.That(createdVisitor, Is.Not.Null);
            Assert.That(createdVisitor.ActorId, Is.EqualTo(result.Result.ActorId));
            Assert.That(createdVisitor.ContinuityTokenHash, Is.EqualTo(Hash(result.Result.ContinuityToken)));
            Assert.That(createdVisitor.InstallationIdHash, Is.Null);
            Assert.That(createdVisitor.BootstrapContext, Is.EqualTo("initial context"));
            Assert.That(createdVisitor.AgentKey, Is.EqualTo("sales-agent"));
            Assert.That(result.Result.IdentityStage, Is.EqualTo("visitor"));
            Assert.That(result.Result.AccessToken, Is.EqualTo("visitor-access-token"));
            Assert.That(result.Result.WasRestored, Is.False);
            Assert.That(result.Result.VisitorExpiresUtc, Is.GreaterThan(DateTime.UtcNow.AddHours(23)));

            _tokenHelper.Verify(helper => helper.GetAnonymousVisitorJWToken(It.Is<AppUser>(user => user.Id == SharedAppUserId && user.CurrentOrganization.Id == SharedOrganizationId), createdVisitor.ActorId, It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public async Task RestoreAsync_Should_Rotate_Continuity_Token_And_Extend_Activity()
        {
            var originalHash = Hash("continuity-token");
            var visitor = CreateActiveVisitor(originalHash);
            AnonymousVisitor updatedVisitor = null;
            _visitorRepo.Setup(repo => repo.FindByContinuityTokenHashAsync(originalHash)).ReturnsAsync(visitor);
            _visitorRepo.Setup(repo => repo.UpdateAsync(visitor)).Callback<AnonymousVisitor>(item => updatedVisitor = item).Returns(Task.CompletedTask);

            var result = await CreateManager().RestoreAsync(new AnonymousVisitorRestoreRequest { ContinuityToken = "continuity-token" });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.ActorId, Is.EqualTo(visitor.ActorId));
            Assert.That(result.Result.WasRestored, Is.True);
            Assert.That(result.Result.ContinuityToken, Is.Not.EqualTo("continuity-token"));
            Assert.That(updatedVisitor.ContinuityTokenHash, Is.EqualTo(Hash(result.Result.ContinuityToken)));
            Assert.That(updatedVisitor.ContinuityTokenHash, Is.Not.EqualTo(originalHash));
            Assert.That(updatedVisitor.LastActivityUtc, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(updatedVisitor.ExpiresUtc, Is.GreaterThan(DateTime.UtcNow.AddHours(23)));
        }

        [Test]
        public async Task RestoreAsync_Should_Reject_InstallationId_Without_ContinuityToken()
        {
            var result = await CreateManager().RestoreAsync(new AnonymousVisitorRestoreRequest { InstallationId = "installation-id" });

            Assert.That(result.Successful, Is.False);
            _visitorRepo.Verify(repo => repo.FindByInstallationIdHashAsync(It.IsAny<string>()), Times.Never);
            _tokenHelper.Verify(helper => helper.GetAnonymousVisitorJWToken(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task RestoreAsync_Should_Expire_Visitor_Without_Issuing_A_Token()
        {
            var visitor = CreateActiveVisitor(Hash("continuity-token"));
            visitor.ExpiresUtc = DateTime.UtcNow.AddMinutes(-1);
            _visitorRepo.Setup(repo => repo.FindByContinuityTokenHashAsync(Hash("continuity-token"))).ReturnsAsync(visitor);
            _visitorRepo.Setup(repo => repo.UpdateAsync(visitor)).Returns(Task.CompletedTask);

            var result = await CreateManager().RestoreAsync(new AnonymousVisitorRestoreRequest { ContinuityToken = "continuity-token" });

            Assert.That(result.Successful, Is.False);
            Assert.That(visitor.State, Is.EqualTo(AnonymousVisitorState.Expired));
            Assert.That(visitor.ExpiredUtc.HasValue, Is.True);
            _tokenHelper.Verify(helper => helper.GetAnonymousVisitorJWToken(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        private AnonymousVisitorBootstrapManager CreateManager()
        {
            return new AnonymousVisitorBootstrapManager(_visitorRepo.Object, _appUserRepo.Object, _organizationRepo.Object, _options.Object, _tokenOptions.Object, _tokenHelper.Object);
        }

        private static AnonymousVisitor CreateActiveVisitor(string continuityTokenHash, string actorId = "visitor-actor")
        {
            var now = DateTime.UtcNow;
            return new AnonymousVisitor
            {
                ActorId = actorId,
                State = AnonymousVisitorState.Active,
                ContinuityTokenHash = continuityTokenHash,
                BootstrapContext = "preserved context",
                CreatedUtc = now.AddHours(-1),
                LastActivityUtc = now.AddMinutes(-5),
                ExpiresUtc = now.AddHours(1),
                StateChangedUtc = now.AddHours(-1)
            };
        }

        private static string Hash(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var item in bytes) builder.Append(item.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
