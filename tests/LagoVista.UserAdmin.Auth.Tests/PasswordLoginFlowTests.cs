using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Auth;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class PasswordLoginFlowTests
    {
        private const string SuccessfulPasswordLoginEvidence = "auth|auth.test-binding.password.establish-session|auth.flow.password.establish-session|auth.transition.password.establish-session";

        [Test]
        [Property("AptixEvidence", SuccessfulPasswordLoginEvidence)]
        public async Task LoginWithPasswordAsync_Should_Execute_Handler_And_Return_Manager_Result()
        {
            var request = new AuthLoginRequest
            {
                Email = "user@example.com",
                Password = "correct-password",
                RememberMe = true
            };

            var response = new AuthenticationResponse
            {
                AuthenticationState = AuthenticationResponseState.Authenticated,
                RedirectPage = "/home"
            };

            var managerResult = InvokeResult<AuthenticationResponse>.Create(response);

            var signInManager = new Mock<ISignInManager>(MockBehavior.Strict);
            signInManager
                .Setup(manager => manager.PasswordSignInAsync(request))
                .ReturnsAsync(managerResult);

            var recoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var passwordLoginHandler = new PasswordLoginFlowHandler(signInManager.Object);
            var flowService = new AuthenticationFlowService(passwordLoginHandler, recoveryHandler.Object);

            var result = await flowService.LoginWithPasswordAsync(request);

            Assert.That(result, Is.SameAs(managerResult));
            Assert.That(result.Result, Is.SameAs(response));
            Assert.That(result.Result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.Authenticated));
            Assert.That(result.Result.RedirectPage, Is.EqualTo("/home"));

            signInManager.Verify(manager => manager.PasswordSignInAsync(request), Times.Once);
            signInManager.VerifyNoOtherCalls();
        }
    }
}