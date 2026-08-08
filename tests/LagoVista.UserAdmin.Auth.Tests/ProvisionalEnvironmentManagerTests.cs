using LagoVista.Core;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class ProvisionalEnvironmentManagerTests
    {
        [Test]
        public async Task RestoreAsync_Should_RecordActivity_And_SlideExpiration()
        {
            var environment = CreateActiveEnvironment();
            var originalExpiration = environment.ExpiresUtc;
            var harness = CreateHarness();

            harness.EnvironmentRepo.Setup(repo => repo.FindByRecoveryTokenHashAsync(It.IsAny<string>())).ReturnsAsync(environment);
            harness.EnvironmentRepo.Setup(repo => repo.UpdateAsync(environment)).Returns(Task.CompletedTask);

            var result = await harness.Manager.RestoreAsync(new RestoreProvisionalEnvironmentRequest { RecoveryToken = "recovery-token" });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.ProvisionalEnvironmentId, Is.EqualTo(environment.Id));
            Assert.That(environment.LastActivityUtc, Is.GreaterThan(environment.CreatedUtc));
            Assert.That(environment.ExpiresUtc, Is.GreaterThan(originalExpiration));
            harness.EnvironmentRepo.Verify(repo => repo.UpdateAsync(environment), Times.Once);
        }

        [Test]
        public async Task RestoreAsync_Should_Reject_MismatchedContinuityCredentials()
        {
            var recoveryEnvironment = CreateActiveEnvironment();
            var installationEnvironment = CreateActiveEnvironment();
            var harness = CreateHarness();

            harness.EnvironmentRepo.Setup(repo => repo.FindByRecoveryTokenHashAsync(It.IsAny<string>())).ReturnsAsync(recoveryEnvironment);
            harness.EnvironmentRepo.Setup(repo => repo.FindByInstallationIdHashAsync(It.IsAny<string>())).ReturnsAsync(installationEnvironment);

            var result = await harness.Manager.RestoreAsync(new RestoreProvisionalEnvironmentRequest { RecoveryToken = "recovery-token", InstallationId = "installation-id" });

            Assert.That(result.Successful, Is.False);
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
            harness.OrganizationManager.Setup(manager => manager.PurgeProvisionalOrganizationAsync(environment.OrganizationId, environment.AppUserId, environment.SubscriptionId)).ReturnsAsync(InvokeResult.FromError("blocked"));

            var result = await harness.Manager.PurgeAsync(100);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.BlockedCount, Is.EqualTo(1));
            Assert.That(result.Result.DeletedCount, Is.EqualTo(0));
            Assert.That(result.Result.Failures, Has.Count.EqualTo(1));
            harness.EnvironmentRepo.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
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
            var manager = new ProvisionalEnvironmentManager(environmentRepo.Object, userManager.Object, organizationManager.Object, subscriptionManager.Object, subscriptionLevelManager.Object);
            return new Harness(manager, environmentRepo, userManager, organizationManager);
        }

        private sealed class Harness
        {
            public Harness(ProvisionalEnvironmentManager manager, Mock<IProvisionalEnvironmentRepo> environmentRepo, Mock<IUserManager> userManager, Mock<IOrganizationManager> organizationManager)
            {
                Manager = manager;
                EnvironmentRepo = environmentRepo;
                UserManager = userManager;
                OrganizationManager = organizationManager;
            }

            public ProvisionalEnvironmentManager Manager { get; }
            public Mock<IProvisionalEnvironmentRepo> EnvironmentRepo { get; }
            public Mock<IUserManager> UserManager { get; }
            public Mock<IOrganizationManager> OrganizationManager { get; }
        }
    }
}
