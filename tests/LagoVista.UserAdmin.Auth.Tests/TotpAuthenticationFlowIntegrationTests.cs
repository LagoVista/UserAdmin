using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
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
using OtpNet;
using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetSignInManager = Microsoft.AspNetCore.Identity.SignInManager<LagoVista.UserAdmin.Models.Users.AppUser>;
using LagoVistaSignInManager = LagoVista.AspNetCore.Identity.Managers.SignInManager;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class TotpAuthenticationFlowIntegrationTests
    {
        private const string SuccessEvidence = "auth|auth.test-binding.totp-sign-in|auth.flow.totp-sign-in|auth.transition.totp-sign-in.success";
        private const string RejectedEvidence = "auth|auth.test-binding.totp-sign-in|auth.flow.totp-sign-in|auth.transition.totp-sign-in.rejected";
        private const string UserId = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private const string OrgId = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";
        private const string ChallengeId = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";
        private const string Secret = "JBSWY3DPEHPK3PXP";

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        [Property("AptixAuthEvents", "TotpVerifyStart|TotpVerifySuccess|PasswordAuthenticationSucceeded")]
        public async Task ValidTotp_WithPasswordIssuedChallenge_Should_EstablishSession_AndConsumeChallenge()
        {
            var harness = CreateHarness();
            var code = new Totp(Base32Encoding.ToBytes(Secret), step: 30, totpSize: 6).ComputeTotp();

            harness.AppUserRepo.Setup(repo => repo.TryAcceptTotpTimeStepAsync(UserId, It.IsAny<long>(), true, It.IsAny<string>()))
                .ReturnsAsync(InvokeResult<long>.Create(1));
            harness.AspNetSignInManager.Setup(manager => manager.SignInAsync(harness.User, true, null)).Returns(Task.CompletedTask);
            harness.AppUserRepo.Setup(repo => repo.UpdateAsync(harness.User)).Returns(Task.CompletedTask);
            harness.RedirectServices.Setup(service => service.IdentityDefaultRedirectAsync(harness.User, null)).ReturnsAsync(InvokeResult<string>.Create("/home"));

            var result = await harness.FlowService.AuthenticateWithTotpAsync(new TotpSignInRequest
            {
                Email = harness.User.Email,
                Totp = code,
                MfaChallengeId = ChallengeId,
                RememberMe = true
            });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.Authenticated));
            Assert.That(result.Result.RedirectPage, Is.EqualTo("/home"));
            Assert.That(harness.User.TwoFactorEnabled, Is.True);
            Assert.That(harness.User.LastLogin, Is.Not.Null.And.Not.Empty);
            harness.AspNetSignInManager.Verify(manager => manager.SignInAsync(harness.User, true, null), Times.Once);
            harness.AppUserRepo.Verify(repo => repo.TryAcceptTotpTimeStepAsync(UserId, It.IsAny<long>(), true, It.IsAny<string>()), Times.Once);
            harness.MfaChallengeStore.Verify(store => store.ConsumeAsync(ChallengeId), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpVerifyStart,
                AuthLogTypes.TotpVerifySuccess,
                AuthLogTypes.PasswordAuthenticationSucceeded
            }));
        }

        [Test]
        [Property("AptixEvidence", RejectedEvidence)]
        public async Task Totp_WithoutPasswordIssuedChallenge_Should_RejectWithoutEvaluatingFactor()
        {
            var harness = CreateHarness();
            var code = new Totp(Base32Encoding.ToBytes(Secret), step: 30, totpSize: 6).ComputeTotp();

            var result = await harness.FlowService.AuthenticateWithTotpAsync(new TotpSignInRequest
            {
                Email = harness.User.Email,
                Totp = code,
                RememberMe = true
            });

            Assert.That(result.Successful, Is.False);
            harness.MfaChallengeStore.Verify(store => store.GetAsync(It.IsAny<string>()), Times.Never);
            harness.AppUserRepo.Verify(repo => repo.TryAcceptTotpTimeStepAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
            harness.AspNetSignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
            Assert.That(harness.Log.Events, Is.Empty);
        }

        [Test]
        [Property("AptixEvidence", RejectedEvidence)]
        [Property("AptixAuthEvents", "TotpVerifyStart|TotpVerifyFailed")]
        public async Task InvalidTotp_WithValidChallenge_Should_RejectWithoutConsumingChallengeOrSession()
        {
            var harness = CreateHarness();
            var validCode = new Totp(Base32Encoding.ToBytes(Secret), step: 30, totpSize: 6).ComputeTotp();
            var invalidCode = (validCode[0] == '0' ? "1" : "0") + validCode.Substring(1);

            var result = await harness.FlowService.AuthenticateWithTotpAsync(new TotpSignInRequest
            {
                Email = harness.User.Email,
                Totp = invalidCode,
                MfaChallengeId = ChallengeId,
                RememberMe = true
            });

            Assert.That(result.Successful, Is.False);
            harness.MfaChallengeStore.Verify(store => store.ConsumeAsync(It.IsAny<string>()), Times.Never);
            harness.AppUserRepo.Verify(repo => repo.TryAcceptTotpTimeStepAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
            harness.AspNetSignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpVerifyStart,
                AuthLogTypes.TotpVerifyFailed
            }));
        }

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        [Property("AptixAuthEvents", "TotpVerifyStart|TotpVerifySuccess")]
        public async Task ValidTotpToken_WithPasswordIssuedChallenge_Should_ProveMfaBeforeIssuingTokens()
        {
            var harness = CreateHarness();
            var code = new Totp(Base32Encoding.ToBytes(Secret), step: 30, totpSize: 6).ComputeTotp();

            harness.AppUserRepo.Setup(repo => repo.TryAcceptTotpTimeStepAsync(UserId, It.IsAny<long>(), true, It.IsAny<string>()))
                .ReturnsAsync(InvokeResult<long>.Create(1));
            harness.AuthTokenManager.Setup(manager => manager.GenerateOneTimeUseTokenAsync(UserId, null))
                .ReturnsAsync(InvokeResult<SingleUseToken>.Create(new SingleUseToken
                {
                    UserId = UserId,
                    Token = "single-use-token",
                    Expires = DateTime.UtcNow.AddMinutes(5).ToString("O")
                }));
            harness.AuthTokenManager.Setup(manager => manager.SingleUseTokenGrantAsync(It.Is<AuthRequest>(request =>
                    request.GrantType == "single-use-token" &&
                    request.UserId == UserId &&
                    request.SingleUseToken == "single-use-token")))
                .ReturnsAsync(InvokeResult<AuthResponse>.Create(new AuthResponse
                {
                    AccessToken = "access-token",
                    RefreshToken = "refresh-token"
                }));

            var authRequest = new AuthRequest
            {
                AppId = "test-app",
                AppInstanceId = "test-instance",
                Email = harness.User.Email,
                UserName = harness.User.UserName
            };

            var result = await harness.FlowService.AuthenticateWithTotpTokenAsync(new TotpTokenSignInRequest
            {
                MfaChallengeId = ChallengeId,
                Totp = code,
                Auth = authRequest
            });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.AccessToken, Is.EqualTo("access-token"));
            Assert.That(result.Result.RefreshToken, Is.EqualTo("refresh-token"));
            harness.AppUserRepo.Verify(repo => repo.TryAcceptTotpTimeStepAsync(UserId, It.IsAny<long>(), true, It.IsAny<string>()), Times.Once);
            harness.MfaChallengeStore.Verify(store => store.ConsumeAsync(ChallengeId), Times.Once);
            harness.AuthTokenManager.Verify(manager => manager.GenerateOneTimeUseTokenAsync(UserId, null), Times.Once);
            harness.AuthTokenManager.Verify(manager => manager.SingleUseTokenGrantAsync(It.Is<AuthRequest>(tokenRequest =>
                tokenRequest.GrantType == "single-use-token" &&
                tokenRequest.UserId == UserId &&
                tokenRequest.SingleUseToken == "single-use-token")), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpVerifyStart,
                AuthLogTypes.TotpVerifySuccess
            }));
        }

        private static TotpAuthenticationHarness CreateHarness()
        {
            var log = new RecordingAuthenticationLogManager();
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Strict);
            var secureStorage = new Mock<ISecureStorage>(MockBehavior.Strict);
            var redirectServices = new Mock<IUserRedirectServices>(MockBehavior.Strict);
            var userManager = new Mock<IUserManager>(MockBehavior.Strict);
            var authTokenManager = new Mock<IAuthTokenManager>(MockBehavior.Strict);
            var mfaChallengeStore = new Mock<IMfaChallengeStore>(MockBehavior.Strict);
            var aspNetSignInManager = CreateAspNetSignInManager();
            var appConfig = new Mock<IAppConfig>(MockBehavior.Loose);
            var systemOrg = EntityHeader.Create(OrgId, "System");
            appConfig.SetupGet(config => config.SystemOwnerOrg).Returns(systemOrg);

            var user = new AppUser("user@example.com", "test")
            {
                Id = UserId,
                UserName = "user@example.com",
                Email = "user@example.com",
                EmailConfirmed = true,
                TwoFactorEnabled = true,
                AuthenticatorKeySecretId = "auth-secret"
            };

            var challenge = new MfaChallenge
            {
                Id = ChallengeId,
                UserId = UserId,
                Email = user.Email,
                AvailableProviders = new[] { "totp" },
                CreatedUtc = DateTime.UtcNow.ToString("O"),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(5).ToString("O")
            };

            appUserRepo.Setup(repo => repo.FindByIdAsync(UserId)).ReturnsAsync(user);
            secureStorage.Setup(storage => storage.GetUserSecretAsync(It.IsAny<EntityHeader>(), "auth-secret"))
                .ReturnsAsync(InvokeResult<string>.Create(Secret));
            mfaChallengeStore.Setup(store => store.GetAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(challenge));
            mfaChallengeStore.Setup(store => store.ConsumeAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(challenge));

            var mfaManager = new AppUserMfaManager(
                appUserRepo.Object,
                secureStorage.Object,
                log,
                new Mock<IAdminLogger>().Object,
                appConfig.Object,
                new Mock<IDependencyManager>().Object,
                new Mock<ISecurity>().Object);

            var signInManager = new LagoVistaSignInManager(
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
                appConfig.Object,
                userManager.Object,
                new Mock<IOrganizationManager>().Object,
                new Mock<IOrganizationRepo>().Object,
                aspNetSignInManager.Object);

            var handler = new TotpAuthenticationFlowHandler(appUserRepo.Object, mfaManager, appConfig.Object, mfaChallengeStore.Object);
            var passwordHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var flowService = new AuthenticationFlowService(
                passwordHandler.Object,
                recoveryHandler.Object,
                totpAuthenticationHandler: handler,
                signInManager: signInManager,
                authTokenManager: authTokenManager.Object);

            return new TotpAuthenticationHarness
            {
                FlowService = flowService,
                AppUserRepo = appUserRepo,
                RedirectServices = redirectServices,
                AuthTokenManager = authTokenManager,
                MfaChallengeStore = mfaChallengeStore,
                AspNetSignInManager = aspNetSignInManager,
                Log = log,
                User = user
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

        private sealed class TotpAuthenticationHarness
        {
            public AuthenticationFlowService FlowService { get; set; }
            public Mock<IAppUserRepo> AppUserRepo { get; set; }
            public Mock<IUserRedirectServices> RedirectServices { get; set; }
            public Mock<IAuthTokenManager> AuthTokenManager { get; set; }
            public Mock<IMfaChallengeStore> MfaChallengeStore { get; set; }
            public Mock<AspNetSignInManager> AspNetSignInManager { get; set; }
            public RecordingAuthenticationLogManager Log { get; set; }
            public AppUser User { get; set; }
        }
    }
}
