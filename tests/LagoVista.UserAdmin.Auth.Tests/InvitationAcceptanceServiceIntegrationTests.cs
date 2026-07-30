using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class InvitationAcceptanceServiceIntegrationTests
    {
        private const string InvitationAcceptanceEvidence = "auth|auth.test-binding.invitation.accept|auth.flow.invitation.accept|auth.transition.invitation.accept";
        private const string InviteId = "invite-123";
        private const string UserId = "user-123";

        [Test]
        [Property("AptixEvidence", InvitationAcceptanceEvidence)]
        [Property("AptixAuthEvents", "InviteAcceptanceSucceeded")]
        public async Task SuccessfulAcceptance_Should_ConsumeInvitation_AddMembershipOnce_And_PreserveCurrentOrganization()
        {
            var log = new RecordingAuthenticationLogManager();
            var organizationManager = new Mock<IOrganizationManager>(MockBehavior.Strict);
            var invitationRepo = new Mock<IInviteUserRepo>(MockBehavior.Strict);
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Strict);
            var currentOrganization = new OrganizationSummary { Id = "home-org", Name = "Home Organization" };
            var user = new AppUser("user@example.com", "test")
            {
                Id = UserId,
                CurrentOrganization = currentOrganization,
                EmailConfirmed = true
            };
            var invitation = CreateInvitation(Invitation.StatusTypes.Sent);

            appUserRepo.Setup(repo => repo.FindByIdAsync(UserId)).ReturnsAsync(user);
            appUserRepo.Setup(repo => repo.UpdateAsync(user)).Returns(Task.CompletedTask);
            invitationRepo.Setup(repo => repo.GetInvitationAsync(InviteId)).ReturnsAsync(invitation);
            invitationRepo.Setup(repo => repo.UpdateInvitationAsync(invitation)).Returns(Task.CompletedTask);
            organizationManager
                .Setup(manager => manager.AddUserToOrgAsync(user, It.Is<EntityHeader>(org => org.Id == invitation.OrganizationId), It.Is<EntityHeader>(invitedBy => invitedBy.Id == invitation.InvitedById), false, false))
                .ReturnsAsync(InvokeResult.Success);

            var service = new InvitationAcceptanceService(organizationManager.Object, invitationRepo.Object, appUserRepo.Object, log);

            var result = await service.AcceptInvitationAsync(InviteId, UserId);

            Assert.That(result.Successful, Is.True);
            Assert.That(user.CurrentOrganization, Is.SameAs(currentOrganization));
            Assert.That(user.Organizations.Count(org => org.Id == invitation.OrganizationId), Is.EqualTo(1));
            Assert.That(invitation.Accepted, Is.True);
            Assert.That(invitation.Status, Is.EqualTo(Invitation.StatusTypes.Accepted));
            Assert.That(invitation.DateAccepted, Is.Not.Empty);
            Assert.That(log.Events.Select(evt => evt.Type), Does.Contain(AuthLogTypes.InviteAcceptanceSucceeded));
            organizationManager.VerifyAll();
            invitationRepo.VerifyAll();
            appUserRepo.VerifyAll();
        }

        [Test]
        [Property("AptixEvidence", InvitationAcceptanceEvidence)]
        [Property("AptixAuthEvents", "InviteAcceptanceFailed")]
        public async Task AlreadyConsumedInvitation_Should_Not_CreateDuplicateMembership()
        {
            var log = new RecordingAuthenticationLogManager();
            var organizationManager = new Mock<IOrganizationManager>(MockBehavior.Strict);
            var invitationRepo = new Mock<IInviteUserRepo>(MockBehavior.Strict);
            var appUserRepo = new Mock<IAppUserRepo>(MockBehavior.Strict);
            var user = new AppUser("user@example.com", "test") { Id = UserId };
            var invitation = CreateInvitation(Invitation.StatusTypes.Accepted);

            appUserRepo.Setup(repo => repo.FindByIdAsync(UserId)).ReturnsAsync(user);
            invitationRepo.Setup(repo => repo.GetInvitationAsync(InviteId)).ReturnsAsync(invitation);

            var service = new InvitationAcceptanceService(organizationManager.Object, invitationRepo.Object, appUserRepo.Object, log);

            var result = await service.AcceptInvitationAsync(InviteId, UserId);

            Assert.That(result.Successful, Is.False);
            Assert.That(user.Organizations, Is.Empty);
            Assert.That(log.Events.Select(evt => evt.Type), Does.Contain(AuthLogTypes.InviteAcceptanceFailed));
            organizationManager.Verify(manager => manager.AddUserToOrgAsync(It.IsAny<AppUser>(), It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
            invitationRepo.Verify(repo => repo.UpdateInvitationAsync(It.IsAny<Invitation>()), Times.Never);
            appUserRepo.Verify(repo => repo.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        private static Invitation CreateInvitation(Invitation.StatusTypes status)
        {
            return new Invitation
            {
                RowKey = InviteId,
                OrganizationId = "invited-org",
                OrganizationName = "Invited Organization",
                InvitedById = "inviter-123",
                InvitedByName = "Inviting User",
                Status = status
            };
        }
    }
}
