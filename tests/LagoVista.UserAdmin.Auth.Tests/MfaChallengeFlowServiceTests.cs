using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class MfaChallengeFlowServiceTests
    {
        private const string MfaRequiredEvidence = "auth|auth.test-binding.password-sign-in|auth.flow.password-sign-in|auth.transition.password-sign-in.mfa-required";
        private const string ChallengeId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string UserId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private const string Email = "user@example.com";

        [Test]
        [Property("AptixEvidence", MfaRequiredEvidence)]
        public async Task ValidatePasskeyChallenge_Should_ReturnServerBoundUser()
        {
            var store = new Mock<IMfaChallengeStore>(MockBehavior.Strict);
            var challenge = CreateChallenge("passkey");
            store.Setup(repo => repo.GetAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(challenge));
            var service = new MfaChallengeFlowService(store.Object);

            var result = await service.ValidateAsync(ChallengeId, "passkey", Email);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.UserId, Is.EqualTo(UserId));
            store.Verify(repo => repo.GetAsync(ChallengeId), Times.Once);
            store.Verify(repo => repo.ConsumeAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", MfaRequiredEvidence)]
        public async Task ValidatePasskeyChallenge_WhenPasskeyWasNotOffered_Should_Reject()
        {
            var store = new Mock<IMfaChallengeStore>(MockBehavior.Strict);
            store.Setup(repo => repo.GetAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(CreateChallenge("totp")));
            var service = new MfaChallengeFlowService(store.Object);

            var result = await service.ValidateAsync(ChallengeId, "passkey", Email);

            Assert.That(result.Successful, Is.False);
            store.Verify(repo => repo.ConsumeAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", MfaRequiredEvidence)]
        public async Task ConsumePasskeyChallenge_Should_RevalidateThenConsumeExactlyOnce()
        {
            var store = new Mock<IMfaChallengeStore>(MockBehavior.Strict);
            var challenge = CreateChallenge("passkey", "totp");
            store.Setup(repo => repo.GetAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(challenge));
            store.Setup(repo => repo.ConsumeAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(challenge));
            var service = new MfaChallengeFlowService(store.Object);

            var result = await service.ConsumeAsync(ChallengeId, "passkey", Email);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.UserId, Is.EqualTo(UserId));
            store.Verify(repo => repo.GetAsync(ChallengeId), Times.Once);
            store.Verify(repo => repo.ConsumeAsync(ChallengeId), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", MfaRequiredEvidence)]
        public async Task ValidatePasskeyChallenge_WithDifferentEmail_Should_Reject()
        {
            var store = new Mock<IMfaChallengeStore>(MockBehavior.Strict);
            store.Setup(repo => repo.GetAsync(ChallengeId)).ReturnsAsync(InvokeResult<MfaChallenge>.Create(CreateChallenge("passkey")));
            var service = new MfaChallengeFlowService(store.Object);

            var result = await service.ValidateAsync(ChallengeId, "passkey", "other@example.com");

            Assert.That(result.Successful, Is.False);
            store.Verify(repo => repo.ConsumeAsync(It.IsAny<string>()), Times.Never);
        }

        private static MfaChallenge CreateChallenge(params string[] providers)
        {
            return new MfaChallenge
            {
                Id = ChallengeId,
                UserId = UserId,
                Email = Email,
                AvailableProviders = providers,
                CreatedUtc = DateTime.UtcNow.ToString("O"),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(5).ToString("O")
            };
        }
    }
}
