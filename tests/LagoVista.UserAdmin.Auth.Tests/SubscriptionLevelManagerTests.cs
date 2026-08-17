using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Orgs;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class SubscriptionLevelManagerTests
    {
        [Test]
        public async Task EnsureSystemSubscriptionLevelAsync_Should_ReturnExistingLevelByKey()
        {
            var definition = SystemSubscriptionLevels.CreateProvisional();
            var existing = new SubscriptionLevel
            {
                Id = Guid.NewGuid(),
                Key = definition.Key,
                Name = definition.Name,
                IncludedWorkUnits = definition.IncludedWorkUnits,
                IsActive = true
            };
            var repo = new Mock<ISubscriptionLevelRepo>(MockBehavior.Strict);
            repo.Setup(item => item.GetSubscriptionLevelByKeyAsync(definition.Key)).ReturnsAsync(existing);
            var manager = new SubscriptionLevelManager(repo.Object);

            var result = await manager.EnsureSystemSubscriptionLevelAsync(definition);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(existing));
            repo.Verify(item => item.AddSubscriptionLevelAsync(It.IsAny<SubscriptionLevel>()), Times.Never);
        }

        [Test]
        public async Task EnsureSystemSubscriptionLevelAsync_Should_CreateMissingCanonicalLevel()
        {
            var definition = SystemSubscriptionLevels.CreateProvisional();
            var repo = new Mock<ISubscriptionLevelRepo>(MockBehavior.Strict);
            repo.Setup(item => item.GetSubscriptionLevelByKeyAsync(definition.Key)).ReturnsAsync((SubscriptionLevel)null);
            repo.Setup(item => item.GetSubscriptionLevelAsync(definition.Id)).ReturnsAsync((SubscriptionLevel)null);
            repo.Setup(item => item.AddSubscriptionLevelAsync(definition)).Returns(Task.CompletedTask);
            var manager = new SubscriptionLevelManager(repo.Object);

            var result = await manager.EnsureSystemSubscriptionLevelAsync(definition);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(definition));
            Assert.That(result.Result.IncludedWorkUnits, Is.EqualTo(100m));
            Assert.That(result.Result.AllowsOverage, Is.False);
            repo.Verify(item => item.AddSubscriptionLevelAsync(definition), Times.Once);
        }

        [Test]
        public async Task EnsureSystemSubscriptionLevelAsync_Should_ReReadAfterConcurrentCreate()
        {
            var definition = SystemSubscriptionLevels.CreateProvisional();
            var concurrent = SystemSubscriptionLevels.CreateProvisional();
            var repo = new Mock<ISubscriptionLevelRepo>(MockBehavior.Strict);
            repo.SetupSequence(item => item.GetSubscriptionLevelByKeyAsync(definition.Key))
                .ReturnsAsync((SubscriptionLevel)null)
                .ReturnsAsync(concurrent);
            repo.Setup(item => item.GetSubscriptionLevelAsync(definition.Id)).ReturnsAsync((SubscriptionLevel)null);
            repo.Setup(item => item.AddSubscriptionLevelAsync(definition)).ThrowsAsync(new InvalidOperationException("duplicate"));
            var manager = new SubscriptionLevelManager(repo.Object);

            var result = await manager.EnsureSystemSubscriptionLevelAsync(definition);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(concurrent));
        }

        [Test]
        public async Task EnsureSystemSubscriptionLevelAsync_Should_RejectCanonicalIdCollision()
        {
            var definition = SystemSubscriptionLevels.CreateProvisional();
            var conflicting = new SubscriptionLevel
            {
                Id = definition.Id,
                Key = "different-level",
                Name = "Different Level"
            };
            var repo = new Mock<ISubscriptionLevelRepo>(MockBehavior.Strict);
            repo.Setup(item => item.GetSubscriptionLevelByKeyAsync(definition.Key)).ReturnsAsync((SubscriptionLevel)null);
            repo.Setup(item => item.GetSubscriptionLevelAsync(definition.Id)).ReturnsAsync(conflicting);
            var manager = new SubscriptionLevelManager(repo.Object);

            var result = await manager.EnsureSystemSubscriptionLevelAsync(definition);

            Assert.That(result.Successful, Is.False);
            repo.Verify(item => item.AddSubscriptionLevelAsync(It.IsAny<SubscriptionLevel>()), Times.Never);
        }
    }
}
