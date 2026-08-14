using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Security;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class EmailVerificationSendFlowTests
    {
        private const string SentEvidence = "auth|auth.test-binding.email-verification.send-code|auth.flow.email-verification.send-code|auth.transition.email-verification.code-sent";
        private const string ResentEvidence = "auth|auth.test-binding.email-verification.send-code|auth.flow.email-verification.send-code|auth.transition.email-verification.code-resent";
        private const string ThrottledEvidence = "auth|auth.test-binding.email-verification.send-code|auth.flow.email-verification.send-code|auth.transition.email-verification.resend-throttled";
        private const string UserId = "7F3A91C2D8E44B6FA1029C7D5E8B34A1";

        [Test]
        [Property("AptixEvidence", SentEvidence)]
        public async Task NoRecentCode_Should_Send_And_ReturnCodeSentTransition()
        {
            var manager = new Mock<IUserVerficationManager>(MockBehavior.Strict);
            var repo = new Mock<IEmailVerificationCodeRepo>(MockBehavior.Strict);
            var user = EntityHeader.Create(UserId, "Test User");

            repo.Setup(x => x.GetLatestAsync(UserId)).ReturnsAsync((EmailVerificationCode)null);
            manager.Setup(x => x.SendConfirmationEmailAsync(UserId, "", "", "", "")).ReturnsAsync(InvokeResult<string>.Create("123456"));

            var handler = new EmailVerificationSendFlowHandler(manager.Object, repo.Object);
            var result = await handler.HandleAsync(new EmailVerificationSendFlowRequest(user));

            Assert.That(result.TransitionKey, Is.EqualTo(EmailVerificationSendFlowHandler.SentTransitionKey));
            Assert.That(result.PublicResult.Successful, Is.True);
            Assert.That(result.PublicResult.Result.Outcome, Is.EqualTo(EmailVerificationSendOutcome.Sent));
            Assert.That(result.PublicResult.Result.VerificationCode, Is.EqualTo("123456"));
            Assert.That(result.PublicResult.Result.RetryAfterSeconds, Is.EqualTo(0));
            manager.VerifyAll();
            repo.VerifyAll();
        }

        [Test]
        [Property("AptixEvidence", ThrottledEvidence)]
        public async Task RecentCode_Should_Throttle_WithoutSendingAnotherCode()
        {
            var manager = new Mock<IUserVerficationManager>(MockBehavior.Strict);
            var repo = new Mock<IEmailVerificationCodeRepo>(MockBehavior.Strict);
            var user = EntityHeader.Create(UserId, "Test User");
            var latest = new EmailVerificationCode
            {
                Id = "verification-code-id",
                UserId = UserId,
                CreatedUtc = DateTime.UtcNow.AddSeconds(-10),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(9),
                CodeHash = "hash"
            };

            repo.Setup(x => x.GetLatestAsync(UserId)).ReturnsAsync(latest);

            var handler = new EmailVerificationSendFlowHandler(manager.Object, repo.Object);
            var result = await handler.HandleAsync(new EmailVerificationSendFlowRequest(user));

            Assert.That(result.TransitionKey, Is.EqualTo(EmailVerificationSendFlowHandler.ThrottledTransitionKey));
            Assert.That(result.PublicResult.Successful, Is.True);
            Assert.That(result.PublicResult.Result.Outcome, Is.EqualTo(EmailVerificationSendOutcome.Throttled));
            Assert.That(result.PublicResult.Result.RetryAfterSeconds, Is.GreaterThan(0).And.LessThanOrEqualTo(60));
            manager.Verify(x => x.SendConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            repo.VerifyAll();
        }

        [Test]
        [Property("AptixEvidence", ResentEvidence)]
        public async Task ExistingCodeAfterCooldown_Should_Resend_And_ReturnCodeResentTransition()
        {
            var manager = new Mock<IUserVerficationManager>(MockBehavior.Strict);
            var repo = new Mock<IEmailVerificationCodeRepo>(MockBehavior.Strict);
            var user = EntityHeader.Create(UserId, "Test User");
            var latest = new EmailVerificationCode
            {
                Id = "verification-code-id",
                UserId = UserId,
                CreatedUtc = DateTime.UtcNow.AddSeconds(-90),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(8),
                CodeHash = "hash"
            };

            repo.Setup(x => x.GetLatestAsync(UserId)).ReturnsAsync(latest);
            manager.Setup(x => x.SendConfirmationEmailAsync(UserId, "", "", "", "")).ReturnsAsync(InvokeResult<string>.Create("654321"));

            var handler = new EmailVerificationSendFlowHandler(manager.Object, repo.Object);
            var result = await handler.HandleAsync(new EmailVerificationSendFlowRequest(user));

            Assert.That(result.TransitionKey, Is.EqualTo(EmailVerificationSendFlowHandler.ResentTransitionKey));
            Assert.That(result.PublicResult.Successful, Is.True);
            Assert.That(result.PublicResult.Result.Outcome, Is.EqualTo(EmailVerificationSendOutcome.Resent));
            Assert.That(result.PublicResult.Result.VerificationCode, Is.EqualTo("654321"));
            Assert.That(result.PublicResult.Result.RetryAfterSeconds, Is.EqualTo(0));
            manager.VerifyAll();
            repo.VerifyAll();
        }
    }
}
