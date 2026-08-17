using LagoVista.Core;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class ProvisionalEnvironmentManagerTests
    {
        [Test]
        public async Task CreateAsync_Should_UseTrustedProvisionalSubscriptionBootstrap()
        {
            var harness = CreateHarness();
            var organization = new Organization { Id = Guid.NewGuid().ToId(), Name = "Provisional Workspace" };
            var subscriptionLevel = new SubscriptionLevel { Id = Guid.NewGuid(), Name = "Provisional" };

            harness.EnvironmentRepo.Setup(repo => repo.FindByCreationRequestIdAsync("creation-request")).ReturnsAsync((ProvisionalEnvironment)null);
            harness.EnvironmentRepo.Setup(repo => repo.CreateAsync(It.IsAny<ProvisionalEnvironment>())).Returns(Task.CompletedTask);
            harness.EnvironmentRepo.Setup(repo => repo.UpdateAsync(It.IsAny<ProvisionalEnvironment>())).Returns(Task.CompletedTask);
            harness.UserManager.Setup(manager => manager.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((AppUser)null);
            harness.UserManager.Setup(manager => manager.CreateAsync(It.IsAny<AppUser>())).ReturnsAsync(InvokeResult.Success);
            harness.OrganizationManager.Setup(manager => manager.CreateProvisionalOrganizationAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(InvokeResult<Organization>.Create(organization));
            harness.SubscriptionLevelManager.Setup(manager => manager.GetSubscriptionLevelByKeyAsync(Subscription.SubscriptionKey_Provisional)).ReturnsAsync(subscriptionLevel);
            harness.SubscriptionManager.Setup(manager => manager.EnsureProvisionalSubscriptionAsync(It.IsAny<Subscription>(), It.IsAny<LagoVista.Core.Models.EntityHeader>(), It.IsAny<LagoVista.Core.Models.EntityHeader>())).ReturnsAsync(InvokeResult.Success);

            var result = await harness.Manager.CreateAsync(new CreateProvisionalEnvironmentRequest { CreationRequestId = "creation-request" });

            Assert.That(result.Successful, Is.True);
            harness.SubscriptionManager.Verify(manager => manager.EnsureProvisionalSubscriptionAsync(
                It.Is<Subscription>(subscription => subscription.Id == result.Result.SubscriptionId && subscription.Key == Subscription.SubscriptionKey_Provisional),
                It.Is<LagoVista.Core.Models.EntityHeader>(org => org.Id == organization.Id),
                It.IsAny<LagoVista.Core.Models.EntityHeader>()), Times.Once);
            harness.SubscriptionManager.Verify(manager => manager.GetSubscriptionAsync(It.IsAny<GuidString36>(), It.IsAny<LagoVista.Core.Models.EntityHeader>(), It.IsAny<LagoVista.Core.Models.EntityHeader>()), Times.Never);
        }

        [Test]
        public async Task RestoreAsync_Should_RotateCredential_RecordActivity_And_SlideExpiration()
        {
            var environment = CreateActiveEnvironment();
            environment.RecoveryTokenHash = Hash("recovery-token");
            var originalExpiration = environment.ExpiresUtc;
            var harness = CreateHarness();

            harness.EnvironmentRepo.Setup(repo => repo.FindByRecoveryTokenHashAsync(It.IsAny<string>())).ReturnsAsync(environment);
            harness.EnvironmentRepo.Setup(repo => repo.UpdateAsync(environment)).Returns(Task.CompletedTask);

            var result = await harness.Manager.RestoreAsync(new RestoreProvisionalEnvironmentRequest { RecoveryToken = "recovery-token" });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.ProvisionalEnvironmentId, Is.EqualTo(environment.Id));
            Assert.That(result.Result.RecoveryToken, Is.Not.EqualTo("recovery-token"));
            Assert.That(environment.RecoveryTokenHash, Is.EqualTo(Hash(result.Result.RecoveryToken)));
            Assert.That(environment.LastActivityUtc, Is.GreaterThan(environment.CreatedUtc));
            Assert.That(environment.ExpiresUtc, Is.GreaterThan(originalExpiration));
            harness.EnvironmentRepo.Verify(repo => repo.UpdateAsync(environment), Times.Once);
        }

        [Test]
        public async Task RestoreAsync_Should_Reject_InstallationId_Without_RecoveryToken()
        {
            var harness = CreateHarness();
            var result = await harness.Manager.RestoreAsync(new RestoreProvisionalEnvironmentRequest { InstallationId = "installation-id" });

            Assert.That(result.Successful, Is.False);
            harness.EnvironmentRepo.Verify(repo => repo.FindByInstallationIdHashAsync(It.IsAny<string>()), Times.Never);
            harness.EnvironmentRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ProvisionalEnvironment>()), Times.Never);
        }

        [Test]
        public async Task ClaimAsync_Should_Require_CurrentEnvironmentUser()
        {
            var environment = CreateActiveEnvironment();
            var harness = CreateHarness();
            harness.EnvironmentRepo.Setup(repo => repo.GetByIdAsync(environment.Id)).ReturnsAsync(environment);

            var result = await harness.Manager.ClaimAsync(environment.Id, "different-user");

            Assert.That(result.Successful, Is.False);
            harness.UserManager.Verify(manager => manager.FindByIdAsync(It.IsAny<string>()), Times.Never);
            harness.EnvironmentRepo.Verify(repo => repo.UpdateAsync(It.IsAny<ProvisionalEnvironment>()), Times.Never);
        }

        [Test]
        public async Task ClaimAsync_Should_RetireContinuityCredentials_ForEstablishedUser()
        {
            var environment = CreateActiveEnvironment();
            environment.RecoveryTokenHash = "recovery-hash";
            environment.InstallationIdHash = "installation-hash";
            var user = new AppUser("user@example.com", "test") { Id = environment.AppUserId, IsAnonymous = false };
            var harness = CreateHarness();

            harness.EnvironmentRepo.Setup(repo => repo.GetByIdAsync(environment.Id)).ReturnsAsync(environment);
            harness.UserManager.Setup(manager => manager.FindByIdAsync(environment.AppUserId)).ReturnsAsync(user);
            harness.EnvironmentRepo.Setup(repo => repo.UpdateAsync(environment)).Returns(Task.CompletedTask);

            var result = await harness.Manager.ClaimAsync(environment.Id, environment.AppUserId);

            Assert.That(result.Successful, Is.True);
            Assert.That(environment.State, Is.EqualTo(ProvisionalEnvironmentState.Claimed));
            Assert.That(environment.ClaimedUtc, Is.Not.Null);
            Assert.That(environment.RecoveryTokenHash, Is.Null);
            Assert.That(environment.InstallationIdHash, Is.Null);
            harness.EnvironmentRepo.Verify(repo => repo.UpdateAsync(environment), Times.Once);
        }

        [Test]
        public async Task ExpireAsync_Should_SetRetentionWindow_ForDueEnvironment()
        {
            var environment = CreateActiveEnvironment();
            environment.ExpiresUtc = DateTime.UtcNow.AddMinutes(-1);
            var harness = CreateHarness();

            harness.EnvironmentRepo.Setup(repo => repo.GetByStateAsync(ProvisionalEnvironmentState.Active, It.IsAny<DateTime?>(), 100)).ReturnsAsync(new[] { environment });
            harness.EnvironmentRepo.Setup(repo => repo.UpdateAsync(environment)).Returns(Task.CompletedTask);

            var result = await harness.Manager.ExpireAsync(DateTime.UtcNow, 100);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.UpdatedCount, Is.EqualTo(1));
            Assert.That(environment.State, Is.EqualTo(ProvisionalEnvironmentState.Expired));
            Assert.That(environment.ExpiredUtc, Is.Not.Null);
            Assert.That(environment.PurgeAfterUtc, Is.Not.Null);
            Assert.That(environment.PurgeAfterUtc.Value, Is.GreaterThan(environment.ExpiredUtc.Value));
            harness.EnvironmentRepo.Verify(repo => repo.UpdateAsync(environment), Times.Once);
        }

        [Test]
        public async Task PurgeAsync_Should_PreserveEnvironment_WhenOrganizationPurgeIsBlocked()
        {
            var environment = CreateActiveEnvironment();
            environment.State = ProvisionalEnvironmentState.PurgePending;
            var harness = CreateHarness();

            harness.EnvironmentRepo.Setup(repo => repo.GetByStateAsync(ProvisionalEnvironmentState.PurgePending, null, 100)).ReturnsAsync(new[] { environment });
            harness.OrganizationManager.Setup(manager => manager.ValidateProvisionalOrganizationForPurgeAsync(environment.OrganizationId, environment.AppUserId, environment.SubscriptionId)).ReturnsAsync(InvokeResult.FromError("blocked"));

            var result = await harness.Manager.PurgeAsync(100);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.BlockedCount, Is.EqualTo(1));
            Assert.That(result.Result.DeletedCount, Is.EqualTo(0));
            Assert.That(result.Result.Failures, Has.Count.EqualTo(1));
            harness.BillingArchiveRepo.Verify(repo => repo.GetBillingEventsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            harness.EnvironmentRepo.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task PurgeAsync_Should_Archive_RollUp_And_DeleteBillingBeforeEnvironment()
        {
            var environment = CreateActiveEnvironment();
            environment.State = ProvisionalEnvironmentState.PurgePending;
            environment.TermsAndConditionsAccepted = true;
            environment.TermsAndConditionsVersion = "2026-08-09";
            environment.TermsAndConditionsAcceptedIPAddress = "192.0.2.10";
            environment.TermsAndConditionsAcceptedUtc = DateTime.UtcNow.AddDays(-1);
            var billingEvent = new ProvisionalEnvironmentBillingEventArchive { Id = Guid.NewGuid().ToString("D"), SubscriptionId = environment.SubscriptionId, ProductId = Guid.NewGuid().ToString("D"), StartTimestamp = DateTime.UtcNow.AddHours(-1), EndTimestamp = DateTime.UtcNow, ActualCost = 1.25m, Extended = 2.50m };
            var archive = new ProvisionalEnvironmentArchiveWriteResult { ArchivePath = "2026/08/08/archive", BillingEventsSha256 = "hash", BillingEventCount = 1 };
            var harness = CreateHarness();

            harness.EnvironmentRepo.Setup(repo => repo.GetByStateAsync(ProvisionalEnvironmentState.PurgePending, null, 100)).ReturnsAsync(new[] { environment });
            harness.OrganizationManager.Setup(manager => manager.ValidateProvisionalOrganizationForPurgeAsync(environment.OrganizationId, environment.AppUserId, environment.SubscriptionId)).ReturnsAsync(InvokeResult.Success);
            harness.BillingArchiveRepo.Setup(repo => repo.GetBillingEventsAsync(environment.OrganizationId, environment.SubscriptionId)).ReturnsAsync(new[] { billingEvent });
            harness.ArchiveStore.Setup(store => store.WriteAndVerifyAsync(It.IsAny<ProvisionalEnvironmentArchiveWriteRequest>())).ReturnsAsync(archive);
            harness.ArchiveAccountingService.Setup(service => service.EnsureRollupAsync(It.IsAny<ProvisionalEnvironmentArchiveAccountingRequest>())).ReturnsAsync(new ProvisionalEnvironmentArchiveAccountingResult { RollupBillingEventId = Guid.NewGuid().ToString("D") });
            harness.BillingArchiveRepo.Setup(repo => repo.DeleteBillingEventsAsync(environment.OrganizationId, environment.SubscriptionId, It.Is<IReadOnlyCollection<string>>(ids => ids.Count == 1 && ids.Contains(billingEvent.Id)))).ReturnsAsync(1);
            harness.OrganizationManager.Setup(manager => manager.PurgeProvisionalOrganizationAsync(environment.OrganizationId, environment.AppUserId, environment.SubscriptionId)).ReturnsAsync(InvokeResult.Success);
            harness.EnvironmentRepo.Setup(repo => repo.DeleteAsync(environment.Id)).Returns(Task.CompletedTask);

            var result = await harness.Manager.PurgeAsync(100);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.DeletedCount, Is.EqualTo(1));
            Assert.That(result.Result.BlockedCount, Is.EqualTo(0));
            harness.ArchiveStore.Verify(store => store.WriteAndVerifyAsync(It.Is<ProvisionalEnvironmentArchiveWriteRequest>(request => request.Manifest.SchemaVersion == 2 && request.Manifest.ProvisionalEnvironmentId == environment.Id && request.Manifest.TermsAndConditionsAccepted && request.Manifest.TermsAndConditionsVersion == "2026-08-09" && request.Manifest.TotalActualCost == 1.25m && request.Manifest.TotalExtended == 2.50m)), Times.Once);
            harness.EnvironmentRepo.Verify(repo => repo.DeleteAsync(environment.Id), Times.Once);
        }

        private static ProvisionalEnvironment CreateActiveEnvironment()
        {
            var now = DateTime.UtcNow;
            return new ProvisionalEnvironment
            {
                Id = Guid.NewGuid().ToId(),
                State = ProvisionalEnvironmentState.Active,
                AppUserId = Guid.NewGuid().ToId(),
                OrganizationId = Guid.NewGuid().ToId(),
                SubscriptionId = Guid.NewGuid().ToString(),
                CreatedUtc = now.AddDays(-1),
                ActivatedUtc = now.AddDays(-1),
                LastActivityUtc = now.AddDays(-1),
                ExpiresUtc = now.AddDays(1),
                StateChangedUtc = now.AddDays(-1)
            };
        }

        private static Harness CreateHarness()
        {
            var environmentRepo = new Mock<IProvisionalEnvironmentRepo>(MockBehavior.Strict);
            var userManager = new Mock<IUserManager>(MockBehavior.Strict);
            var organizationManager = new Mock<IOrganizationManager>(MockBehavior.Strict);
            var subscriptionManager = new Mock<ISubscriptionManager>(MockBehavior.Strict);
            var subscriptionLevelManager = new Mock<ISubscriptionLevelManager>(MockBehavior.Strict);
            var billingArchiveRepo = new Mock<IProvisionalEnvironmentBillingArchiveRepo>(MockBehavior.Strict);
            var archiveStore = new Mock<IProvisionalEnvironmentArchiveStore>(MockBehavior.Strict);
            var archiveAccountingService = new Mock<IProvisionalEnvironmentArchiveAccountingService>(MockBehavior.Strict);
            var manager = new ProvisionalEnvironmentManager(environmentRepo.Object, userManager.Object, organizationManager.Object, subscriptionManager.Object, subscriptionLevelManager.Object, billingArchiveRepo.Object, archiveStore.Object, archiveAccountingService.Object);
            return new Harness(manager, environmentRepo, userManager, organizationManager, subscriptionManager, subscriptionLevelManager, billingArchiveRepo, archiveStore, archiveAccountingService);
        }

        private static string Hash(string value)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
                var builder = new System.Text.StringBuilder(bytes.Length * 2);
                foreach (var item in bytes) builder.Append(item.ToString("x2"));
                return builder.ToString();
            }
        }

        private sealed class Harness
        {
            public Harness(ProvisionalEnvironmentManager manager, Mock<IProvisionalEnvironmentRepo> environmentRepo, Mock<IUserManager> userManager, Mock<IOrganizationManager> organizationManager, Mock<ISubscriptionManager> subscriptionManager, Mock<ISubscriptionLevelManager> subscriptionLevelManager, Mock<IProvisionalEnvironmentBillingArchiveRepo> billingArchiveRepo, Mock<IProvisionalEnvironmentArchiveStore> archiveStore, Mock<IProvisionalEnvironmentArchiveAccountingService> archiveAccountingService)
            {
                Manager = manager;
                EnvironmentRepo = environmentRepo;
                UserManager = userManager;
                OrganizationManager = organizationManager;
                SubscriptionManager = subscriptionManager;
                SubscriptionLevelManager = subscriptionLevelManager;
                BillingArchiveRepo = billingArchiveRepo;
                ArchiveStore = archiveStore;
                ArchiveAccountingService = archiveAccountingService;
            }

            public ProvisionalEnvironmentManager Manager { get; }
            public Mock<IProvisionalEnvironmentRepo> EnvironmentRepo { get; }
            public Mock<IUserManager> UserManager { get; }
            public Mock<IOrganizationManager> OrganizationManager { get; }
            public Mock<ISubscriptionManager> SubscriptionManager { get; }
            public Mock<ISubscriptionLevelManager> SubscriptionLevelManager { get; }
            public Mock<IProvisionalEnvironmentBillingArchiveRepo> BillingArchiveRepo { get; }
            public Mock<IProvisionalEnvironmentArchiveStore> ArchiveStore { get; }
            public Mock<IProvisionalEnvironmentArchiveAccountingService> ArchiveAccountingService { get; }
        }
    }
}
