using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class ContinuitySessionManagerTests
    {
        [Test]
        public async Task ResolveAsync_Should_Return_Restored_Visitor()
        {
            var harness = CreateHarness();
            harness.VisitorManager.Setup(manager => manager.RestoreAsync(It.Is<AnonymousVisitorRestoreRequest>(request => request.ContinuityToken == "continuity-token"))).ReturnsAsync(InvokeResult<AnonymousVisitorBootstrapResponse>.Create(CreateVisitorResponse(true)));

            var result = await harness.Manager.ResolveAsync("continuity-token");

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.IdentityStage, Is.EqualTo(ClaimsFactory.VisitorIdentityStage));
            Assert.That(result.Result.ActorId, Is.EqualTo("visitor-actor"));
            Assert.That(result.Result.ContinuityToken, Is.EqualTo("rotated-visitor-token"));
            harness.ProvisionalManager.Verify(manager => manager.RestoreAsync(It.IsAny<RestoreProvisionalEnvironmentRequest>()), Times.Never);
        }

        [Test]
        public async Task ResolveAsync_Should_Return_Restored_Provisional_Session()
        {
            var harness = CreateHarness();
            var expiresUtc = DateTime.UtcNow.AddDays(30);
            harness.VisitorManager.Setup(manager => manager.RestoreAsync(It.IsAny<AnonymousVisitorRestoreRequest>())).ReturnsAsync(InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("not a visitor"));
            harness.ProvisionalManager.Setup(manager => manager.RestoreAsync(It.Is<RestoreProvisionalEnvironmentRequest>(request => request.RecoveryToken == "continuity-token"))).ReturnsAsync(InvokeResult<RestoreProvisionalEnvironmentResponse>.Create(new RestoreProvisionalEnvironmentResponse
            {
                ActorId = "journey-actor",
                ProvisionalEnvironmentId = "environment-id",
                AppUserId = "app-user-id",
                OrganizationId = "organization-id",
                SubscriptionId = "subscription-id",
                RecoveryToken = "rotated-provisional-token",
                ExpiresUtc = expiresUtc,
                BootstrapContext = "preserved context"
            }));
            harness.AppUserRepo.Setup(repo => repo.FindByIdAsync("app-user-id")).ReturnsAsync(new AppUser(null, "provisional-user", "test") { Id = "app-user-id" });
            harness.OrganizationRepo.Setup(repo => repo.GetOrganizationAsync("organization-id")).ReturnsAsync(new Organization { Id = "organization-id", Name = "Provisional", Namespace = "provisional" });
            harness.TokenOptions.SetupGet(options => options.AccessExpiration).Returns(TimeSpan.FromMinutes(15));
            harness.TokenHelper.Setup(helper => helper.GetProvisionalJWToken(It.IsAny<AppUser>(), "journey-actor", It.IsAny<DateTime>())).Returns("provisional-access-token");

            var result = await harness.Manager.ResolveAsync("continuity-token");

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.IdentityStage, Is.EqualTo(ClaimsFactory.ProvisionalIdentityStage));
            Assert.That(result.Result.ActorId, Is.EqualTo("journey-actor"));
            Assert.That(result.Result.AccessToken, Is.EqualTo("provisional-access-token"));
            Assert.That(result.Result.ContinuityToken, Is.EqualTo("rotated-provisional-token"));
            Assert.That(result.Result.BootstrapContext, Is.EqualTo("preserved context"));
        }

        [Test]
        public async Task ResolveAsync_Should_Create_Visitor_When_Credential_Is_Invalid()
        {
            var harness = CreateHarness();
            harness.VisitorManager.Setup(manager => manager.RestoreAsync(It.IsAny<AnonymousVisitorRestoreRequest>())).ReturnsAsync(InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("not a visitor"));
            harness.ProvisionalManager.Setup(manager => manager.RestoreAsync(It.IsAny<RestoreProvisionalEnvironmentRequest>())).ReturnsAsync(InvokeResult<RestoreProvisionalEnvironmentResponse>.FromError("not provisional"));
            harness.VisitorManager.Setup(manager => manager.BootstrapAsync(It.IsAny<AnonymousVisitorBootstrapRequest>())).ReturnsAsync(InvokeResult<AnonymousVisitorBootstrapResponse>.Create(CreateVisitorResponse(false)));

            var result = await harness.Manager.ResolveAsync("invalid-token");

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.IdentityStage, Is.EqualTo(ClaimsFactory.VisitorIdentityStage));
            Assert.That(result.Result.WasRestored, Is.False);
        }

        private static AnonymousVisitorBootstrapResponse CreateVisitorResponse(bool wasRestored)
        {
            return new AnonymousVisitorBootstrapResponse
            {
                ActorId = "visitor-actor",
                IdentityStage = ClaimsFactory.VisitorIdentityStage,
                AccessToken = "visitor-access-token",
                AccessTokenExpiresUtc = DateTime.UtcNow.AddMinutes(15),
                ContinuityToken = "rotated-visitor-token",
                VisitorExpiresUtc = DateTime.UtcNow.AddHours(24),
                WasRestored = wasRestored
            };
        }

        private static Harness CreateHarness()
        {
            var visitorManager = new Mock<IAnonymousVisitorBootstrapManager>();
            var provisionalManager = new Mock<IProvisionalEnvironmentManager>();
            var appUserRepo = new Mock<IAppUserLoaderRepo>();
            var organizationRepo = new Mock<IOrganizationLoaderRepo>();
            var tokenOptions = new Mock<ITokenAuthOptions>();
            var tokenHelper = new Mock<ITokenHelper>();
            var manager = new ContinuitySessionManager(visitorManager.Object, provisionalManager.Object, appUserRepo.Object, organizationRepo.Object, tokenOptions.Object, tokenHelper.Object);
            return new Harness(manager, visitorManager, provisionalManager, appUserRepo, organizationRepo, tokenOptions, tokenHelper);
        }

        private sealed class Harness
        {
            public Harness(ContinuitySessionManager manager, Mock<IAnonymousVisitorBootstrapManager> visitorManager, Mock<IProvisionalEnvironmentManager> provisionalManager, Mock<IAppUserLoaderRepo> appUserRepo, Mock<IOrganizationLoaderRepo> organizationRepo, Mock<ITokenAuthOptions> tokenOptions, Mock<ITokenHelper> tokenHelper)
            {
                Manager = manager;
                VisitorManager = visitorManager;
                ProvisionalManager = provisionalManager;
                AppUserRepo = appUserRepo;
                OrganizationRepo = organizationRepo;
                TokenOptions = tokenOptions;
                TokenHelper = tokenHelper;
            }

            public ContinuitySessionManager Manager { get; }
            public Mock<IAnonymousVisitorBootstrapManager> VisitorManager { get; }
            public Mock<IProvisionalEnvironmentManager> ProvisionalManager { get; }
            public Mock<IAppUserLoaderRepo> AppUserRepo { get; }
            public Mock<IOrganizationLoaderRepo> OrganizationRepo { get; }
            public Mock<ITokenAuthOptions> TokenOptions { get; }
            public Mock<ITokenHelper> TokenHelper { get; }
        }
    }
}
