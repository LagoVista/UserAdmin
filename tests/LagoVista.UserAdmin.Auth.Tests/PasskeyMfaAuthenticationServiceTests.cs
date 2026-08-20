using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Auth.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class PasskeyMfaAuthenticationServiceTests
    {
        private const string Evidence = "auth|auth.test-binding.passkey-sign-in.mfa|auth.behavior.passkey.mfa-sign-in|auth.transition.passkey.complete-mfa";
        private const string ChallengeId = "P1111111111111111111111111111111";
        private const string UserId = "P2222222222222222222222222222222";

        [Test]
        [Property("AptixEvidence", Evidence)]
        public async Task Begin_WithValidPasswordIssuedChallenge_Should_BindPasskeyChallengeToChallengeUserAndRequireStepUp()
        {
            var harness = CreateHarness();
            var expected = new PasskeyBeginOptionsResponse { ChallengeId = "P3333333333333333333333333333333" };

            harness.PasskeyManager
                .Setup(manager => manager.BeginAuthenticationOptionsAsync(UserId, true, "/auth/continue/passkey", null, null))
                .ReturnsAsync(InvokeResult<PasskeyBeginOptionsResponse>.Create(expected));

            var result = await harness.Service.BeginAsync(ChallengeId, "/auth/continue/passkey", null, null);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(expected));
            harness.PasskeyManager.Verify(manager => manager.BeginAuthenticationOptionsAsync(UserId, true, "/auth/continue/passkey", null, null), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", Evidence)]
        public async Task Complete_WithValidPasskeyProof_Should_ConsumeMfaChallengeAfterProofAndReturnChallengeUser()
        {
            var harness = CreateHarness();
            var passkey = new PasskeyAuthenticationCompleteRequest { ChallengeId = "P4444444444444444444444444444444" };
            var user = new AppUser("user@example.com", "test") { Id = UserId, UserName = "user@example.com" };
            var sequence = new MockSequence();

            harness.MfaChallengeFlow
                .InSequence(sequence)
                .Setup(service => service.ValidateAsync(ChallengeId, "passkey", null))
                .ReturnsAsync(InvokeResult<MfaChallenge>.Create(CreateChallenge()));
            harness.PasskeyManager
                .InSequence(sequence)
                .Setup(manager => manager.CompleteAuthenticationAsync(UserId, passkey, true, null, null))
                .ReturnsAsync(InvokeResult.Success);
            harness.MfaChallengeFlow
                .InSequence(sequence)
                .Setup(service => service.ConsumeAsync(ChallengeId, "passkey", null))
                .ReturnsAsync(InvokeResult<MfaChallenge>.Create(CreateChallenge()));
            harness.AppUserRepo
                .InSequence(sequence)
                .Setup(repo => repo.FindByIdAsync(UserId))
                .ReturnsAsync(user);

            var result = await harness.Service.CompleteAsync(ChallengeId, passkey, null, null);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(user));
            harness.PasskeyManager.Verify(manager => manager.CompleteAuthenticationAsync(UserId, passkey, true, null, null), Times.Once);
            harness.MfaChallengeFlow.Verify(service => service.ConsumeAsync(ChallengeId, "passkey", null), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", Evidence)]
        public async Task Complete_WhenPasskeyProofFails_Should_NotConsumePasswordIssuedMfaChallenge()
        {
            var harness = CreateHarness();
            var passkey = new PasskeyAuthenticationCompleteRequest { ChallengeId = "P5555555555555555555555555555555" };

            harness.PasskeyManager
                .Setup(manager => manager.CompleteAuthenticationAsync(UserId, passkey, true, null, null))
                .ReturnsAsync(InvokeResult.FromError("invalid-passkey"));

            var result = await harness.Service.CompleteAsync(ChallengeId, passkey, null, null);

            Assert.That(result.Successful, Is.False);
            harness.MfaChallengeFlow.Verify(service => service.ConsumeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            harness.AppUserRepo.Verify(repo => repo.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", Evidence)]
        public async Task Complete_WithInvalidMfaChallenge_Should_RejectBeforePasskeyProof()
        {
            var mfaChallengeFlow = new Mock<IMfaChallengeFlowService>(MockBehavior.Strict);
            var passkeyManager = new Mock<IAppUserPasskeyManager>(MockBehavior.Strict);
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Strict);
            var service = new PasskeyMfaAuthenticationService(mfaChallengeFlow.Object, passkeyManager.Object, appUserRepo.Object);
            var passkey = new PasskeyAuthenticationCompleteRequest { ChallengeId = "P6666666666666666666666666666666" };

            mfaChallengeFlow
                .Setup(flow => flow.ValidateAsync(ChallengeId, "passkey", null))
                .ReturnsAsync(InvokeResult<MfaChallenge>.FromError("mfa_challenge_invalid"));

            var result = await service.CompleteAsync(ChallengeId, passkey, null, null);

            Assert.That(result.Successful, Is.False);
            passkeyManager.Verify(manager => manager.CompleteAuthenticationAsync(It.IsAny<string>(), It.IsAny<PasskeyAuthenticationCompleteRequest>(), It.IsAny<bool>(), null, null), Times.Never);
            appUserRepo.Verify(repo => repo.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        private static Harness CreateHarness()
        {
            var mfaChallengeFlow = new Mock<IMfaChallengeFlowService>(MockBehavior.Strict);
            var passkeyManager = new Mock<IAppUserPasskeyManager>(MockBehavior.Strict);
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Strict);

            mfaChallengeFlow
                .Setup(service => service.ValidateAsync(ChallengeId, "passkey", null))
                .ReturnsAsync(InvokeResult<MfaChallenge>.Create(CreateChallenge()));

            return new Harness
            {
                MfaChallengeFlow = mfaChallengeFlow,
                PasskeyManager = passkeyManager,
                AppUserRepo = appUserRepo,
                Service = new PasskeyMfaAuthenticationService(mfaChallengeFlow.Object, passkeyManager.Object, appUserRepo.Object)
            };
        }

        private static MfaChallenge CreateChallenge()
        {
            return new MfaChallenge
            {
                Id = ChallengeId,
                UserId = UserId,
                Email = "user@example.com",
                AvailableProviders = new[] { "totp", "passkey" },
                CreatedUtc = DateTime.UtcNow.ToString("O"),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(5).ToString("O")
            };
        }

        private sealed class Harness
        {
            public PasskeyMfaAuthenticationService Service { get; set; }
            public Mock<IMfaChallengeFlowService> MfaChallengeFlow { get; set; }
            public Mock<IAppUserPasskeyManager> PasskeyManager { get; set; }
            public Mock<IAppUserRepo> AppUserRepo { get; set; }
        }
    }
}
