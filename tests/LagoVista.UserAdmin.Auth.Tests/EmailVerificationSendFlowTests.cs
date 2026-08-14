using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces;
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
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class EmailVerificationSendFlowTests
    {
        private const string SentEvidence = "auth|auth.test-binding.email-verification.send-code|auth.flow.email-verification.send-code|auth.transition.email-verification.code-sent";
        private const string ResentEvidence = "auth|auth.test-binding.email-verification.send-code|auth.flow.email-verification.send-code|auth.transition.email-verification.code-resent";
        private const string ThrottledEvidence = "auth|auth.test-binding.email-verification.send-code|auth.flow.email-verification.send-code|auth.transition.email-verification.resend-throttled";
        private const string SuccessfulSendEvents = "SendingEmailConfirm|SendEmailConfirmSuccess";
        private const string UserId = "7F3A91C2D8E44B6FA1029C7D5E8B34A1";

        [Test]
        [Property("AptixEvidence", SentEvidence)]
        [Property("AptixAuthEvents", SuccessfulSendEvents)]
        public async Task NoRecentCode_Should_Send_And_ReturnCodeSentTransition()
        {
            var harness = CreateHarness();
            var user = CreateUser();
            EmailVerificationCode storedCode = null;
            string deliveredBody = null;

            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync((EmailVerificationCode)null);
            harness.EmailVerificationCodeRepo
                .Setup(repo => repo.StoreAsync(It.IsAny<EmailVerificationCode>()))
                .Callback<EmailVerificationCode>(code => storedCode = code)
                .Returns(Task.CompletedTask);
            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(user);
            harness.UserManager.Setup(manager => manager.UpdateAsync(user)).ReturnsAsync(InvokeResult.Success);
            harness.EmailSender
                .Setup(sender => sender.SendAsync(
                    user.Email,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<EntityHeader>(),
                    It.IsAny<EntityHeader>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Callback<string, string, string, EntityHeader, EntityHeader, string, string>((_, __, body, ___, ____, _____, ______) => deliveredBody = body)
                .ReturnsAsync(InvokeResult.Success);
            harness.SignInManager.Setup(manager => manager.RefreshUserLoginAsync(user)).Returns(Task.CompletedTask);

            var result = await harness.FlowService.SendEmailVerificationCodeAsync(user.ToEntityHeader());

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.Outcome, Is.EqualTo(EmailVerificationSendOutcome.Sent));
            Assert.That(result.Result.VerificationCode, Has.Length.EqualTo(6));
            Assert.That(result.Result.RetryAfterSeconds, Is.EqualTo(0));
            Assert.That(storedCode, Is.Not.Null);
            Assert.That(storedCode.UserId, Is.EqualTo(UserId));
            Assert.That(storedCode.CodeHash, Is.Not.Null.And.Not.Empty);
            Assert.That(storedCode.AttemptCount, Is.EqualTo(0));
            Assert.That(storedCode.ExpiresUtc, Is.GreaterThan(storedCode.CreatedUtc));
            Assert.That(deliveredBody, Does.Contain(result.Result.VerificationCode));
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.SendingEmailConfirm,
                AuthLogTypes.SendEmailConfirmSuccess
            }));
            harness.EmailVerificationCodeRepo.Verify(repo => repo.StoreAsync(It.IsAny<EmailVerificationCode>()), Times.Once);
            harness.EmailSender.Verify(sender => sender.SendAsync(user.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            harness.SignInManager.Verify(manager => manager.RefreshUserLoginAsync(user), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", ThrottledEvidence)]
        public async Task RecentCode_Should_Throttle_WithoutSendingAnotherCode()
        {
            var harness = CreateHarness();
            var latest = new EmailVerificationCode
            {
                Id = "verification-code-id",
                UserId = UserId,
                CreatedUtc = DateTime.UtcNow.AddSeconds(-10),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(9),
                CodeHash = "hash"
            };

            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync(latest);

            var result = await harness.FlowService.SendEmailVerificationCodeAsync(EntityHeader.Create(UserId, "Test User"));

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.Outcome, Is.EqualTo(EmailVerificationSendOutcome.Throttled));
            Assert.That(result.Result.RetryAfterSeconds, Is.GreaterThan(0).And.LessThanOrEqualTo(60));
            harness.UserManager.Verify(manager => manager.FindByIdAsync(It.IsAny<string>()), Times.Never);
            harness.EmailVerificationCodeRepo.Verify(repo => repo.StoreAsync(It.IsAny<EmailVerificationCode>()), Times.Never);
            harness.EmailSender.Verify(sender => sender.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.RefreshUserLoginAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", ResentEvidence)]
        [Property("AptixAuthEvents", SuccessfulSendEvents)]
        public async Task ExistingCodeAfterCooldown_Should_Resend_And_ReturnCodeResentTransition()
        {
            var harness = CreateHarness();
            var user = CreateUser();
            var latest = new EmailVerificationCode
            {
                Id = "verification-code-id",
                UserId = UserId,
                CreatedUtc = DateTime.UtcNow.AddSeconds(-90),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(8),
                CodeHash = "old-hash",
                AttemptCount = 1
            };
            EmailVerificationCode replacementCode = null;
            string deliveredBody = null;

            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync(latest);
            harness.EmailVerificationCodeRepo
                .Setup(repo => repo.StoreAsync(It.IsAny<EmailVerificationCode>()))
                .Callback<EmailVerificationCode>(code => replacementCode = code)
                .Returns(Task.CompletedTask);
            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(user);
            harness.UserManager.Setup(manager => manager.UpdateAsync(user)).ReturnsAsync(InvokeResult.Success);
            harness.EmailSender
                .Setup(sender => sender.SendAsync(
                    user.Email,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<EntityHeader>(),
                    It.IsAny<EntityHeader>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Callback<string, string, string, EntityHeader, EntityHeader, string, string>((_, __, body, ___, ____, _____, ______) => deliveredBody = body)
                .ReturnsAsync(InvokeResult.Success);
            harness.SignInManager.Setup(manager => manager.RefreshUserLoginAsync(user)).Returns(Task.CompletedTask);

            var result = await harness.FlowService.SendEmailVerificationCodeAsync(user.ToEntityHeader());

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.Outcome, Is.EqualTo(EmailVerificationSendOutcome.Resent));
            Assert.That(result.Result.VerificationCode, Has.Length.EqualTo(6));
            Assert.That(result.Result.RetryAfterSeconds, Is.EqualTo(0));
            Assert.That(replacementCode, Is.Not.Null);
            Assert.That(replacementCode.Id, Is.Not.EqualTo(latest.Id));
            Assert.That(replacementCode.UserId, Is.EqualTo(UserId));
            Assert.That(replacementCode.CodeHash, Is.Not.Null.And.Not.Empty.And.Not.EqualTo(latest.CodeHash));
            Assert.That(replacementCode.AttemptCount, Is.EqualTo(0));
            Assert.That(deliveredBody, Does.Contain(result.Result.VerificationCode));
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.SendingEmailConfirm,
                AuthLogTypes.SendEmailConfirmSuccess
            }));
            harness.EmailVerificationCodeRepo.Verify(repo => repo.StoreAsync(It.IsAny<EmailVerificationCode>()), Times.Once);
            harness.EmailSender.Verify(sender => sender.SendAsync(user.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            harness.SignInManager.Verify(manager => manager.RefreshUserLoginAsync(user), Times.Once);
        }

        private static EmailVerificationSendHarness CreateHarness()
        {
            var log = new RecordingAuthenticationLogManager();
            var userManager = new Mock<IUserManager>(MockBehavior.Strict);
            var emailVerificationCodeRepo = new Mock<IEmailVerificationCodeRepo>(MockBehavior.Strict);
            var emailSender = new Mock<IEmailSender>(MockBehavior.Loose);
            var signInManager = new Mock<ISignInManager>(MockBehavior.Strict);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Loose);

            appConfig.SetupGet(config => config.AppName).Returns("NuvIoT");
            appConfig.SetupGet(config => config.Environment).Returns(Environments.Development);
            appConfig.SetupGet(config => config.SystemOwnerOrg).Returns(EntityHeader.Create("system", "System"));
            appConfig.SetupGet(config => config.WebAddress).Returns("https://dev.nuviot.com");

            var manager = new UserVerficationManager(
                new Mock<IAdminLogger>(MockBehavior.Loose).Object,
                userManager.Object,
                appConfig.Object,
                new Mock<ISmsSender>(MockBehavior.Loose).Object,
                new Mock<IAppUserRepo>(MockBehavior.Loose).Object,
                log,
                new Mock<IOrganizationManager>(MockBehavior.Loose).Object,
                signInManager.Object,
                emailSender.Object,
                emailVerificationCodeRepo.Object,
                new Mock<IDependencyManager>(MockBehavior.Loose).Object,
                new Mock<ISecurity>(MockBehavior.Loose).Object);

            var sendHandler = new EmailVerificationSendFlowHandler(manager, emailVerificationCodeRepo.Object);
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryRequestHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);

            return new EmailVerificationSendHarness
            {
                Log = log,
                UserManager = userManager,
                EmailVerificationCodeRepo = emailVerificationCodeRepo,
                EmailSender = emailSender,
                SignInManager = signInManager,
                FlowService = new AuthenticationFlowService(
                    passwordLoginHandler.Object,
                    recoveryRequestHandler.Object,
                    emailVerificationSendHandler: sendHandler)
            };
        }

        private static AppUser CreateUser()
        {
            return new AppUser("user@example.com", "test")
            {
                Id = UserId,
                UserName = "user@example.com",
                Email = "user@example.com",
                SecurityStamp = "email-verification-security-stamp",
                EmailConfirmed = false
            };
        }

        private sealed class EmailVerificationSendHarness
        {
            public RecordingAuthenticationLogManager Log { get; set; }
            public Mock<IUserManager> UserManager { get; set; }
            public Mock<IEmailVerificationCodeRepo> EmailVerificationCodeRepo { get; set; }
            public Mock<IEmailSender> EmailSender { get; set; }
            public Mock<ISignInManager> SignInManager { get; set; }
            public AuthenticationFlowService FlowService { get; set; }
        }
    }
}
