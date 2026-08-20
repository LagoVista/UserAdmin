using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class EmailPasskeyAuthenticationServiceTests
    {
        private const string SuccessEvidence = "auth|auth.test-binding.passkey-sign-in|auth.flow.passkey-sign-in|auth.transition.passkey.complete-authentication";
        private const string UserId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string OrgId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private const string ActorId = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private const string Email = "user@example.com";
        private const string PasskeyUrl = "/auth/continue/passkey";

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        public async Task Begin_WithKnownEmail_Should_ResolveExactUser_AndRequireUserVerification()
        {
            var harness = CreateHarness();
            var response = new PasskeyBeginOptionsResponse { ChallengeId = "challenge-id" };

            harness.AppUserRepo
                .Setup(repo => repo.FindByEmailAsync(Email))
                .ReturnsAsync(harness.User);
            harness.PasskeyManager
                .Setup(manager => manager.BeginAuthenticationOptionsAsync(UserId, true, PasskeyUrl, harness.Organization, harness.Actor))
                .ReturnsAsync(InvokeResult<PasskeyBeginOptionsResponse>.Create(response));

            var result = await harness.Service.BeginAsync($"  {Email}  ", PasskeyUrl, harness.Organization, harness.Actor);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(response));
            Assert.That(result.Result.ChallengeId, Is.EqualTo("challenge-id"));
            harness.AppUserRepo.Verify(repo => repo.FindByEmailAsync(Email), Times.Once);
            harness.PasskeyManager.Verify(manager => manager.BeginAuthenticationOptionsAsync(UserId, true, PasskeyUrl, harness.Organization, harness.Actor), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        public async Task Begin_WithUnknownEmail_Should_NotAskPasskeyManagerForChallenge()
        {
            var harness = CreateHarness();

            harness.AppUserRepo
                .Setup(repo => repo.FindByEmailAsync(Email))
                .ReturnsAsync((AppUser)null);

            var result = await harness.Service.BeginAsync(Email, PasskeyUrl, harness.Organization, harness.Actor);

            Assert.That(result.Successful, Is.False);
            harness.PasskeyManager.Verify(manager => manager.BeginAuthenticationOptionsAsync(
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        public async Task Begin_WhenUserBoundChallengeFails_Should_ReturnNeutralFailure()
        {
            var harness = CreateHarness();

            harness.AppUserRepo
                .Setup(repo => repo.FindByEmailAsync(Email))
                .ReturnsAsync(harness.User);
            harness.PasskeyManager
                .Setup(manager => manager.BeginAuthenticationOptionsAsync(UserId, true, PasskeyUrl, harness.Organization, harness.Actor))
                .ReturnsAsync(InvokeResult<PasskeyBeginOptionsResponse>.FromError("no_passkeys_registered"));

            var result = await harness.Service.BeginAsync(Email, PasskeyUrl, harness.Organization, harness.Actor);

            Assert.That(result.Successful, Is.False);
            harness.PasskeyManager.Verify(manager => manager.BeginAuthenticationOptionsAsync(UserId, true, PasskeyUrl, harness.Organization, harness.Actor), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        public async Task Complete_WithValidProof_Should_BindProofToResolvedUser_AndReturnThatUser()
        {
            var harness = CreateHarness();
            var request = new PasskeyAuthenticationCompleteRequest { ChallengeId = "challenge-id" };

            harness.AppUserRepo
                .Setup(repo => repo.FindByEmailAsync(Email))
                .ReturnsAsync(harness.User);
            harness.PasskeyManager
                .Setup(manager => manager.CompleteAuthenticationAsync(UserId, request, false, harness.Organization, harness.Actor))
                .ReturnsAsync(InvokeResult.Success);

            var result = await harness.Service.CompleteAsync($" {Email} ", request, false, harness.Organization, harness.Actor);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(harness.User));
            harness.AppUserRepo.Verify(repo => repo.FindByEmailAsync(Email), Times.Once);
            harness.PasskeyManager.Verify(manager => manager.CompleteAuthenticationAsync(UserId, request, false, harness.Organization, harness.Actor), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        public async Task Complete_ForMfa_Should_ForwardStepUpToSameUserBoundProof()
        {
            var harness = CreateHarness();
            var request = new PasskeyAuthenticationCompleteRequest { ChallengeId = "challenge-id" };

            harness.AppUserRepo
                .Setup(repo => repo.FindByEmailAsync(Email))
                .ReturnsAsync(harness.User);
            harness.PasskeyManager
                .Setup(manager => manager.CompleteAuthenticationAsync(UserId, request, true, harness.Organization, harness.Actor))
                .ReturnsAsync(InvokeResult.Success);

            var result = await harness.Service.CompleteAsync(Email, request, true, harness.Organization, harness.Actor);

            Assert.That(result.Successful, Is.True);
            harness.PasskeyManager.Verify(manager => manager.CompleteAuthenticationAsync(UserId, request, true, harness.Organization, harness.Actor), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        public async Task Complete_WithUnknownEmail_Should_NotEvaluateAssertion()
        {
            var harness = CreateHarness();
            var request = new PasskeyAuthenticationCompleteRequest { ChallengeId = "challenge-id" };

            harness.AppUserRepo
                .Setup(repo => repo.FindByEmailAsync(Email))
                .ReturnsAsync((AppUser)null);

            var result = await harness.Service.CompleteAsync(Email, request, false, harness.Organization, harness.Actor);

            Assert.That(result.Successful, Is.False);
            harness.PasskeyManager.Verify(manager => manager.CompleteAuthenticationAsync(
                It.IsAny<string>(), It.IsAny<PasskeyAuthenticationCompleteRequest>(), It.IsAny<bool>(), It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>()), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        public async Task Complete_WhenAssertionFails_Should_NotReturnResolvedUser()
        {
            var harness = CreateHarness();
            var request = new PasskeyAuthenticationCompleteRequest { ChallengeId = "challenge-id" };

            harness.AppUserRepo
                .Setup(repo => repo.FindByEmailAsync(Email))
                .ReturnsAsync(harness.User);
            harness.PasskeyManager
                .Setup(manager => manager.CompleteAuthenticationAsync(UserId, request, false, harness.Organization, harness.Actor))
                .ReturnsAsync(InvokeResult.FromError("invalid_assertion"));

            var result = await harness.Service.CompleteAsync(Email, request, false, harness.Organization, harness.Actor);

            Assert.That(result.Successful, Is.False);
            harness.PasskeyManager.Verify(manager => manager.CompleteAuthenticationAsync(UserId, request, false, harness.Organization, harness.Actor), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        public async Task Complete_WithMissingRequest_Should_RejectBeforeResolvingUser()
        {
            var harness = CreateHarness();

            var result = await harness.Service.CompleteAsync(Email, null, false, harness.Organization, harness.Actor);

            Assert.That(result.Successful, Is.False);
            harness.AppUserRepo.Verify(repo => repo.FindByEmailAsync(It.IsAny<string>()), Times.Never);
            harness.PasskeyManager.Verify(manager => manager.CompleteAuthenticationAsync(
                It.IsAny<string>(), It.IsAny<PasskeyAuthenticationCompleteRequest>(), It.IsAny<bool>(), It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>()), Times.Never);
        }

        private static Harness CreateHarness()
        {
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Strict);
            var passkeyManager = new Mock<IAppUserPasskeyManager>(MockBehavior.Strict);
            var user = new AppUser(Email, "test")
            {
                Id = UserId,
                Email = Email,
                UserName = Email,
                EmailConfirmed = true
            };

            var organization = EntityHeader.Create(OrgId, "Test Org");
            var actor = EntityHeader.Create(ActorId, "Test User");

            return new Harness
            {
                AppUserRepo = appUserRepo,
                PasskeyManager = passkeyManager,
                User = user,
                Organization = organization,
                Actor = actor,
                Service = new EmailPasskeyAuthenticationService(appUserRepo.Object, passkeyManager.Object)
            };
        }

        private sealed class Harness
        {
            public Mock<IAppUserRepo> AppUserRepo { get; set; }
            public Mock<IAppUserPasskeyManager> PasskeyManager { get; set; }
            public EmailPasskeyAuthenticationService Service { get; set; }
            public AppUser User { get; set; }
            public EntityHeader Organization { get; set; }
            public EntityHeader Actor { get; set; }
        }
    }
}
