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
using OtpNet;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class TotpEnrollmentFlowIntegrationTests
    {
        private const string BeginEvidence = "auth|auth.test-binding.totp-enrollment.success|auth.flow.totp-enrollment.begin|auth.transition.totp-enrollment.begin";
        private const string ConfirmEvidence = "auth|auth.test-binding.totp-enrollment.success|auth.flow.totp-enrollment.confirm|auth.transition.totp-enrollment.success";
        private const string TestUserId = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private const string TestOrganizationId = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";

        [Test]
        [Property("AptixEvidence", BeginEvidence)]
        [Property("AptixAuthEvents", "TotpBeginEnrollmentStart,TotpBeginEnrollmentSuccess")]
        public async Task BeginEnrollment_Should_RunRealFlowHandlerAndMfaManager_AndStageAuthenticatorMaterial()
        {
            var harness = CreateHarness();
            harness.SecureStorage
                .Setup(storage => storage.AddUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>()))
                .ReturnsAsync(InvokeResult<string>.Create("auth-secret"));

            var result = await harness.FlowService.BeginTotpEnrollmentAsync(harness.User.Id, harness.Organization, harness.UserHeader);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.Result.Secret, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Result.OtpAuthUri, Does.StartWith("otpauth://totp/"));
            Assert.That(result.Result.OtpAuthUri, Does.Contain(result.Result.Secret));
            Assert.That(harness.User.AuthenticatorKeySecretId, Is.EqualTo("auth-secret"));
            Assert.That(harness.User.TwoFactorEnabled, Is.False);
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(harness.User), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpBeginEnrollmentStart,
                AuthLogTypes.TotpBeginEnrollmentSuccess
            }));
        }

        [Test]
        [Property("AptixEvidence", ConfirmEvidence)]
        [Property("AptixAuthEvents", "TotpConfirmEnrollmentStart,TotpConfirmEnrollmentSuccess")]
        public async Task ConfirmEnrollment_Should_RunRealFlowHandlerAndMfaManager_AndEnableTotp()
        {
            var harness = CreateHarness();
            harness.SecureStorage
                .SetupSequence(storage => storage.AddUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>()))
                .ReturnsAsync(InvokeResult<string>.Create("auth-secret"))
                .ReturnsAsync(InvokeResult<string>.Create("recovery-secret"));

            var beginResult = await harness.FlowService.BeginTotpEnrollmentAsync(harness.User.Id, harness.Organization, harness.UserHeader);
            Assert.That(beginResult.Successful, Is.True);

            harness.SecureStorage
                .Setup(storage => storage.GetUserSecretAsync(It.IsAny<EntityHeader>(), "auth-secret"))
                .ReturnsAsync(InvokeResult<string>.Create(beginResult.Result.Secret));
            harness.AppUserRepo
                .Setup(repo => repo.TryAcceptTotpTimeStepAsync(harness.User.Id, It.IsAny<long>(), true, It.IsAny<string>()))
                .ReturnsAsync(InvokeResult<long>.Create(1));

            var totp = new Totp(Base32Encoding.ToBytes(beginResult.Result.Secret), step: 30, totpSize: 6).ComputeTotp();
            var result = await harness.FlowService.ConfirmTotpEnrollmentAsync(harness.User.Id, totp, harness.Organization, harness.UserHeader);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Has.Count.EqualTo(10));
            Assert.That(result.Result.Distinct().Count(), Is.EqualTo(10));
            Assert.That(harness.User.AuthenticatorKeySecretId, Is.EqualTo("auth-secret"));
            Assert.That(harness.User.RecoveryCodesSecretId, Is.EqualTo("recovery-secret"));
            Assert.That(harness.User.TwoFactorEnabled, Is.True);
            Assert.That(harness.User.LastMfaDateTimeUtc, Is.Not.Null.And.Not.Empty);
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(harness.User), Times.Exactly(2));
            Assert.That(harness.Log.Events.Select(evt => evt.Type).Skip(2), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpConfirmEnrollmentStart,
                AuthLogTypes.TotpConfirmEnrollmentSuccess
            }));
        }

        private static TotpEnrollmentHarness CreateHarness()
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
                Email = "user@example.com",
                TwoFactorEnabled = false
            };
            var organization = EntityHeader.Create(TestOrganizationId, "Organization");
            var userHeader = EntityHeader.Create(user.Id, "User");

            appUserRepo.Setup(repo => repo.FindByIdAsync(user.Id)).ReturnsAsync(user);
            appUserRepo.Setup(repo => repo.UpdateAsync(It.IsAny<AppUser>())).Returns(Task.CompletedTask);

            var mfaManager = new AppUserMfaManager(appUserRepo.Object, secureStorage.Object, log, logger.Object, appConfig.Object, dependencyManager.Object, security.Object);
            var beginHandler = new TotpEnrollmentBeginFlowHandler(mfaManager);
            var confirmHandler = new TotpEnrollmentConfirmFlowHandler(mfaManager);
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var flowService = new AuthenticationFlowService(
                passwordLoginHandler.Object,
                recoveryHandler.Object,
                totpEnrollmentBeginHandler: beginHandler,
                totpEnrollmentConfirmHandler: confirmHandler);

            return new TotpEnrollmentHarness
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

        private sealed class TotpEnrollmentHarness
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
