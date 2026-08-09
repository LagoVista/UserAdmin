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
    public class AnonymousVisitorPromotionManagerTests
    {
        private Mock<IAnonymousVisitorRepo> _visitorRepo;
        private Mock<IProvisionalEnvironmentManager> _provisionalEnvironmentManager;
        private Mock<IAnonymousVisitorPromotionOptions> _options;

        [SetUp]
        public void Setup()
        {
            _visitorRepo = new Mock<IAnonymousVisitorRepo>();
            _provisionalEnvironmentManager = new Mock<IProvisionalEnvironmentManager>();
            _options = new Mock<IAnonymousVisitorPromotionOptions>();
            _options.SetupGet(options => options.TermsAndConditionsVersion).Returns("2026-08-09");
        }

        [Test]
        public async Task PromoteAsync_Should_Create_Environment_Transfer_Context_And_Retire_Visitor()
        {
            var visitor = CreateActiveVisitor();
            CreateProvisionalEnvironmentRequest createRequest = null;
            _visitorRepo.Setup(repo => repo.GetByActorIdAsync(visitor.ActorId)).ReturnsAsync(visitor);
            _visitorRepo.Setup(repo => repo.UpdateAsync(visitor)).Returns(Task.CompletedTask);
            _provisionalEnvironmentManager.Setup(manager => manager.CreateAsync(It.IsAny<CreateProvisionalEnvironmentRequest>())).Callback<CreateProvisionalEnvironmentRequest>(request => createRequest = request).ReturnsAsync(CreateEnvironmentResult(false));

            var result = await CreateManager().PromoteAsync(visitor.ActorId, "192.0.2.10", new AnonymousVisitorPromotionRequest { TermsAndConditionsAccepted = true, TermsAndConditionsVersion = "2026-08-09", InstallationId = "installation-id" });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.IdentityStage, Is.EqualTo("provisional"));
            Assert.That(result.Result.ActorId, Is.EqualTo(visitor.ActorId));
            Assert.That(result.Result.ProvisionalEnvironmentId, Is.EqualTo("environment-id"));
            Assert.That(result.Result.BootstrapContext, Is.EqualTo("visitor context"));
            Assert.That(result.Result.WasResumed, Is.False);
            Assert.That(createRequest.CreationRequestId, Is.EqualTo($"anonymous-visitor:{visitor.ActorId}"));
            Assert.That(createRequest.BootstrapContext, Is.EqualTo("visitor context"));
            Assert.That(createRequest.TermsAndConditionsAccepted, Is.True);
            Assert.That(createRequest.TermsAndConditionsVersion, Is.EqualTo("2026-08-09"));
            Assert.That(createRequest.TermsAndConditionsAcceptedIPAddress, Is.EqualTo("192.0.2.10"));
            Assert.That(createRequest.ConversionJourneyId, Is.EqualTo("journey-id"));
            Assert.That(createRequest.AgentKey, Is.EqualTo("sales-agent"));
            Assert.That(visitor.State, Is.EqualTo(AnonymousVisitorState.Promoted));
            Assert.That(visitor.ProvisionalEnvironmentId, Is.EqualTo("environment-id"));
            Assert.That(visitor.PromotedUtc.HasValue, Is.True);
            Assert.That(visitor.ContinuityTokenHash, Is.Null);
            Assert.That(visitor.InstallationIdHash, Is.Null);
            _visitorRepo.Verify(repo => repo.UpdateAsync(visitor), Times.Once);
        }

        [Test]
        public async Task PromoteAsync_Should_Resume_Existing_Promotion_Without_Updating_Visitor()
        {
            var visitor = CreateActiveVisitor();
            visitor.State = AnonymousVisitorState.Promoted;
            visitor.ProvisionalEnvironmentId = "environment-id";
            visitor.PromotedUtc = DateTime.UtcNow.AddMinutes(-1);
            visitor.ContinuityTokenHash = null;
            visitor.InstallationIdHash = null;
            _visitorRepo.Setup(repo => repo.GetByActorIdAsync(visitor.ActorId)).ReturnsAsync(visitor);
            _provisionalEnvironmentManager.Setup(manager => manager.CreateAsync(It.Is<CreateProvisionalEnvironmentRequest>(request => request.CreationRequestId == $"anonymous-visitor:{visitor.ActorId}"))).ReturnsAsync(CreateEnvironmentResult(true));

            var result = await CreateManager().PromoteAsync(visitor.ActorId, "192.0.2.10", new AnonymousVisitorPromotionRequest { TermsAndConditionsAccepted = true, TermsAndConditionsVersion = "2026-08-09" });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.WasResumed, Is.True);
            Assert.That(result.Result.ProvisionalEnvironmentId, Is.EqualTo(visitor.ProvisionalEnvironmentId));
            _visitorRepo.Verify(repo => repo.UpdateAsync(It.IsAny<AnonymousVisitor>()), Times.Never);
        }

        [Test]
        public async Task PromoteAsync_Should_Not_Retire_Visitor_When_Provisioning_Fails()
        {
            var visitor = CreateActiveVisitor();
            _visitorRepo.Setup(repo => repo.GetByActorIdAsync(visitor.ActorId)).ReturnsAsync(visitor);
            _provisionalEnvironmentManager.Setup(manager => manager.CreateAsync(It.IsAny<CreateProvisionalEnvironmentRequest>())).ReturnsAsync(InvokeResult<CreateProvisionalEnvironmentResponse>.FromError("provisioning failed"));

            var result = await CreateManager().PromoteAsync(visitor.ActorId, "192.0.2.10", new AnonymousVisitorPromotionRequest { TermsAndConditionsAccepted = true, TermsAndConditionsVersion = "2026-08-09" });

            Assert.That(result.Successful, Is.False);
            Assert.That(visitor.State, Is.EqualTo(AnonymousVisitorState.Active));
            _visitorRepo.Verify(repo => repo.UpdateAsync(It.IsAny<AnonymousVisitor>()), Times.Never);
        }

        [Test]
        public async Task PromoteAsync_Should_Converge_When_Another_Request_Retires_The_Visitor()
        {
            var visitor = CreateActiveVisitor();
            var persistedVisitor = CreateActiveVisitor();
            persistedVisitor.State = AnonymousVisitorState.Promoted;
            persistedVisitor.ProvisionalEnvironmentId = "environment-id";
            persistedVisitor.PromotedUtc = DateTime.UtcNow;
            persistedVisitor.ContinuityTokenHash = null;
            persistedVisitor.InstallationIdHash = null;
            _visitorRepo.SetupSequence(repo => repo.GetByActorIdAsync(visitor.ActorId)).ReturnsAsync(visitor).ReturnsAsync(persistedVisitor);
            _visitorRepo.Setup(repo => repo.UpdateAsync(visitor)).ThrowsAsync(new InvalidOperationException("etag conflict"));
            _provisionalEnvironmentManager.Setup(manager => manager.CreateAsync(It.IsAny<CreateProvisionalEnvironmentRequest>())).ReturnsAsync(CreateEnvironmentResult(true));

            var result = await CreateManager().PromoteAsync(visitor.ActorId, "192.0.2.10", new AnonymousVisitorPromotionRequest { TermsAndConditionsAccepted = true, TermsAndConditionsVersion = "2026-08-09" });

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.ProvisionalEnvironmentId, Is.EqualTo(persistedVisitor.ProvisionalEnvironmentId));
            Assert.That(result.Result.WasResumed, Is.True);
        }

        [Test]
        public async Task PromoteAsync_Should_Require_Affirmative_Versioned_Consent()
        {
            var withoutAcceptance = await CreateManager().PromoteAsync("actor-id", "192.0.2.10", new AnonymousVisitorPromotionRequest { TermsAndConditionsVersion = "2026-08-09" });
            var withoutVersion = await CreateManager().PromoteAsync("actor-id", "192.0.2.10", new AnonymousVisitorPromotionRequest { TermsAndConditionsAccepted = true });

            Assert.That(withoutAcceptance.Successful, Is.False);
            Assert.That(withoutVersion.Successful, Is.False);
            _visitorRepo.Verify(repo => repo.GetByActorIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task PromoteAsync_Should_Reject_A_Stale_Terms_Version()
        {
            var result = await CreateManager().PromoteAsync("actor-id", "192.0.2.10", new AnonymousVisitorPromotionRequest { TermsAndConditionsAccepted = true, TermsAndConditionsVersion = "2026-01-01" });

            Assert.That(result.Successful, Is.False);
            _visitorRepo.Verify(repo => repo.GetByActorIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task PromoteAsync_Should_Expire_An_Overdue_Visitor()
        {
            var visitor = CreateActiveVisitor();
            visitor.ExpiresUtc = DateTime.UtcNow.AddMinutes(-1);
            _visitorRepo.Setup(repo => repo.GetByActorIdAsync(visitor.ActorId)).ReturnsAsync(visitor);
            _visitorRepo.Setup(repo => repo.UpdateAsync(visitor)).Returns(Task.CompletedTask);

            var result = await CreateManager().PromoteAsync(visitor.ActorId, "192.0.2.10", new AnonymousVisitorPromotionRequest { TermsAndConditionsAccepted = true, TermsAndConditionsVersion = "2026-08-09" });

            Assert.That(result.Successful, Is.False);
            Assert.That(visitor.State, Is.EqualTo(AnonymousVisitorState.Expired));
            Assert.That(visitor.ExpiredUtc.HasValue, Is.True);
            _provisionalEnvironmentManager.Verify(manager => manager.CreateAsync(It.IsAny<CreateProvisionalEnvironmentRequest>()), Times.Never);
        }

        private AnonymousVisitorPromotionManager CreateManager()
        {
            return new AnonymousVisitorPromotionManager(_visitorRepo.Object, _provisionalEnvironmentManager.Object, _options.Object);
        }

        private static AnonymousVisitor CreateActiveVisitor()
        {
            var now = DateTime.UtcNow;
            return new AnonymousVisitor
            {
                ActorId = "visitor-actor",
                State = AnonymousVisitorState.Active,
                ContinuityTokenHash = "continuity-hash",
                InstallationIdHash = "installation-hash",
                BootstrapContext = "visitor context",
                CreatedUtc = now.AddHours(-1),
                LastActivityUtc = now.AddMinutes(-5),
                ExpiresUtc = now.AddHours(1),
                StateChangedUtc = now.AddHours(-1),
                ConversionJourneyId = "journey-id",
                AgentKey = "sales-agent"
            };
        }

        private static InvokeResult<CreateProvisionalEnvironmentResponse> CreateEnvironmentResult(bool wasResumed)
        {
            return InvokeResult<CreateProvisionalEnvironmentResponse>.Create(new CreateProvisionalEnvironmentResponse
            {
                ProvisionalEnvironmentId = "environment-id",
                AppUserId = "app-user-id",
                OrganizationId = "organization-id",
                SubscriptionId = "subscription-id",
                RecoveryToken = "recovery-token",
                ExpiresUtc = DateTime.UtcNow.AddDays(30),
                WasResumed = wasResumed
            });
        }
    }
}
