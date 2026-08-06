using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
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
    public class PasswordRecoveryCompletionFlowIntegrationTests
    {
        private const string RecoveryCompletionEvidence = "auth|auth.test-binding.recovery.complete|auth.flow.recovery.complete|auth.transition.recovery.complete";
        private const string SuccessfulCompletionEvents = "PasswordRecoveryCompleted";

        [Test]
        [Property("AptixEvidence", RecoveryCompletionEvidence)]
        [Property("AptixAuthEvents", SuccessfulCompletionEvents)]
        public async Task SuccessfulReset_Should_CompleteRecovery_And_RecordMilestone()
        {
            var harness = CreateHarness();
            var user = new AppUser("user@example.com", "test") { UserName = "user@example.com", Email = "user@example.com" };
            var request = CreateRequest();

            harness.UserManager.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            harness.UserManager.Setup(manager => manager.ResetPasswordAsync(user, request.Token, request.NewPassword)).ReturnsAsync(InvokeResult.Success);

            var result = await harness.FlowService.CompletePasswordRecoveryAsync(request);

            Assert.That(result.Successful, Is.True);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.PasswordRecoveryCompleted
            }));
        }

        [Test]
        [Property("AptixEvidence", RecoveryCompletionEvidence)]
        public async Task FailedReset_Should_ReturnFailure_WithoutRecordingCompletion()
        {
            var harness = CreateHarness();
            var user = new AppUser("user@example.com", "test") { UserName = "user@example.com", Email = "user@example.com" };
            var request = CreateRequest();
            var failedResult = InvokeResult.FromErrors(new ErrorMessage("Reset failed."));

            harness.UserManager.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            harness.UserManager.Setup(manager => manager.ResetPasswordAsync(user, request.Token, request.NewPassword)).ReturnsAsync(failedResult);

            var result = await harness.FlowService.CompletePasswordRecoveryAsync(request);

            Assert.That(result.Successful, Is.False);
            Assert.That(harness.Log.Events, Is.Empty);
        }

        private static ResetPassword CreateRequest()
        {
            return new ResetPassword
            {
                Email = "user@example.com",
                Token = "reset-token",
                NewPassword = "NewPassword123!"
            };
        }

        private static PasswordRecoveryCompletionHarness CreateHarness()
        {
            var log = new RecordingAuthenticationLogManager();
            var validators = new Mock<IAuthRequestValidators>(MockBehavior.Strict);
            var userManager = new Mock<IUserManager>(MockBehavior.Strict);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Loose);

            validators
                .Setup(validator => validator.ValidateResetPasswordRequest(It.IsAny<ResetPassword>()))
                .Returns(InvokeResult.Success);
            appConfig.SetupGet(config => config.SystemOwnerOrg).Returns(EntityHeader.Create("system", "System"));

            var manager = new PasswordManager(
                validators.Object,
                userManager.Object,
                new Mock<IEmailSender>(MockBehavior.Loose).Object,
                new Mock<IPasswordResetCodeRepo>(MockBehavior.Loose).Object,
                new Mock<IDependencyManager>(MockBehavior.Loose).Object,
                new Mock<ISecurity>(MockBehavior.Loose).Object,
                log,
                new Mock<IAdminLogger>(MockBehavior.Loose).Object,
                appConfig.Object);

            var completionHandler = new PasswordRecoveryCompletionFlowHandler(manager);
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryRequestHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);

            return new PasswordRecoveryCompletionHarness
            {
                Log = log,
                UserManager = userManager,
                FlowService = new AuthenticationFlowService(passwordLoginHandler.Object, recoveryRequestHandler.Object, completionHandler)
            };
        }

        private sealed class PasswordRecoveryCompletionHarness
        {
            public RecordingAuthenticationLogManager Log { get; set; }
            public Mock<IUserManager> UserManager { get; set; }
            public AuthenticationFlowService FlowService { get; set; }
        }
    }
}
