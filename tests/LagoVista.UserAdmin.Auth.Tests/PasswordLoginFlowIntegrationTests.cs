using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetSignInManager = Microsoft.AspNetCore.Identity.SignInManager<LagoVista.UserAdmin.Models.Users.AppUser>;
using LagoVistaSignInManager = LagoVista.AspNetCore.Identity.Managers.SignInManager;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class PasswordLoginFlowIntegrationTests
    {
        private const string PasswordLoginEvidence = "auth|auth.test-binding.password.establish-session|auth.flow.password.establish-session|auth.transition.password.establish-session";
        private const string UserNotFoundEvents = "PasswordAuthenticationStarted|PasswordAuthUserNotFound";
        private const string InvalidCredentialsEvents = "PasswordAuthenticationStarted|PasswordAuthenticationFailed";
        private const string SuccessfulLoginEvents = "PasswordAuthenticationStarted|PasswordAuthenticationSucceeded";

        [Test]
        [Property("AptixEvidence", PasswordLoginEvidence)]
        [Property("AptixAuthEvents", UserNotFoundEvents)]
        public async Task UserNotFound_Should_ReturnFailure_And_RecordStartedThenUserNotFound()
        {
            var harness = CreateHarness();
            harness.UserManager.Setup(manager => manager.FindByNameAsync("missing@example.com")).ReturnsAsync((AppUser)null);

            var result = await harness.FlowService.LoginWithPasswordAsync(CreateRequest("missing@example.com"));

            Assert.That(result.Successful, Is.False);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.PasswordAuthenticationStarted,
                AuthLogTypes.PasswordAuthUserNotFound
            }));
            Assert.That(harness.Log.Events.Last().UserName, Is.EqualTo("missing@example.com"));
        }

        [Test]
        [Property("AptixEvidence", PasswordLoginEvidence)]
        [Property("AptixAuthEvents", InvalidCredentialsEvents)]
        public async Task InvalidCredentials_Should_ReturnFailure_And_RecordStartedThenFailed()
        {
            var harness = CreateHarness();
            var user = new AppUser("user@example.com", "test") { UserName = "user@example.com" };

            harness.UserManager.Setup(manager => manager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            harness.AspNetSignInManager
                .Setup(manager => manager.PasswordSignInAsync("user@example.com", "wrong-password", true, false))
                .ReturnsAsync(SignInResult.Failed);

            var result = await harness.FlowService.LoginWithPasswordAsync(CreateRequest("user@example.com", "wrong-password"));

            Assert.That(result.Successful, Is.False);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.PasswordAuthenticationStarted,
                AuthLogTypes.PasswordAuthenticationFailed
            }));
            Assert.That(harness.Log.Events.Last().Errors, Is.EqualTo("Likely invalid credentials."));
        }

        [Test]
        [Property("AptixEvidence", PasswordLoginEvidence)]
        [Property("AptixAuthEvents", SuccessfulLoginEvents)]
        public async Task SuccessfulLogin_Should_ReturnAuthenticated_And_RecordStartedThenSucceeded()
        {
            var harness = CreateHarness();
            var user = new AppUser("user@example.com", "test") { UserName = "user@example.com" };

            harness.UserManager.Setup(manager => manager.FindByNameAsync("user@example.com")).ReturnsAsync(user);
            harness.AspNetSignInManager
                .Setup(manager => manager.PasswordSignInAsync("user@example.com", "correct-password", true, false))
                .ReturnsAsync(SignInResult.Success);
            harness.AppUserRepo.Setup(repo => repo.UpdateAsync(user)).Returns(Task.CompletedTask);
            harness.RedirectServices.Setup(service => service.IdentityDefaultRedirectAsync(user, null)).ReturnsAsync(InvokeResult<string>.Create("/home"));

            var result = await harness.FlowService.LoginWithPasswordAsync(CreateRequest("user@example.com", "correct-password"));

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.Authenticated));
            Assert.That(result.Result.RedirectPage, Is.EqualTo("/home"));
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.PasswordAuthenticationStarted,
                AuthLogTypes.PasswordAuthenticationSucceeded
            }));
            Assert.That(harness.Log.Events.Last().UserId, Is.EqualTo(user.Id.Value));
        }

        private static AuthLoginRequest CreateRequest(string email, string password = "password")
        {
            return new AuthLoginRequest
            {
                Email = email,
                Password = password,
                RememberMe = true,
                LockoutOnFailure = false
            };
        }

        private static PasswordLoginHarness CreateHarness()
        {
            var log = new RecordingAuthenticationLogManager();
            var aspNetSignInManager = CreateAspNetSignInManager();
            var userManager = new Mock<IUserManager>(MockBehavior.Strict);
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Strict);
            var redirectServices = new Mock<IUserRedirectServices>(MockBehavior.Strict);

            var manager = new LagoVistaSignInManager(
                new Mock<IAdminLogger>().Object,
                new Mock<IDefaultRoleList>().Object,
                new Mock<IUserRoleManager>().Object,
                new Mock<IDependencyManager>().Object,
                new Mock<IOrgUserRepo>().Object,
                new Mock<IUserFavoritesManager>().Object,
                new Mock<IMostRecentlyUsedManager>().Object,
                appUserRepo.Object,
                redirectServices.Object,
                log,
                new Mock<ISecurity>().Object,
                new Mock<IAppConfig>().Object,
                userManager.Object,
                new Mock<IOrganizationManager>().Object,
                new Mock<IOrganizationRepo>().Object,
                aspNetSignInManager.Object);

            var handler = new PasswordLoginFlowHandler(manager);
            var recoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);

            return new PasswordLoginHarness
            {
                Log = log,
                UserManager = userManager,
                AppUserRepo = appUserRepo,
                RedirectServices = redirectServices,
                AspNetSignInManager = aspNetSignInManager,
                FlowService = new AuthenticationFlowService(handler, recoveryHandler.Object)
            };
        }

        private static Mock<AspNetSignInManager> CreateAspNetSignInManager()
        {
            var userStore = new Mock<IUserStore<AppUser>>();
            var identityOptions = Options.Create(new IdentityOptions());
            var aspNetUserManager = new Mock<UserManager<AppUser>>(
                userStore.Object,
                identityOptions,
                new Mock<IPasswordHasher<AppUser>>().Object,
                Array.Empty<IUserValidator<AppUser>>(),
                Array.Empty<IPasswordValidator<AppUser>>(),
                new Mock<ILookupNormalizer>().Object,
                new IdentityErrorDescriber(),
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<AppUser>>>().Object);

            return new Mock<AspNetSignInManager>(
                aspNetUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
                identityOptions,
                new Mock<ILogger<AspNetSignInManager>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<AppUser>>().Object);
        }

        private sealed class PasswordLoginHarness
        {
            public RecordingAuthenticationLogManager Log { get; set; }
            public Mock<IUserManager> UserManager { get; set; }
            public Mock<IAppUserRepo> AppUserRepo { get; set; }
            public Mock<IUserRedirectServices> RedirectServices { get; set; }
            public Mock<AspNetSignInManager> AspNetSignInManager { get; set; }
            public AuthenticationFlowService FlowService { get; set; }
        }
    }
}
