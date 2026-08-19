using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class TotpManagementFlowIntegrationTests
    {
        private const string TurnOffEvidence = "auth|auth.test-binding.totp-management.maintenance|auth.flow.totp-management.turn-off|auth.transition.totp-management.disable-success";
        private const string RotateEvidence = "auth|auth.test-binding.totp-management.maintenance|auth.flow.totp-management.rotate-recovery-codes|auth.transition.totp-management.rotate-recovery-codes-success";
        private const string TestUserId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string TestOrganizationId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";

        [Test]
        [Property("AptixEvidence", TurnOffEvidence)]
        [Property("AptixAuthEvents", "TotpDisableMfaStart,TotpDisableMfaSuccess")]
        public async Task TurnOff_Should_RunRealFlowHandlerAndMfaManager_AndRemoveTotpMaterial()
        {
            var harness = CreateHarness();

            harness.User.TwoFactorEnabled = true;
            harness.User.AuthenticatorKeySecretId = "auth-secret";
            harness.User.RecoveryCodesSecretId = "recovery-secret";
            harness.User.LastMfaDateTimeUtc = "2026-08-19T12:00:00Z";
            harness.User.LastTotpAcceptedTimeStep = 12345;

            harness.SecureStorage.Setup(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), "auth-secret")).ReturnsAsync(InvokeResult.Success);
            harness.SecureStorage.Setup(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), "recovery-secret")).ReturnsAsync(InvokeResult.Success);

            var result = await harness.FlowService.TurnOffTotpAsync(harness.User.Id, harness.Organization, harness.UserHeader);

            Assert.That(result.Successful, Is.True);
            Assert.That(harness.User.TwoFactorEnabled, Is.False);
            Assert.That(harness.User.AuthenticatorKeySecretId, Is.Null);
            Assert.That(harness.User.RecoveryCodesSecretId, Is.Null);
            Assert.That(harness.User.LastMfaDateTimeUtc, Is.Null);
            Assert.That(harness.User.LastTotpAcceptedTimeStep, Is.EqualTo(0));
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(harness.User), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[] { AuthLogTypes.TotpDisableMfaStart, AuthLogTypes.TotpDisableMfaSuccess }));
        }

        [Test]
        [Property("AptixEvidence", RotateEvidence)]
        [Property("AptixAuthEvents", "TotpRotateRecoveryCodesStart,TotpRotateRecoveryCodesSuccess")]
        public async Task RotateRecoveryCodes_Should_RunRealFlowHandlerAndMfaManager_AndKeepTotpEnabled()
        {
            var harness = CreateHarness();

            harness.User.TwoFactorEnabled = true;
            harness.User.AuthenticatorKeySecretId = "auth-secret";
            harness.User.RecoveryCodesSecretId = "old-recovery-secret";

            harness.SecureStorage.Setup(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), "old-recovery-secret")).ReturnsAsync(InvokeResult.Success);
            harness.SecureStorage.Setup(storage => storage.AddUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>())).ReturnsAsync(InvokeResult<string>.Create("new-recovery-secret"));

            var result = await harness.FlowService.RotateTotpRecoveryCodesAsync(harness.User.Id, harness.Organization, harness.UserHeader);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Has.Count.EqualTo(10));
            Assert.That(result.Result.Distinct().Count(), Is.EqualTo(10));
            Assert.That(harness.User.TwoFactorEnabled, Is.True);
            Assert.That(harness.User.AuthenticatorKeySecretId, Is.EqualTo("auth-secret"));
            Assert.That(harness.User.RecoveryCodesSecretId, Is.EqualTo("new-recovery-secret"));
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(harness.User), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[] { AuthLogTypes.TotpRotateRecoveryCodesStart, AuthLogTypes.TotpRotateRecoveryCodesSuccess }));
        }

        [Test]
        public async Task RotateRecoveryCodes_WhenTotpIsNotEnabled_ShouldRejectBeforeMutation()
        {
            var harness = CreateHarness();
            harness.User.TwoFactorEnabled = false;
            harness.User.AuthenticatorKeySecretId = null;

            var result = await harness.FlowService.RotateTotpRecoveryCodesAsync(harness.User.Id, harness.Organization, harness.UserHeader);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("mfa_not_enabled"));
            harness.SecureStorage.Verify(storage => storage.AddUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>()), Times.Never);
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
            Assert.That(harness.Log.Events, Is.Empty);
        }

        private static TotpManagementHarness CreateHarness()
        {
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Loose);
            var secureStorage = new Mock<ISecureStorage>(MockBehavior.Strict);
            var log = new RecordingAuthenticationLogManager();
            var logger = new Mock<IAdminLogger>(MockBehavior.Loose);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Loose);
            var dependencyManager = new Mock<IDependencyManager>(MockBehavior.Loose);
            var security = new Mock<ISecurity>(MockBehavior.Loose);

            var user = new AppUser("user@example.com", "test")
            {
                Id = TestUserId,
                UserName = "user@example.com",
                Email = "user@example.com"
            };
            var organization = EntityHeader.Create(TestOrganizationId, "Organization");
            var userHeader = EntityHeader.Create(user.Id, "User");

            appUserRepo.Setup(repo => repo.FindByIdAsync(user.Id)).ReturnsAsync(user);
            appUserRepo.Setup(repo => repo.UpdateAsync(It.IsAny<AppUser>())).Returns(Task.CompletedTask);

            var mfaManager = new AppUserMfaManager(appUserRepo.Object, secureStorage.Object, log, logger.Object, appConfig.Object, dependencyManager.Object, security.Object);
            var turnOffHandler = new TotpTurnOffFlowHandler(mfaManager, appUserRepo.Object);
            var rotationHandler = new TotpRecoveryCodeRotationFlowHandler(mfaManager, appUserRepo.Object);
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var flowService = new AuthenticationFlowService(passwordLoginHandler.Object, recoveryHandler.Object, totpTurnOffHandler: turnOffHandler, totpRecoveryCodeRotationHandler: rotationHandler);

            return new TotpManagementHarness
            {
                FlowService = flowService,
                AppUserRepo = appUserRepo,
                SecureStorage = secureStorage,
                Log = log,
                User = user,
                Organization = organization,
                UserHeader = userHeader
            };
        }

        private sealed class TotpManagementHarness
        {
            public AuthenticationFlowService FlowService { get; set; }
            public Mock<IAppUserRepo> AppUserRepo { get; set; }
            public Mock<ISecureStorage> SecureStorage { get; set; }
            public RecordingAuthenticationLogManager Log { get; set; }
            public AppUser User { get; set; }
            public EntityHeader Organization { get; set; }
            public EntityHeader UserHeader { get; set; }
        }
    }
}
