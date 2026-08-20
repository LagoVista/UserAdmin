using Fido2NetLib;
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class RequestScopedPasskeyManagerTests
    {
        private const string UserId = "R1111111111111111111111111111111";
        private const string ChallengeId = "R2222222222222222222222222222222";
        private const string Email = "request@example.com";
        private const string RpId = "demo.customer.example";
        private const string Origin = "https://demo.customer.example";

        [Test]
        public async Task BeginRegistration_Should_UseRequestScopedRpForChallengeAndFidoOptions()
        {
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Loose);
            var credentialRepo = new Mock<IAppUserPasskeyCredentialRepo>(MockBehavior.Strict);
            var challengeStore = new Mock<IPasskeyChallengeStore>(MockBehavior.Strict);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Strict);
            var logger = new Mock<IAdminLogger>(MockBehavior.Loose);
            var userRegistrationManager = new Mock<IUserRegistrationManager>(MockBehavior.Loose);
            var authLog = new RecordingAuthenticationLogManager();
            var relyingPartyContext = new Mock<IPasskeyRelyingPartyContext>(MockBehavior.Strict);
            PasskeyChallengePacket capturedPacket = null;

            var user = new AppUser(Email, "test")
            {
                Id = UserId,
                Email = Email,
                UserName = Email,
                EmailConfirmed = true
            };

            appUserRepo.Setup(repo => repo.FindByIdAsync(UserId)).ReturnsAsync(user);
            credentialRepo.Setup(repo => repo.GetByUserAsync(UserId, RpId)).ReturnsAsync(System.Array.Empty<PasskeyCredential>());
            challengeStore
                .Setup(store => store.CreateAsync(It.IsAny<PasskeyChallengePacket>()))
                .ReturnsAsync((PasskeyChallengePacket packet) =>
                {
                    capturedPacket = packet;
                    packet.Challenge.Id = ChallengeId;
                    return InvokeResult<PasskeyChallengePacket>.Create(packet);
                });
            relyingPartyContext.SetupGet(context => context.Current).Returns(new PasskeyRelyingParty(RpId, Origin, true));

            var baseFido2Configuration = new Fido2Configuration
            {
                RPID = "localhost",
                RPName = "NuvOS",
                Origins = new HashSet<string> { "https://localhost:5001" }
            };

            using var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var manager = new RequestScopedAppUserPasskeyManager(
                appUserRepo.Object,
                userRegistrationManager.Object,
                authLog,
                credentialRepo.Object,
                challengeStore.Object,
                appConfig.Object,
                logger.Object,
                baseFido2Configuration,
                relyingPartyContext.Object,
                serviceProvider);

            var result = await manager.BeginRegistrationOptionsAsync(UserId, "/auth/passkey/management", null, null);

            Assert.That(result.Successful, Is.True);
            Assert.That(capturedPacket, Is.Not.Null);
            Assert.That(capturedPacket.Challenge.RpId, Is.EqualTo(RpId));
            Assert.That(capturedPacket.Challenge.Origin, Is.EqualTo(Origin));
            Assert.That(result.Result.Options["rp"]?["id"]?.Value<string>(), Is.EqualTo(RpId));
            credentialRepo.Verify(repo => repo.GetByUserAsync(UserId, RpId), Times.Once);
        }
    }
}
