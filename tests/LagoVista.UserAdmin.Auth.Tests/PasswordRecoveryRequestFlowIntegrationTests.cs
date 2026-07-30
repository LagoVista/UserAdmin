using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class PasswordRecoveryRequestFlowIntegrationTests
    {
        private const string RecoveryRequestEvidence = "auth|auth.test-binding.recovery.request|auth.flow.recovery.request|auth.transition.recovery.request";
        private const string SuccessfulRequestEvents = "PasswordRecoveryRequested|PasswordRecoveryCodeGenerated|PasswordRecoveryMessageSent";
        private const string UserNotFoundEvents = "PasswordRecoveryRequested";

        [Test]
        [Property("AptixEvidence", RecoveryRequestEvidence)]
        [Property("AptixAuthEvents", SuccessfulRequestEvents)]
        public async Task SuccessfulRequest_Should_SendRecoveryMessage_And_RecordMilestones()
        {
            var harness = CreateHarness();
            var user = new AppUser("user@example.com", "test") { UserName = "user@example.com", Email = "user@example.com" };

            harness.UserManager.Setup(manager => manager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            harness.UserManager.Setup(manager => manager.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            harness.EmailSender
                .Setup(sender => sender.SendAsync(
                    "user@example.com",
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<EntityHeader>(),
                    It.IsAny<EntityHeader>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(InvokeResult.Success);

            var result = await harness.FlowService.RequestPasswordRecoveryAsync(new SendResetPasswordLink { Email = "user@example.com" });

            Assert.That(result.Successful, Is.True);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.PasswordRecoveryRequested,
                AuthLogTypes.PasswordRecoveryCodeGenerated,
                AuthLogTypes.PasswordRecoveryMessageSent
            }));
        }

        [Test]
        [Property("AptixEvidence", RecoveryRequestEvidence)]
        [Property("AptixAuthEvents", UserNotFoundEvents)]
        public async Task UserNotFound_Should_ReturnFailure_And_RecordRequestOnly()
        {
            var harness = CreateHarness();
            harness.UserManager.Setup(manager => manager.FindByEmailAsync("missing@example.com")).ReturnsAsync((AppUser)null);

            var result = await harness.FlowService.RequestPasswordRecoveryAsync(new SendResetPasswordLink { Email = "missing@example.com" });

            Assert.That(result.Successful, Is.False);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.PasswordRecoveryRequested
            }));
            Assert.That(harness.Log.Events.Single().UserName, Is.EqualTo("missing@example.com"));
        }

        private static PasswordRecoveryRequestHarness CreateHarness()
        {
            var log = new RecordingAuthenticationLogManager();
            var validators = new Mock<IAuthRequestValidators>(MockBehavior.Strict);
            var userManager = new Mock<IUserManager>(MockBehavior.Strict);
            var emailSender = new Mock<IEmailSender>(MockBehavior.Loose);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Loose);

            validators
                .Setup(validator => validator.ValidateSendPasswordLinkRequest(It.IsAny<SendResetPasswordLink>()))
                .Returns(InvokeResult.Success);
            appConfig.SetupGet(config => config.WebAddress).Returns("https://dev.nuviot.com");
            appConfig.SetupGet(config => config.AppName).Returns("NuvIoT");
            appConfig.SetupGet(config => config.SystemOwnerOrg).Returns(EntityHeader.Create("system", "System"));

            var manager = new PasswordManager(
                validators.Object,
                userManager.Object,
                emailSender.Object,
                new Mock<IDependencyManager>(MockBehavior.Loose).Object,
                new Mock<ISecurity>(MockBehavior.Loose).Object,
                log,
                new Mock<IAdminLogger>(MockBehavior.Loose).Object,
                appConfig.Object);

            var recoveryHandler = new PasswordRecoveryRequestFlowHandler(manager);
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);

            return new PasswordRecoveryRequestHarness
            {
                Log = log,
                UserManager = userManager,
                EmailSender = emailSender,
                FlowService = new AuthenticationFlowService(passwordLoginHandler.Object, recoveryHandler)
            };
        }

        private sealed class PasswordRecoveryRequestHarness
        {
            public RecordingAuthenticationLogManager Log { get; set; }
            public Mock<IUserManager> UserManager { get; set; }
            public Mock<IEmailSender> EmailSender { get; set; }
            public AuthenticationFlowService FlowService { get; set; }
        }
    }
}
