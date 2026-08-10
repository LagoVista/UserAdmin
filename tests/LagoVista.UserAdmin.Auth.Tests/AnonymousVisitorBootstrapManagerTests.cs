using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
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
        private Mock<IAnonymousVisitorRepo> _visitorRepo;
        private Mock<IAnonymousVisitorBootstrapOptions> _options;
        private Mock<ITokenAuthOptions> _tokenOptions;
        private Mock<IAnonymousVisitorTokenService> _tokenService;

        [SetUp]
        public void Setup()
        {
            _visitorRepo = new Mock<IAnonymousVisitorRepo>();
            _options = new Mock<IAnonymousVisitorBootstrapOptions>();
            _tokenOptions = new Mock<ITokenAuthOptions>();
            _tokenService = new Mock<IAnonymousVisitorTokenService>();

            _options.SetupGet(item => item.ActiveLifetime).Returns(TimeSpan.FromHours(24));
            _tokenOptions.SetupGet(item => item.AccessExpiration).Returns(TimeSpan.FromMinutes(15));
            _tokenService.Setup(service => service.CreateToken(It.IsAny<string>(), It.IsAny<DateTime>())).Returns("visitor-access-token");
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
            Assert.That(result.Result.IdentityStage, Is.EqualTo(ClaimsFactory.VisitorIdentityStage));
            Assert.That(result.Result.AccessToken, Is.EqualTo("visitor-access-token"));
            Assert.That(result.Result.WasRestored, Is.False);
            Assert.That(result.Result.VisitorExpiresUtc, Is.GreaterThan(DateTime.UtcNow.AddHours(23)));

            _tokenService.Verify(service => service.CreateToken(createdVisitor.ActorId, It.IsAny<DateTime>()), Times.Once);
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
            _tokenService.Verify(service => service.CreateToken(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
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
            _tokenService.Verify(service => service.CreateToken(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        private AnonymousVisitorBootstrapManager CreateManager()
        {
            return new AnonymousVisitorBootstrapManager(_visitorRepo.Object, _options.Object, _tokenOptions.Object, _tokenService.Object);
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
