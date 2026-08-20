using Fido2NetLib;
using Fido2NetLib.Objects;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth.Passkeys;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class AppUserPasskeyMfaIntegrationTests
    {
        private const string Evidence = "auth|auth.test-binding.passkey-sign-in.mfa|auth.behavior.passkey.mfa-sign-in|auth.transition.passkey.complete-mfa";
        private const string UserId = "K1111111111111111111111111111111";
        private const string OtherUserId = "K2222222222222222222222222222222";
        private const string ChallengeId = "K3333333333333333333333333333333";
        private const string RpId = "auth.example.com";
        private const string Origin = "https://auth.example.com";
        private const string CredentialId = "AQID";

        [Test]
        [Property("AptixEvidence", Evidence)]
        [Property("AptixAuthEvents", "PasskeyAuthenticationOptionsSent|PasskeySetupStarted|PasskeyAuthenticationOptionsBeginSent")]
        public async Task BeginAuthentication_ForMfa_Should_RequireUserVerificationAndBindChallengeToExpectedUser()
        {
            var harness = CreateHarness();
            GetAssertionOptionsParams capturedOptions = null;
            PasskeyChallengePacket capturedPacket = null;

            harness.CredentialRepo
                .Setup(repo => repo.GetByUserAsync(UserId, RpId))
                .ReturnsAsync(new[] { CreateCredential(UserId) });

            harness.Fido2
                .Setup(fido => fido.GetAssertionOptions(It.IsAny<GetAssertionOptionsParams>()))
                .Returns((GetAssertionOptionsParams options) =>
                {
                    capturedOptions = options;
                    return new AssertionOptions
                    {
                        Challenge = new byte[] { 4, 5, 6 },
                        Timeout = 60000,
                        RpId = RpId,
                        AllowCredentials = options.AllowedCredentials,
                        UserVerification = options.UserVerification,
                        Extensions = new AuthenticationExtensionsClientInputs()
                    };
                });

            harness.ChallengeStore
                .Setup(store => store.CreateAsync(It.IsAny<PasskeyChallengePacket>()))
                .ReturnsAsync((PasskeyChallengePacket packet) =>
                {
                    capturedPacket = packet;
                    packet.Challenge.Id = ChallengeId;
                    return InvokeResult<PasskeyChallengePacket>.Create(packet);
                });

            var result = await harness.Manager.BeginAuthenticationOptionsAsync(UserId, true, "/auth/continue/passkey", null, null);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.ChallengeId, Is.EqualTo(ChallengeId));
            Assert.That(capturedOptions, Is.Not.Null);
            Assert.That(capturedOptions.UserVerification, Is.EqualTo(UserVerificationRequirement.Required));
            Assert.That(capturedPacket, Is.Not.Null);
            Assert.That(capturedPacket.Challenge.UserId, Is.EqualTo(UserId));
            Assert.That(capturedPacket.Challenge.UserVerification, Is.EqualTo((int)UserVerificationRequirement.Required));
            Assert.That(capturedPacket.Challenge.AllowCredentialIds, Is.EqualTo(new[] { CredentialId }));
            Assert.That(capturedPacket.Challenge.PasskeyUrl, Is.EqualTo("/auth/continue/passkey"));
        }

        [Test]
        [Property("AptixEvidence", Evidence)]
        public async Task CompleteAuthentication_WhenPasskeyChallengeBelongsToDifferentUser_Should_RejectBeforeCredentialOrFidoVerification()
        {
            var harness = CreateHarness();
            harness.ChallengeStore
                .Setup(store => store.ConsumeAsync(ChallengeId))
                .ReturnsAsync(InvokeResult<PasskeyChallengePacket>.Create(CreateChallengePacket(OtherUserId)));

            var result = await harness.Manager.CompleteAuthenticationAsync(
                UserId,
                new PasskeyAuthenticationCompleteRequest { ChallengeId = ChallengeId },
                true,
                null,
                null);

            Assert.That(result.Successful, Is.False);
            harness.CredentialRepo.Verify(repo => repo.FindByCredentialIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            harness.Fido2.Verify(fido => fido.MakeAssertionAsync(It.IsAny<MakeAssertionParams>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", Evidence)]
        public async Task CompleteAuthentication_WhenCredentialBelongsToDifferentUser_Should_RejectBeforeFidoVerification()
        {
            var harness = CreateHarness();
            harness.ChallengeStore
                .Setup(store => store.ConsumeAsync(ChallengeId))
                .ReturnsAsync(InvokeResult<PasskeyChallengePacket>.Create(CreateChallengePacket(UserId)));
            harness.CredentialRepo
                .Setup(repo => repo.FindByCredentialIdAsync(RpId, CredentialId))
                .ReturnsAsync(CreateCredential(OtherUserId));

            var result = await harness.Manager.CompleteAuthenticationAsync(
                UserId,
                CreateCompleteRequest(),
                true,
                null,
                null);

            Assert.That(result.Successful, Is.False);
            harness.Fido2.Verify(fido => fido.MakeAssertionAsync(It.IsAny<MakeAssertionParams>(), It.IsAny<CancellationToken>()), Times.Never);
            harness.CredentialRepo.Verify(repo => repo.UpdateSignCountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", Evidence)]
        [Property("AptixAuthEvents", "PasskeyCompleteAuthenticationStart|PasskeyCompleteAuthenticationSuccess")]
        public async Task CompleteAuthentication_WithValidChallengeAndCredential_Should_AdvanceCounterAndUpdateMfaFreshness()
        {
            var harness = CreateHarness();
            var appUser = new AppUser("user@example.com", "test") { Id = UserId, UserName = "user@example.com" };
            MakeAssertionParams capturedAssertion = null;

            harness.ChallengeStore
                .Setup(store => store.ConsumeAsync(ChallengeId))
                .ReturnsAsync(InvokeResult<PasskeyChallengePacket>.Create(CreateChallengePacket(UserId)));
            harness.CredentialRepo
                .Setup(repo => repo.FindByCredentialIdAsync(RpId, CredentialId))
                .ReturnsAsync(CreateCredential(UserId));
            harness.Fido2
                .Setup(fido => fido.MakeAssertionAsync(It.IsAny<MakeAssertionParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((MakeAssertionParams assertion, CancellationToken _) =>
                {
                    capturedAssertion = assertion;
                    return new VerifyAssertionResult
                    {
                        CredentialId = new byte[] { 1, 2, 3 },
                        SignCount = 10,
                        IsBackedUp = false
                    };
                });
            harness.CredentialRepo
                .Setup(repo => repo.UpdateSignCountAsync(UserId, RpId, CredentialId, 10, It.IsAny<string>()))
                .ReturnsAsync(InvokeResult.Success);
            harness.AppUserRepo.Setup(repo => repo.FindByIdAsync(UserId)).ReturnsAsync(appUser);
            harness.AppUserRepo.Setup(repo => repo.UpdateAsync(appUser)).Returns(Task.CompletedTask);

            var result = await harness.Manager.CompleteAuthenticationAsync(UserId, CreateCompleteRequest(), true, null, null);

            Assert.That(result.Successful, Is.True);
            Assert.That(capturedAssertion, Is.Not.Null);
            Assert.That(capturedAssertion.StoredSignatureCounter, Is.EqualTo(9));
            Assert.That(capturedAssertion.OriginalOptions.UserVerification, Is.EqualTo(UserVerificationRequirement.Required));
            Assert.That(appUser.LastMfaDateTimeUtc, Is.Not.Null.And.Not.Empty);
            harness.CredentialRepo.Verify(repo => repo.UpdateSignCountAsync(UserId, RpId, CredentialId, 10, It.IsAny<string>()), Times.Once);
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(appUser), Times.Once);
        }

        private static Harness CreateHarness()
        {
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Loose);
            var credentialRepo = new Mock<IAppUserPasskeyCredentialRepo>(MockBehavior.Strict);
            var challengeStore = new Mock<IPasskeyChallengeStore>(MockBehavior.Strict);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Strict);
            var logger = new Mock<IAdminLogger>(MockBehavior.Loose);
            var fido2 = new Mock<IFido2>(MockBehavior.Strict);
            var userRegistrationManager = new Mock<IUserRegistrationManager>(MockBehavior.Loose);
            var authLog = new RecordingAuthenticationLogManager();

            appConfig.SetupGet(config => config.WebAddress).Returns(Origin);

            return new Harness
            {
                AppUserRepo = appUserRepo,
                CredentialRepo = credentialRepo,
                ChallengeStore = challengeStore,
                Fido2 = fido2,
                Manager = new AppUserPasskeyManager(
                    appUserRepo.Object,
                    userRegistrationManager.Object,
                    authLog,
                    credentialRepo.Object,
                    challengeStore.Object,
                    appConfig.Object,
                    logger.Object,
                    fido2.Object)
            };
        }

        private static PasskeyCredential CreateCredential(string userId)
        {
            return new PasskeyCredential
            {
                UserId = userId,
                RpId = RpId,
                CredentialId = CredentialId,
                PublicKey = "BwgJ",
                SignCount = 9,
                CreatedUtc = DateTime.UtcNow.AddDays(-1).ToString("O")
            };
        }

        private static PasskeyChallengePacket CreateChallengePacket(string userId)
        {
            return new PasskeyChallengePacket
            {
                Challenge = new PasskeyChallenge
                {
                    Id = ChallengeId,
                    UserId = userId,
                    RpId = RpId,
                    Origin = Origin,
                    PasskeyUrl = "/auth/continue/passkey",
                    Purpose = PasskeyChallengePurpose.Authenticate,
                    Challenge = "BAUG",
                    CreatedUtc = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(4).ToString("O"),
                    AllowCredentialIds = new[] { CredentialId },
                    UserVerification = (int)UserVerificationRequirement.Required,
                    TimeoutMs = 60000
                }
            };
        }

        private static PasskeyAuthenticationCompleteRequest CreateCompleteRequest()
        {
            return new PasskeyAuthenticationCompleteRequest
            {
                ChallengeId = ChallengeId,
                Assertion = new WebAuthnAssertionWire
                {
                    Id = CredentialId,
                    RawId = CredentialId,
                    Type = "public-key",
                    Response = new WebAuthnAssertionResponseWire
                    {
                        ClientDataJSON = String.Empty,
                        AuthenticatorData = String.Empty,
                        Signature = String.Empty,
                        UserHandle = null
                    }
                }
            };
        }

        private sealed class Harness
        {
            public AppUserPasskeyManager Manager { get; set; }
            public Mock<IAppUserRepo> AppUserRepo { get; set; }
            public Mock<IAppUserPasskeyCredentialRepo> CredentialRepo { get; set; }
            public Mock<IPasskeyChallengeStore> ChallengeStore { get; set; }
            public Mock<IFido2> Fido2 { get; set; }
        }
    }
}
