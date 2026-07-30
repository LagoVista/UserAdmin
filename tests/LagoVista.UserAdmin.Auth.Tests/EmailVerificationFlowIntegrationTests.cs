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
    public class EmailVerificationFlowIntegrationTests
    {
        private const string EmailVerificationEvidence = "auth|auth.test-binding.email-verification.complete|auth.flow.email-verification.complete|auth.transition.email-verification.complete";
        private const string UserId = "7F3A91C2D8E44B6FA1029C7D5E8B34A1";

        [Test]
        [Property("AptixEvidence", EmailVerificationEvidence)]
        public async Task ValidToken_Should_CompleteVerification_And_ReturnRedirect()
        {
            var request = new ConfirmEmail { ReceivedCode = "valid-token" };
            var user = EntityHeader.Create(UserId, "Test User");
            var verificationManager = new Mock<IUserVerficationManager>(MockBehavior.Strict);
            var expectedResult = InvokeResult.SuccessRedirect("/home");

            verificationManager
                .Setup(manager => manager.ValidateEmailAsync(request, user))
                .ReturnsAsync(expectedResult);

            var result = await CreateFlowService(verificationManager.Object).VerifyEmailAsync(request, user);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.RedirectURL, Is.EqualTo("/home"));
            verificationManager.Verify(manager => manager.ValidateEmailAsync(request, user), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", EmailVerificationEvidence)]
        public async Task InvalidToken_Should_ReturnFailure_WithoutSuccessfulTransitionResult()
        {
            var request = new ConfirmEmail { ReceivedCode = "invalid-token" };
            var user = EntityHeader.Create(UserId, "Test User");
            var verificationManager = new Mock<IUserVerficationManager>(MockBehavior.Strict);
            var expectedResult = InvokeResult.FromError("Invalid email verification token.");

            verificationManager
                .Setup(manager => manager.ValidateEmailAsync(request, user))
                .ReturnsAsync(expectedResult);

            var result = await CreateFlowService(verificationManager.Object).VerifyEmailAsync(request, user);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Invalid email verification token"));
            verificationManager.Verify(manager => manager.ValidateEmailAsync(request, user), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", EmailVerificationEvidence)]
        public async Task AlreadyConfirmedUser_Should_ReturnIdempotentSuccess()
        {
            var request = new ConfirmEmail { ReceivedCode = "already-confirmed-token" };
            var user = EntityHeader.Create(UserId, "Test User");
            var verificationManager = new Mock<IUserVerficationManager>(MockBehavior.Strict);
            var expectedResult = InvokeResult.SuccessRedirect("/home");

            verificationManager
                .Setup(manager => manager.ValidateEmailAsync(request, user))
                .ReturnsAsync(expectedResult);

            var result = await CreateFlowService(verificationManager.Object).VerifyEmailAsync(request, user);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.RedirectURL, Is.EqualTo("/home"));
            verificationManager.Verify(manager => manager.ValidateEmailAsync(request, user), Times.Once);
        }

        private static AuthenticationFlowService CreateFlowService(IUserVerficationManager verificationManager)
        {
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryRequestHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var emailVerificationHandler = new EmailVerificationFlowHandler(verificationManager);

            return new AuthenticationFlowService(passwordLoginHandler.Object, recoveryRequestHandler.Object, emailVerificationHandler: emailVerificationHandler);
        }
    }
}
