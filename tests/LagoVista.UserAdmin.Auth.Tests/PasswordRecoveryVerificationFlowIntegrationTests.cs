using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class PasswordRecoveryVerificationFlowIntegrationTests
    {
        private const string RecoveryCodeAcceptedEvidence = "auth|auth.test-binding.recovery.verify|auth.flow.recovery.verify|auth.transition.password-recovery.code-accepted";
        private const string RecoveryCodeRejectedEvidence = "auth|auth.test-binding.recovery.verify|auth.flow.recovery.verify|auth.transition.password-recovery.rejected";
        private const string VerifiedEvents = "PasswordRecoveryCodeVerified";
        private const string RejectedEvents = "PasswordRecoveryCodeVerificationFailed";

        [Test]
        [Property("AptixEvidence", RecoveryCodeAcceptedEvidence)]
        [Property("AptixAuthEvents", VerifiedEvents)]
        public async Task ValidCode_Should_ConsumeCode_GenerateResetToken_And_RecordVerified()
        {
            const string recoveryCode = "123456";
            var harness = CreateHarness(recoveryCode);
            harness.UserManager.Setup(manager => manager.GeneratePasswordResetTokenAsync(harness.User)).ReturnsAsync("reset-token");

            var result = await harness.FlowService.VerifyPasswordRecoveryAsync(new VerifyPasswordResetCode { Email = harness.User.Email, Code = recoveryCode });

            Assert.That(result.Successful, Is.True);
            Assert.That(harness.ResetCode.ConsumedUtc.HasValue, Is.True);
            Assert.That(harness.ResetCode.AttemptCount, Is.EqualTo(0));
            harness.PasswordResetCodeRepo.Verify(repo => repo.UpdateAsync(harness.ResetCode), Times.Once);
            harness.UserManager.Verify(manager => manager.GeneratePasswordResetTokenAsync(harness.User), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[] { AuthLogTypes.PasswordRecoveryCodeVerified }));
        }

        [Test]
        [Property("AptixEvidence", RecoveryCodeRejectedEvidence)]
        [Property("AptixAuthEvents", RejectedEvents)]
        public async Task InvalidCode_Should_ReturnNeutralRejection_IncrementAttempt_And_RecordFailure()
        {
            var harness = CreateHarness("123456");

            var result = await harness.FlowService.VerifyPasswordRecoveryAsync(new VerifyPasswordResetCode { Email = harness.User.Email, Code = "654321" });

            Assert.That(result.Successful, Is.False);
            Assert.That(harness.ResetCode.AttemptCount, Is.EqualTo(1));
            Assert.That(harness.ResetCode.ConsumedUtc.HasValue, Is.False);
            harness.PasswordResetCodeRepo.Verify(repo => repo.UpdateAsync(harness.ResetCode), Times.Once);
            harness.UserManager.Verify(manager => manager.GeneratePasswordResetTokenAsync(It.IsAny<AppUser>()), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[] { AuthLogTypes.PasswordRecoveryCodeVerificationFailed }));
        }

        private static PasswordRecoveryVerificationHarness CreateHarness(string validCode)
        {
            var log = new RecordingAuthenticationLogManager();
            var user = new AppUser("user@example.com", "test") { UserName = "user@example.com", Email = "user@example.com", SecurityStamp = "security-stamp" };
            var resetCode = new PasswordResetCode
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                CodeHash = ComputeResetCodeHash(user.SecurityStamp, validCode),
                CreatedUtc = DateTime.UtcNow.AddMinutes(-1),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(9),
                AttemptCount = 0
            };

            var userManager = new Mock<IUserManager>(MockBehavior.Strict);
            var passwordResetCodeRepo = new Mock<IPasswordResetCodeRepo>(MockBehavior.Strict);
            userManager.Setup(manager => manager.FindByNameAsync(user.Email)).ReturnsAsync(user);
            passwordResetCodeRepo.Setup(repo => repo.GetLatestAsync(user.Id)).ReturnsAsync(resetCode);
            passwordResetCodeRepo.Setup(repo => repo.UpdateAsync(resetCode)).Returns(Task.CompletedTask);

            var manager = new PasswordManager(
                new Mock<IAuthRequestValidators>(MockBehavior.Loose).Object,
                userManager.Object,
                new Mock<IEmailSender>(MockBehavior.Loose).Object,
                passwordResetCodeRepo.Object,
                new Mock<IDependencyManager>(MockBehavior.Loose).Object,
                new Mock<ISecurity>(MockBehavior.Loose).Object,
                log,
                new Mock<IAdminLogger>(MockBehavior.Loose).Object,
                new Mock<IAppConfig>(MockBehavior.Loose).Object);

            var verificationHandler = new PasswordRecoveryVerificationFlowHandler(manager);
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryRequestHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);

            return new PasswordRecoveryVerificationHarness
            {
                Log = log,
                User = user,
                ResetCode = resetCode,
                UserManager = userManager,
                PasswordResetCodeRepo = passwordResetCodeRepo,
                FlowService = new AuthenticationFlowService(passwordLoginHandler.Object, recoveryRequestHandler.Object, passwordRecoveryVerificationHandler: verificationHandler)
            };
        }

        private static string ComputeResetCodeHash(string securityStamp, string code)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(securityStamp)))
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(code)));
        }

        private sealed class PasswordRecoveryVerificationHarness
        {
            public RecordingAuthenticationLogManager Log { get; set; }
            public AppUser User { get; set; }
            public PasswordResetCode ResetCode { get; set; }
            public Mock<IUserManager> UserManager { get; set; }
            public Mock<IPasswordResetCodeRepo> PasswordResetCodeRepo { get; set; }
            public AuthenticationFlowService FlowService { get; set; }
        }
    }
}
