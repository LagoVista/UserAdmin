using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class RecoveryCodeAuthenticationFlowIntegrationTests
    {
        private const string SuccessEvidence = "auth|auth.test-binding.totp-sign-in.recovery-code|auth.flow.totp-recovery-sign-in|auth.transition.totp-recovery-sign-in.success";
        private const string RejectedEvidence = "auth|auth.test-binding.totp-sign-in.recovery-code|auth.flow.totp-recovery-sign-in|auth.transition.totp-recovery-sign-in.rejected";
        private const string UserId = "F1111111111111111111111111111111";
        private const string OrgId = "F2222222222222222222222222222222";
        private const string ChallengeId = "F3333333333333333333333333333333";

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        [Property("AptixAuthEvents", "TotpConsumeRecoveryCodeStart|TotpConsumeRecoveryCodeSuccess")]
        public async Task ValidRecoveryCode_WithPasswordIssuedChallenge_Should_InvalidateAuthenticator_AndEstablishSession()
        {
            var harness = await CreateHarnessAsync();
            var code = harness.RecoveryCodes.First();

            var result = await harness.FlowService.AuthenticateWithRecoveryCodeAsync(new RecoveryCodeSignInRequest
            {
                Email = harness.User.Email,
                RecoveryCode = code,
                MfaChallengeId = ChallengeId,
                RememberMe = true
            });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.Authenticated));
            Assert.That(harness.User.TwoFactorEnabled, Is.False);
            Assert.That(harness.User.AuthenticatorKeySecretId, Is.Null);
            Assert.That(harness.User.RecoveryCodesSecretId, Is.EqualTo("recovery-secret-2"));
            Assert.That(harness.User.LastMfaDateTimeUtc, Is.Not.Null.And.Not.Empty);

            harness.MfaChallengeStore.Verify(store => store.ConsumeAsync(ChallengeId), Times.Once);
            harness.SecureStorage.Verify(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), "auth-secret"), Times.Once);
            harness.SignInManager.Verify(manager => manager.SignInAsync(harness.User, true), Times.Once);
            harness.SignInManager.Verify(manager => manager.CompleteSignInToAppAsync(harness.User, null, "", ""), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpConsumeRecoveryCodeStart,
                AuthLogTypes.TotpConsumeRecoveryCodeSuccess
            }));
        }

        [Test]
        [Property("AptixEvidence", RejectedEvidence)]
        public async Task RecoveryCode_WithoutPasswordIssuedChallenge_Should_RejectBeforeSecretAccessOrSession()
        {
            var harness = await CreateHarnessAsync();

            var result = await harness.FlowService.AuthenticateWithRecoveryCodeAsync(new RecoveryCodeSignInRequest
            {
                Email = harness.User.Email,
                RecoveryCode = harness.RecoveryCodes.First(),
                RememberMe = true
            });

            Assert.That(result.Successful, Is.False);
            harness.MfaChallengeStore.Verify(store => store.GetAsync(It.IsAny<string>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), It.IsAny<bool>()), Times.Never);
            Assert.That(harness.Log.Events, Is.Empty);
        }

        [Test]
        [Property("AptixEvidence", RejectedEvidence)]
        [Property("AptixAuthEvents", "TotpConsumeRecoveryCodeStart|TotpConsumeRecoveryCodeFailed")]
        public async Task InvalidRecoveryCode_WithValidChallenge_Should_RejectWithoutConsumingChallengeOrSession()
        {
            var harness = await CreateHarnessAsync();

            var result = await harness.FlowService.AuthenticateWithRecoveryCodeAsync(new RecoveryCodeSignInRequest
            {
                Email = harness.User.Email,
                RecoveryCode = "INVALID-RECOVERY-CODE",
                MfaChallengeId = ChallengeId,
                RememberMe = true
            });

            Assert.That(result.Successful, Is.False);
            harness.MfaChallengeStore.Verify(store => store.ConsumeAsync(It.IsAny<string>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), It.IsAny<bool>()), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpConsumeRecoveryCodeStart,
                AuthLogTypes.TotpConsumeRecoveryCodeFailed
            }));
        }

        private static async Task<RecoveryHarness> CreateHarnessAsync()
        {
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Loose);
            var secureStorage = new Mock<ISecureStorage>(MockBehavior.Strict);
            var mfaChallengeStore = new Mock<IMfaChallengeStore>(MockBehavior.Strict);
            var signInManager = new Mock<ISignInManager>(MockBehavior.Strict);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Loose);
            var mainLog = new RecordingAuthenticationLogManager();
            var setupLog = new RecordingAuthenticationLogManager();
            var logger = new Mock<IAdminLogger>(MockBehavior.Loose);
            var dependencyManager = new Mock<IDependencyManager>(MockBehavior.Loose);
            var security = new Mock<ISecurity>(MockBehavior.Loose);

            var organization = EntityHeader.Create(OrgId, "Organization");
            appConfig.SetupGet(config => config.SystemOwnerOrg).Returns(organization);

            var user = new AppUser("user@example.com", "test")
            {
                Id = UserId,
                UserName = "user@example.com",
                EmailConfirmed = true,
                TwoFactorEnabled = true,
                AuthenticatorKeySecretId = "auth-secret"
            };

            appUserRepo.Setup(repo => repo.FindByIdAsync(UserId)).ReturnsAsync(user);
            appUserRepo.Setup(repo => repo.UpdateAsync(user)).Returns(Task.CompletedTask);

            var storedRecoveryBlobs = new Dictionary<string, string>();
            var addCall = 0;
            secureStorage
                .Setup(storage => storage.AddUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>()))
                .ReturnsAsync((EntityHeader _, string value) =>
                {
                    addCall++;
                    var id = $"recovery-secret-{addCall}";
                    storedRecoveryBlobs[id] = value;
                    return InvokeResult<string>.Create(id);
                });
            secureStorage
                .Setup(storage => storage.GetUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>()))
                .ReturnsAsync((EntityHeader _, string id) => storedRecoveryBlobs.TryGetValue(id, out var value)
                    ? InvokeResult<string>.Create(value)
                    : InvokeResult<string>.FromError("secret_not_found"));
            secureStorage
                .Setup(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>()))
                .ReturnsAsync(InvokeResult.Success);

            var setupManager = new AppUserMfaManager(appUserRepo.Object, secureStorage.Object, setupLog, logger.Object, appConfig.Object, dependencyManager.Object, security.Object);
            var recoveryCodesResult = await setupManager.RotateRecoveryCodesAsync(UserId, organization, user.ToEntityHeader());
            Assert.That(recoveryCodesResult.Successful, Is.True, "Recovery-code test setup must use the real manager to issue codes.");

            var challenge = new MfaChallenge
            {
                Id = ChallengeId,
                UserId = UserId,
                Email = user.Email,
                AvailableProviders = new[] { "totp" },
                CreatedUtc = DateTime.UtcNow.ToString("O"),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(5).ToString("O")
            };
            mfaChallengeStore.Setup(store => store.GetAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(challenge));
            mfaChallengeStore.Setup(store => store.ConsumeAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(challenge));

            signInManager.Setup(manager => manager.SignInAsync(user, true)).Returns(Task.CompletedTask);
            signInManager.Setup(manager => manager.CompleteSignInToAppAsync(user, null, "", ""))
                .ReturnsAsync(InvokeResult<AuthenticationResponse>.Create(new AuthenticationResponse
                {
                    AuthenticationState = AuthenticationResponseState.Authenticated,
                    RedirectPage = "/home"
                }));

            var mfaManager = new AppUserMfaManager(appUserRepo.Object, secureStorage.Object, mainLog, logger.Object, appConfig.Object, dependencyManager.Object, security.Object);
            var recoveryHandler = new RecoveryCodeAuthenticationFlowHandler(appUserRepo.Object, mfaManager, appConfig.Object, mfaChallengeStore.Object);
            var passwordHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var passwordRecoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var flowService = new AuthenticationFlowService(
                passwordHandler.Object,
                passwordRecoveryHandler.Object,
                recoveryCodeAuthenticationHandler: recoveryHandler,
                signInManager: signInManager.Object);

            return new RecoveryHarness
            {
                FlowService = flowService,
                User = user,
                RecoveryCodes = recoveryCodesResult.Result,
                SecureStorage = secureStorage,
                MfaChallengeStore = mfaChallengeStore,
                SignInManager = signInManager,
                Log = mainLog
            };
        }

        private sealed class RecoveryHarness
        {
            public AuthenticationFlowService FlowService { get; set; }
            public AppUser User { get; set; }
            public List<string> RecoveryCodes { get; set; }
            public Mock<ISecureStorage> SecureStorage { get; set; }
            public Mock<IMfaChallengeStore> MfaChallengeStore { get; set; }
            public Mock<ISignInManager> SignInManager { get; set; }
            public RecordingAuthenticationLogManager Log { get; set; }
        }
    }
}
