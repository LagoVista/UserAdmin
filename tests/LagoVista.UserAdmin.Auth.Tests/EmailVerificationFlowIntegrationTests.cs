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
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Orgs;
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
    public class EmailVerificationFlowIntegrationTests
    {
        private const string AcceptedVerificationEvidence = "auth|auth.test-binding.email-verification.verify-code|auth.flow.email-verification.verify-code|auth.transition.email-verification.code-accepted";
        private const string RejectedVerificationEvidence = "auth|auth.test-binding.email-verification.verify-code|auth.flow.email-verification.verify-code|auth.transition.email-verification.code-rejected";
        private const string SuccessfulVerificationEvents = "ConfirmEmailSuccess";
        private const string FailedVerificationEvents = "EmailConfirmFailed";
        private const string UserId = "7F3A91C2D8E44B6FA1029C7D5E8B34A1";
        private const string ValidCode = "123456";
        private const string InvalidCode = "654321";
        private const string SecurityStamp = "email-verification-security-stamp";

        [Test]
        [Property("AptixEvidence", AcceptedVerificationEvidence)]
        [Property("AptixAuthEvents", SuccessfulVerificationEvents)]
        public async Task ValidCode_Should_ConfirmEmail_ConsumeCode_SignIn_And_ReturnRedirect()
        {
            var harness = CreateHarness();
            var appUser = CreateUser();
            var request = new ConfirmEmail { ReceivedCode = ValidCode };
            var userHeader = EntityHeader.Create(UserId, "Test User");
            var verificationCode = CreateVerificationCode(appUser, ValidCode);

            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(appUser);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync(verificationCode);
            harness.UserManager.Setup(manager => manager.UpdateAsync(It.Is<AppUser>(user => user.EmailConfirmed))).ReturnsAsync(InvokeResult.Success);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.UpdateAsync(It.IsAny<EmailVerificationCode>())).Returns(Task.CompletedTask);
            harness.SignInManager.Setup(manager => manager.SignInAsync(appUser, false)).Returns(Task.CompletedTask);

            var result = await harness.FlowService.VerifyEmailAsync(request, userHeader);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.RedirectURL, Is.Not.Null.And.Not.Empty);
            Assert.That(appUser.EmailConfirmed, Is.True);
            Assert.That(verificationCode.ConsumedUtc.HasValue, Is.True);
            harness.EmailVerificationCodeRepo.Verify(repo => repo.UpdateAsync(It.Is<EmailVerificationCode>(code => code.ConsumedUtc.HasValue && code.AttemptCount == 0)), Times.Once);
            harness.SignInManager.Verify(manager => manager.SignInAsync(appUser, false), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Does.Contain(AuthLogTypes.ConfirmEmailSuccess));
        }

        [Test]
        [Property("AptixEvidence", AcceptedVerificationEvidence)]
        [Property("AptixAuthEvents", SuccessfulVerificationEvents)]
        public async Task ValidCode_WithNonProductLineCurrentOrganization_Should_ReturnDefaultRedirect()
        {
            var harness = CreateHarness();
            var appUser = CreateUser();
            appUser.CurrentOrganization = new OrganizationSummary
            {
                Id = "4A4A2B1C4D6E48769A35C8B53462F0A1",
                Text = "Provisional Workspace",
                Namespace = OrgNamespace.Parse("provisionalworkspace"),
                IsForProductLine = false
            };

            var request = new ConfirmEmail { ReceivedCode = ValidCode };
            var userHeader = EntityHeader.Create(UserId, "Test User");
            var verificationCode = CreateVerificationCode(appUser, ValidCode);

            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(appUser);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync(verificationCode);
            harness.UserManager.Setup(manager => manager.UpdateAsync(It.Is<AppUser>(user => user.EmailConfirmed))).ReturnsAsync(InvokeResult.Success);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.UpdateAsync(It.IsAny<EmailVerificationCode>())).Returns(Task.CompletedTask);
            harness.SignInManager.Setup(manager => manager.SignInAsync(appUser, false)).Returns(Task.CompletedTask);

            var result = await harness.FlowService.VerifyEmailAsync(request, userHeader);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.RedirectURL, Is.Not.Null.And.Not.Empty);
            Assert.That(appUser.EmailConfirmed, Is.True);
            harness.OrganizationManager.Verify(manager => manager.GetPublicOrginfoAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", RejectedVerificationEvidence)]
        [Property("AptixAuthEvents", FailedVerificationEvents)]
        public async Task InvalidCode_Should_IncrementAttemptCount_And_ReturnFailure()
        {
            var harness = CreateHarness();
            var appUser = CreateUser();
            var request = new ConfirmEmail { ReceivedCode = InvalidCode };
            var userHeader = EntityHeader.Create(UserId, "Test User");
            var verificationCode = CreateVerificationCode(appUser, ValidCode);

            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(appUser);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync(verificationCode);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.UpdateAsync(It.IsAny<EmailVerificationCode>())).Returns(Task.CompletedTask);

            var result = await harness.FlowService.VerifyEmailAsync(request, userHeader);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("invalid or expired"));
            Assert.That(verificationCode.AttemptCount, Is.EqualTo(1));
            Assert.That(verificationCode.ConsumedUtc.HasValue, Is.False);
            Assert.That(appUser.EmailConfirmed, Is.False);
            harness.UserManager.Verify(manager => manager.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), false), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Does.Contain(AuthLogTypes.EmailConfirmFailed));
        }

        [Test]
        [Property("AptixEvidence", RejectedVerificationEvidence)]
        [Property("AptixAuthEvents", FailedVerificationEvents)]
        public async Task ExpiredCode_Should_ReturnFailure_WithoutUpdatingCodeOrUser()
        {
            var harness = CreateHarness();
            var appUser = CreateUser();
            var request = new ConfirmEmail { ReceivedCode = ValidCode };
            var userHeader = EntityHeader.Create(UserId, "Test User");
            var verificationCode = CreateVerificationCode(appUser, ValidCode);
            verificationCode.ExpiresUtc = DateTime.UtcNow.AddMinutes(-1);

            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(appUser);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync(verificationCode);

            var result = await harness.FlowService.VerifyEmailAsync(request, userHeader);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("invalid or expired"));
            Assert.That(appUser.EmailConfirmed, Is.False);
            harness.EmailVerificationCodeRepo.Verify(repo => repo.UpdateAsync(It.IsAny<EmailVerificationCode>()), Times.Never);
            harness.UserManager.Verify(manager => manager.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), false), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Does.Contain(AuthLogTypes.EmailConfirmFailed));
        }

        [Test]
        [Property("AptixEvidence", RejectedVerificationEvidence)]
        [Property("AptixAuthEvents", FailedVerificationEvents)]
        public async Task ConsumedCode_Should_ReturnFailure_WithoutUpdatingCodeOrUser()
        {
            var harness = CreateHarness();
            var appUser = CreateUser();
            var request = new ConfirmEmail { ReceivedCode = ValidCode };
            var userHeader = EntityHeader.Create(UserId, "Test User");
            var verificationCode = CreateVerificationCode(appUser, ValidCode);
            verificationCode.ConsumedUtc = DateTime.UtcNow.AddSeconds(-10);

            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(appUser);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync(verificationCode);

            var result = await harness.FlowService.VerifyEmailAsync(request, userHeader);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("invalid or expired"));
            Assert.That(appUser.EmailConfirmed, Is.False);
            harness.EmailVerificationCodeRepo.Verify(repo => repo.UpdateAsync(It.IsAny<EmailVerificationCode>()), Times.Never);
            harness.UserManager.Verify(manager => manager.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), false), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Does.Contain(AuthLogTypes.EmailConfirmFailed));
        }

        [Test]
        [Property("AptixEvidence", RejectedVerificationEvidence)]
        [Property("AptixAuthEvents", FailedVerificationEvents)]
        public async Task FifthInvalidAttempt_Should_ConsumeCode_And_ReturnFailure()
        {
            var harness = CreateHarness();
            var appUser = CreateUser();
            var request = new ConfirmEmail { ReceivedCode = InvalidCode };
            var userHeader = EntityHeader.Create(UserId, "Test User");
            var verificationCode = CreateVerificationCode(appUser, ValidCode);
            verificationCode.AttemptCount = 4;

            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(appUser);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.GetLatestAsync(UserId)).ReturnsAsync(verificationCode);
            harness.EmailVerificationCodeRepo.Setup(repo => repo.UpdateAsync(It.IsAny<EmailVerificationCode>())).Returns(Task.CompletedTask);

            var result = await harness.FlowService.VerifyEmailAsync(request, userHeader);

            Assert.That(result.Successful, Is.False);
            Assert.That(verificationCode.AttemptCount, Is.EqualTo(5));
            Assert.That(verificationCode.ConsumedUtc.HasValue, Is.True);
            Assert.That(appUser.EmailConfirmed, Is.False);
            harness.EmailVerificationCodeRepo.Verify(repo => repo.UpdateAsync(It.Is<EmailVerificationCode>(code => code.AttemptCount == 5 && code.ConsumedUtc.HasValue)), Times.Once);
            harness.UserManager.Verify(manager => manager.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), false), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Does.Contain(AuthLogTypes.EmailConfirmFailed));
        }

        [Test]
        [Property("AptixEvidence", AcceptedVerificationEvidence)]
        public async Task AlreadyConfirmedUser_Should_ReturnIdempotentSuccess_WithoutReadingCode()
        {
            var harness = CreateHarness();
            var appUser = CreateUser();
            appUser.EmailConfirmed = true;
            var request = new ConfirmEmail { ReceivedCode = "000000" };
            var userHeader = EntityHeader.Create(UserId, "Test User");

            harness.UserManager.Setup(manager => manager.FindByIdAsync(UserId)).ReturnsAsync(appUser);
            harness.SignInManager.Setup(manager => manager.SignInAsync(appUser, false)).Returns(Task.CompletedTask);

            var result = await harness.FlowService.VerifyEmailAsync(request, userHeader);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.RedirectURL, Is.Not.Null.And.Not.Empty);
            harness.EmailVerificationCodeRepo.Verify(repo => repo.GetLatestAsync(It.IsAny<string>()), Times.Never);
            harness.EmailVerificationCodeRepo.Verify(repo => repo.UpdateAsync(It.IsAny<EmailVerificationCode>()), Times.Never);
            harness.UserManager.Verify(manager => manager.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.SignInAsync(appUser, false), Times.Once);
        }

        private static EmailVerificationHarness CreateHarness()
        {
            var log = new RecordingAuthenticationLogManager();
            var userManager = new Mock<IUserManager>(MockBehavior.Strict);
            var emailVerificationCodeRepo = new Mock<IEmailVerificationCodeRepo>(MockBehavior.Strict);
            var signInManager = new Mock<ISignInManager>(MockBehavior.Strict);
            var organizationManager = new Mock<IOrganizationManager>(MockBehavior.Loose);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Loose);

            var manager = new UserVerficationManager(
                new Mock<IAdminLogger>(MockBehavior.Loose).Object,
                userManager.Object,
                appConfig.Object,
                new Mock<ISmsSender>(MockBehavior.Loose).Object,
                new Mock<IAppUserRepo>(MockBehavior.Loose).Object,
                log,
                organizationManager.Object,
                signInManager.Object,
                new Mock<IEmailSender>(MockBehavior.Loose).Object,
                emailVerificationCodeRepo.Object,
                new Mock<IDependencyManager>(MockBehavior.Loose).Object,
                new Mock<ISecurity>(MockBehavior.Loose).Object);

            var emailVerificationHandler = new EmailVerificationFlowHandler(manager);
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryRequestHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);

            return new EmailVerificationHarness
            {
                Log = log,
                UserManager = userManager,
                EmailVerificationCodeRepo = emailVerificationCodeRepo,
                OrganizationManager = organizationManager,
                SignInManager = signInManager,
                FlowService = new AuthenticationFlowService(passwordLoginHandler.Object, recoveryRequestHandler.Object, emailVerificationHandler: emailVerificationHandler)
            };
        }

        private static AppUser CreateUser()
        {
            return new AppUser("user@example.com", "test")
            {
                Id = UserId,
                UserName = "user@example.com",
                Email = "user@example.com",
                SecurityStamp = SecurityStamp,
                EmailConfirmed = false
            };
        }

        private static EmailVerificationCode CreateVerificationCode(AppUser appUser, string code)
        {
            var now = DateTime.UtcNow;
            return new EmailVerificationCode
            {
                Id = "verification-code-id",
                UserId = appUser.Id,
                CodeHash = ComputeCodeHash(appUser, code),
                CreatedUtc = now,
                ExpiresUtc = now.AddMinutes(10),
                AttemptCount = 0
            };
        }

        private static string ComputeCodeHash(AppUser appUser, string code)
        {
            var key = !String.IsNullOrWhiteSpace(appUser.SecurityStamp) ? appUser.SecurityStamp : appUser.PasswordHash;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(code)));
            }
        }

        private sealed class EmailVerificationHarness
        {
            public RecordingAuthenticationLogManager Log { get; set; }
            public Mock<IUserManager> UserManager { get; set; }
            public Mock<IEmailVerificationCodeRepo> EmailVerificationCodeRepo { get; set; }
            public Mock<IOrganizationManager> OrganizationManager { get; set; }
            public Mock<ISignInManager> SignInManager { get; set; }
            public AuthenticationFlowService FlowService { get; set; }
        }
    }
}
