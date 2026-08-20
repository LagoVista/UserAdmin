using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class TotpAdministrativeResetIntegrationTests
    {
        private const string Evidence = "auth|auth.test-binding.totp-management.admin-reset|auth.transition.totp-management.admin-reset-success";
        private const string ActorId = "A1111111111111111111111111111111";
        private const string TargetId = "A2222222222222222222222222222222";
        private const string OrgId = "A3333333333333333333333333333333";

        [Test]
        [Property("AptixEvidence", Evidence)]
        [Property("AptixAuthEvents", "TotpAdministrativeResetStart|TotpDisableMfaStart|TotpDisableMfaSuccess|TotpAdministrativeResetSuccess")]
        public async Task OrgAdmin_WithFreshMfa_Should_ResetTargetTotp_AndRecordActorTargetAudit()
        {
            var harness = CreateHarness(freshMfa: true, isOrgAdmin: true);

            var result = await harness.Service.ResetAsync(TargetId, harness.Organization, harness.ActorHeader);

            Assert.That(result.Successful, Is.True);
            Assert.That(harness.Target.TwoFactorEnabled, Is.False);
            Assert.That(harness.Target.AuthenticatorKeySecretId, Is.Null);
            Assert.That(harness.Target.RecoveryCodesSecretId, Is.Null);
            Assert.That(harness.Target.LastMfaDateTimeUtc, Is.Null);
            Assert.That(harness.Target.LastTotpAcceptedTimeStep, Is.EqualTo(0));
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(harness.Target), Times.Once);
            harness.SecureStorage.Verify(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), "auth-secret"), Times.Once);
            harness.SecureStorage.Verify(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), "recovery-secret"), Times.Once);

            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpAdministrativeResetStart,
                AuthLogTypes.TotpDisableMfaStart,
                AuthLogTypes.TotpDisableMfaSuccess,
                AuthLogTypes.TotpAdministrativeResetSuccess
            }));
            Assert.That(harness.Log.Events.First().UserId, Is.EqualTo(ActorId));
            Assert.That(harness.Log.Events.First().Extras, Does.Contain(TargetId));
            Assert.That(harness.Log.Events.Last().UserId, Is.EqualTo(ActorId));
            Assert.That(harness.Log.Events.Last().Extras, Does.Contain(TargetId));
        }

        [Test]
        public async Task OrgAdmin_WithoutFreshMfa_Should_RejectBeforeTargetMutation()
        {
            var harness = CreateHarness(freshMfa: false, isOrgAdmin: true);

            var result = await harness.Service.ResetAsync(TargetId, harness.Organization, harness.ActorHeader);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("step_up_required"));
            Assert.That(harness.Target.TwoFactorEnabled, Is.True);
            harness.SecureStorage.Verify(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>()), Times.Never);
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[]
            {
                AuthLogTypes.TotpAdministrativeResetStart,
                AuthLogTypes.TotpAdministrativeResetFailed
            }));
        }

        [Test]
        public async Task NonAdmin_Should_RejectBeforeTargetMutation()
        {
            var harness = CreateHarness(freshMfa: true, isOrgAdmin: false);

            var result = await harness.Service.ResetAsync(TargetId, harness.Organization, harness.ActorHeader);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("not_authorized"));
            harness.SecureStorage.Verify(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>()), Times.Never);
            harness.AppUserRepo.Verify(repo => repo.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        private static Harness CreateHarness(bool freshMfa, bool isOrgAdmin)
        {
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Loose);
            var secureStorage = new Mock<ISecureStorage>(MockBehavior.Strict);
            var organizationManager = new Mock<IOrganizationManager>(MockBehavior.Strict);
            var log = new RecordingAuthenticationLogManager();
            var organization = EntityHeader.Create(OrgId, "Organization");

            var actor = new AppUser("admin@example.com", "test")
            {
                Id = ActorId,
                UserName = "admin@example.com",
                LastMfaDateTimeUtc = freshMfa ? DateTime.UtcNow.AddMinutes(-2).ToString("O") : DateTime.UtcNow.AddHours(-1).ToString("O")
            };

            var target = new AppUser("target@example.com", "test")
            {
                Id = TargetId,
                UserName = "target@example.com",
                TwoFactorEnabled = true,
                AuthenticatorKeySecretId = "auth-secret",
                RecoveryCodesSecretId = "recovery-secret",
                LastMfaDateTimeUtc = DateTime.UtcNow.ToString("O"),
                LastTotpAcceptedTimeStep = 12345
            };
            target.Organizations.Add(organization);

            appUserRepo.Setup(repo => repo.FindByIdAsync(ActorId)).ReturnsAsync(actor);
            appUserRepo.Setup(repo => repo.FindByIdAsync(TargetId)).ReturnsAsync(target);
            appUserRepo.Setup(repo => repo.UpdateAsync(target)).Returns(Task.CompletedTask);
            organizationManager.Setup(manager => manager.IsUserOrgAdminAsync(OrgId, ActorId)).ReturnsAsync(isOrgAdmin);
            secureStorage.Setup(storage => storage.RemoveUserSecretAsync(It.IsAny<EntityHeader>(), It.IsAny<string>())).ReturnsAsync(InvokeResult.Success);

            var mfaManager = new AppUserMfaManager(
                appUserRepo.Object,
                secureStorage.Object,
                log,
                new Mock<IAdminLogger>().Object,
                new Mock<IAppConfig>().Object,
                new Mock<IDependencyManager>().Object,
                new Mock<ISecurity>().Object);

            var service = new TotpAdministrativeResetService(appUserRepo.Object, mfaManager, organizationManager.Object, log);

            return new Harness
            {
                Service = service,
                AppUserRepo = appUserRepo,
                SecureStorage = secureStorage,
                Log = log,
                Organization = organization,
                ActorHeader = EntityHeader.Create(ActorId, "Admin"),
                Target = target
            };
        }

        private sealed class Harness
        {
            public TotpAdministrativeResetService Service { get; set; }
            public Mock<IAppUserRepo> AppUserRepo { get; set; }
            public Mock<ISecureStorage> SecureStorage { get; set; }
            public RecordingAuthenticationLogManager Log { get; set; }
            public EntityHeader Organization { get; set; }
            public EntityHeader ActorHeader { get; set; }
            public AppUser Target { get; set; }
        }
    }
}
