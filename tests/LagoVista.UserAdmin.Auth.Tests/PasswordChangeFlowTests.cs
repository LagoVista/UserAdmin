using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.DTOs;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class PasswordChangeFlowTests
    {
        [Test]
        public async Task ChangePasswordAsync_Should_EmitSuccessTransition_And_ReturnManagerResult()
        {
            var request = new ChangePassword { UserId = "user-id", OldPassword = "old-password", NewPassword = "new-password" };
            var organization = EntityHeader.Create("org-id", "Organization");
            var user = EntityHeader.Create("user-id", "User");
            var managerResult = InvokeResult.Success;
            var passwordManager = new Mock<IPasswordManager>(MockBehavior.Strict);
            passwordManager.Setup(manager => manager.ChangePasswordAsync(request, organization, user)).ReturnsAsync(managerResult);

            var handler = new PasswordChangeFlowHandler(passwordManager.Object);
            var result = await handler.HandleAsync(new PasswordChangeFlowRequest(request, organization, user));

            Assert.That(result.TransitionKey, Is.EqualTo(PasswordChangeFlowHandler.SuccessTransitionKey));
            Assert.That(result.PublicResult, Is.SameAs(managerResult));
            passwordManager.Verify(manager => manager.ChangePasswordAsync(request, organization, user), Times.Once);
            passwordManager.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ChangePasswordAsync_Should_EmitFailedTransition_And_ReturnManagerResult()
        {
            var request = new ChangePassword { UserId = "user-id", OldPassword = "wrong-password", NewPassword = "new-password" };
            var organization = EntityHeader.Create("org-id", "Organization");
            var user = EntityHeader.Create("user-id", "User");
            var managerResult = InvokeResult.FromErrors(new ErrorMessage("Password change rejected."));
            var passwordManager = new Mock<IPasswordManager>(MockBehavior.Strict);
            passwordManager.Setup(manager => manager.ChangePasswordAsync(request, organization, user)).ReturnsAsync(managerResult);

            var handler = new PasswordChangeFlowHandler(passwordManager.Object);
            var result = await handler.HandleAsync(new PasswordChangeFlowRequest(request, organization, user));

            Assert.That(result.TransitionKey, Is.EqualTo(PasswordChangeFlowHandler.FailedTransitionKey));
            Assert.That(result.PublicResult, Is.SameAs(managerResult));
            passwordManager.Verify(manager => manager.ChangePasswordAsync(request, organization, user), Times.Once);
            passwordManager.VerifyNoOtherCalls();
        }

        [Test]
        public async Task AuthenticationFlowService_Should_AcceptPasswordChangeTransitions()
        {
            var request = new ChangePassword { UserId = "user-id", OldPassword = "old-password", NewPassword = "new-password" };
            var organization = EntityHeader.Create("org-id", "Organization");
            var user = EntityHeader.Create("user-id", "User");
            var publicResult = InvokeResult.Success;
            var passwordChangeHandler = new Mock<IAuthenticationFlowHandler<PasswordChangeFlowRequest>>(MockBehavior.Strict);
            passwordChangeHandler.Setup(handler => handler.HandleAsync(It.Is<PasswordChangeFlowRequest>(flowRequest => flowRequest.Request == request && flowRequest.Organization == organization && flowRequest.User == user))).ReturnsAsync(new AuthenticationFlowResult(PasswordChangeFlowHandler.SuccessTransitionKey, publicResult));

            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var flowService = new AuthenticationFlowService(passwordLoginHandler.Object, recoveryHandler.Object, passwordChangeHandler: passwordChangeHandler.Object);

            var result = await flowService.ChangePasswordAsync(request, organization, user);

            Assert.That(result, Is.SameAs(publicResult));
            passwordChangeHandler.VerifyAll();
        }
    }
}
