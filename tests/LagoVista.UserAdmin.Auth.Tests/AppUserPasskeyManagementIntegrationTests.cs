using Fido2NetLib;
using Fido2NetLib.Objects;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Auth.Passkeys;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class AppUserPasskeyManagementIntegrationTests
    {
        private const string AddEvidence = "auth|auth.test-binding.passkey-management|auth.behavior.passkey.add-success|auth.transition.passkey.begin-existing-user-registration|auth.transition.passkey.complete-existing-user-registration";
        private const string RemoveEvidence = "auth|auth.test-binding.passkey-management|auth.behavior.passkey.remove-success|auth.transition.passkey.remove-existing-user-credential";
        private const string UserId = "M1111111111111111111111111111111";
        private const string ChallengeId = "M2222222222222222222222222222222";
        private const string Email = "manager@example.com";
        private const string RpId = "auth.example.com";
        private const string Origin = "https://auth.example.com";
        private const string CredentialId = "AQID";
        private const string PublicKey = "BwgJ";
        private const string PasskeyUrl = "/auth/passkey/management";

        [Test]
        [Property("AptixEvidence", AddEvidence)]
        [Property("AptixAuthEvents", "PasskeyBeginRegistrationStart|PasskeyBeginRegistrationSuccess")]
        public async Task BeginRegistration_ForExistingUser_Should_BindChallengeAndExcludeExistingCredentials()
        {
            var harness = CreateHarness();
            RequestNewCredentialParams capturedOptions = null;
            PasskeyChallengePacket capturedPacket = null;

            harness.AppUserRepo.Setup(repo => repo.FindByIdAsync(UserId)).ReturnsAsync(harness.User);
            harness.CredentialRepo
                .Setup(repo => repo.GetByUserAsync(UserId, RpId))
                .ReturnsAsync(new[] { CreateCredential() });
            harness.Fido2
                .Setup(fido => fido.RequestNewCredential(It.IsAny<RequestNewCredentialParams>()))
                .Returns((RequestNewCredentialParams options) =>
                {
                    capturedOptions = options;
                    return CreateRegistrationOptions(options.User, options.ExcludeCredentials);
                });
            harness.ChallengeStore
                .Setup(store => store.CreateAsync(It.IsAny<PasskeyChallengePacket>()))
                .ReturnsAsync((PasskeyChallengePacket packet) =>
                {
                    capturedPacket = packet;
                    packet.Challenge.Id = ChallengeId;
                    return InvokeResult<PasskeyChallengePacket>.Create(packet);
                });

            var result = await harness.Manager.BeginRegistrationOptionsAsync(UserId, PasskeyUrl, null, null);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.ChallengeId, Is.EqualTo(ChallengeId));
            Assert.That(capturedOptions, Is.Not.Null);
            Assert.That(capturedOptions.User.Name, Is.EqualTo(Email));
            Assert.That(capturedOptions.User.DisplayName, Is.EqualTo(Email));
            Assert.That(capturedOptions.ExcludeCredentials, Has.Count.EqualTo(1));
            Assert.That(capturedPacket, Is.Not.Null);
            Assert.That(capturedPacket.Challenge.UserId, Is.EqualTo(UserId));
            Assert.That(capturedPacket.Challenge.RpId, Is.EqualTo(RpId));
            Assert.That(capturedPacket.Challenge.Origin, Is.EqualTo(Origin));
            Assert.That(capturedPacket.Challenge.PasskeyUrl, Is.EqualTo(PasskeyUrl));
            Assert.That(capturedPacket.Challenge.Purpose, Is.EqualTo(PasskeyChallengePurpose.Register));
        }

        [Test]
        [Property("AptixEvidence", AddEvidence)]
        public async Task CompleteRegistration_WithMissingPayload_Should_RejectBeforeChallengeOrFidoWork()
        {
            var harness = CreateHarness();

            var result = await harness.Manager.CompleteRegistrationAsync(UserId, null, null, null);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("missing_attestation"));
            harness.ChallengeStore.Verify(store => store.ConsumeAsync(It.IsAny<string>()), Times.Never);
            harness.Fido2.Verify(fido => fido.MakeNewCredentialAsync(It.IsAny<MakeNewCredentialParams>(), It.IsAny<CancellationToken>()), Times.Never);
            harness.CredentialRepo.Verify(repo => repo.AddAsync(It.IsAny<PasskeyCredential>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", AddEvidence)]
        public async Task CompleteRegistration_WithMissingAttestation_Should_RejectBeforeChallengeOrFidoWork()
        {
            var harness = CreateHarness();

            var result = await harness.Manager.CompleteRegistrationAsync(
                UserId,
                new PasskeyRegistrationCompleteRequest { ChallengeId = ChallengeId },
                null,
                null);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("missing_attestation"));
            harness.ChallengeStore.Verify(store => store.ConsumeAsync(It.IsAny<string>()), Times.Never);
            harness.Fido2.Verify(fido => fido.MakeNewCredentialAsync(It.IsAny<MakeNewCredentialParams>(), It.IsAny<CancellationToken>()), Times.Never);
            harness.CredentialRepo.Verify(repo => repo.AddAsync(It.IsAny<PasskeyCredential>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", AddEvidence)]
        [Property("AptixAuthEvents", "PasskeyCompleteRegistrationStart|PasskeyCompleteRegistrationSuccess")]
        public async Task CompleteRegistration_WithVerifiedAttestation_Should_PersistCredentialForCurrentUser()
        {
            var harness = CreateHarness();
            PasskeyCredential persisted = null;
            MakeNewCredentialParams capturedMakeCredential = null;
            var options = CreateRegistrationOptions(new Fido2User
            {
                Id = Encoding.UTF8.GetBytes(UserId),
                Name = Email,
                DisplayName = Email
            }, Array.Empty<PublicKeyCredentialDescriptor>());

            harness.ChallengeStore
                .Setup(store => store.ConsumeAsync(ChallengeId))
                .ReturnsAsync(InvokeResult<PasskeyChallengePacket>.Create(new PasskeyChallengePacket
                {
                    Challenge = new PasskeyChallenge
                    {
                        Id = ChallengeId,
                        UserId = UserId,
                        RpId = RpId,
                        Origin = Origin,
                        PasskeyUrl = PasskeyUrl,
                        Purpose = PasskeyChallengePurpose.Register,
                        Challenge = "BAUG",
                        CreatedUtc = DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(4).ToString("O")
                    },
                    OptionsJson = JsonConvert.SerializeObject(options)
                }));
            harness.Fido2
                .Setup(fido => fido.MakeNewCredentialAsync(It.IsAny<MakeNewCredentialParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((MakeNewCredentialParams makeCredential, CancellationToken _) =>
                {
                    capturedMakeCredential = makeCredential;
                    return new RegisteredPublicKeyCredential
                    {
                        Id = new byte[] { 1, 2, 3 },
                        PublicKey = new byte[] { 7, 8, 9 },
                        SignCount = 4,
                        User = options.User
                    };
                });
            harness.CredentialRepo
                .Setup(repo => repo.AddAsync(It.IsAny<PasskeyCredential>()))
                .ReturnsAsync((PasskeyCredential credential) =>
                {
                    persisted = credential;
                    return InvokeResult.Success;
                });

            var result = await harness.Manager.CompleteRegistrationAsync(UserId, CreateRegistrationCompleteRequest(), null, null);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.RedirectURL, Is.EqualTo(PasskeyUrl));
            Assert.That(capturedMakeCredential, Is.Not.Null);
            Assert.That(capturedMakeCredential.OriginalOptions.User.Id, Is.EqualTo(Encoding.UTF8.GetBytes(UserId)));
            Assert.That(persisted, Is.Not.Null);
            Assert.That(persisted.UserId, Is.EqualTo(UserId));
            Assert.That(persisted.RpId, Is.EqualTo(RpId));
            Assert.That(persisted.CredentialId, Is.EqualTo(CredentialId));
            Assert.That(persisted.PublicKey, Is.EqualTo(PublicKey));
            Assert.That(persisted.SignCount, Is.EqualTo(4));
            Assert.That(persisted.Name, Is.Null);
            harness.CredentialRepo.Verify(repo => repo.AddAsync(It.IsAny<PasskeyCredential>()), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", RemoveEvidence)]
        public async Task RemovePasskey_Should_ScopeRemovalToCurrentUserAndRelyingParty()
        {
            var harness = CreateHarness();
            harness.CredentialRepo
                .Setup(repo => repo.RemovePasskeyCredentialAsync(UserId, RpId, CredentialId))
                .ReturnsAsync(InvokeResult.Success);

            var result = await harness.Manager.RemovePasskeyAsync(UserId, CredentialId, null, null);

            Assert.That(result.Successful, Is.True);
            harness.CredentialRepo.Verify(
                repo => repo.RemovePasskeyCredentialAsync(UserId, RpId, CredentialId),
                Times.Once);
        }

        [Test]
        [Property("AptixEvidence", RemoveEvidence)]
        public async Task RemovePasskey_WhenScopedCredentialDoesNotExist_Should_PropagateFailure()
        {
            var harness = CreateHarness();
            harness.CredentialRepo
                .Setup(repo => repo.RemovePasskeyCredentialAsync(UserId, RpId, CredentialId))
                .ReturnsAsync(InvokeResult.FromError("credential_not_found"));

            var result = await harness.Manager.RemovePasskeyAsync(UserId, CredentialId, null, null);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("credential_not_found"));
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
            var user = new AppUser(Email, "test")
            {
                Id = UserId,
                Email = Email,
                UserName = Email,
                EmailConfirmed = true
            };

            appConfig.SetupGet(config => config.WebAddress).Returns(Origin);

            return new Harness
            {
                AppUserRepo = appUserRepo,
                CredentialRepo = credentialRepo,
                ChallengeStore = challengeStore,
                Fido2 = fido2,
                User = user,
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

        private static CredentialCreateOptions CreateRegistrationOptions(Fido2User user, IReadOnlyList<PublicKeyCredentialDescriptor> excludeCredentials)
        {
            return new CredentialCreateOptions
            {
                Challenge = new byte[] { 4, 5, 6 },
                User = user,
                Rp = null,
                PubKeyCredParams = Array.Empty<PubKeyCredParam>(),
                ExcludeCredentials = excludeCredentials
            };
        }

        private static PasskeyRegistrationCompleteRequest CreateRegistrationCompleteRequest()
        {
            return new PasskeyRegistrationCompleteRequest
            {
                ChallengeId = ChallengeId,
                Attestation = new WebAuthnAttestationWire
                {
                    Id = CredentialId,
                    RawId = CredentialId,
                    Type = "public-key",
                    Response = new WebAuthnAttestationResponseWire
                    {
                        ClientDataJSON = "BAUG",
                        AttestationObject = "BwgJ"
                    }
                }
            };
        }

        private static PasskeyCredential CreateCredential()
        {
            return new PasskeyCredential
            {
                UserId = UserId,
                RpId = RpId,
                CredentialId = CredentialId,
                PublicKey = PublicKey,
                SignCount = 0,
                CreatedUtc = DateTime.UtcNow.AddDays(-1).ToString("O")
            };
        }

        private sealed class Harness
        {
            public Mock<IAppUserRepo> AppUserRepo { get; set; }
            public Mock<IAppUserPasskeyCredentialRepo> CredentialRepo { get; set; }
            public Mock<IPasskeyChallengeStore> ChallengeStore { get; set; }
            public Mock<IFido2> Fido2 { get; set; }
            public AppUser User { get; set; }
            public AppUserPasskeyManager Manager { get; set; }
        }
    }
}
