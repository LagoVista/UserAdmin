using LagoVista.Core.Models;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using NUnit.Framework;
using System.Linq;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class InvitationAcceptanceUserStateTests
    {
        private const string InvitationAcceptanceEvidence = "auth|auth.test-binding.invitation.accept|auth.flow.invitation.accept|auth.transition.invitation.accept";

        [Test]
        [Property("AptixEvidence", InvitationAcceptanceEvidence)]
        public void ApplyAcceptedMembership_Should_PreserveCurrentOrganization_And_AddMembershipOnce()
        {
            var currentOrganization = new OrganizationSummary { Id = "home-org", Name = "Home Organization" };
            var acceptedOrganization = EntityHeader.Create("invited-org", "Invited Organization");
            var user = new AppUser("user@example.com", "test")
            {
                CurrentOrganization = currentOrganization
            };

            InvitationAcceptanceUserStateUpdater.ApplyAcceptedMembership(user, acceptedOrganization);
            InvitationAcceptanceUserStateUpdater.ApplyAcceptedMembership(user, acceptedOrganization);

            Assert.That(user.CurrentOrganization, Is.SameAs(currentOrganization));
            Assert.That(user.Organizations.Count(org => org.Id == acceptedOrganization.Id), Is.EqualTo(1));
        }

        [Test]
        [Property("AptixEvidence", InvitationAcceptanceEvidence)]
        public void ApplyAcceptedMembership_Should_InitializeMemberships_WithoutSelectingCurrentOrganization()
        {
            var acceptedOrganization = EntityHeader.Create("invited-org", "Invited Organization");
            var user = new AppUser
            {
                Organizations = null,
                CurrentOrganization = null
            };

            InvitationAcceptanceUserStateUpdater.ApplyAcceptedMembership(user, acceptedOrganization);

            Assert.That(user.CurrentOrganization, Is.Null);
            Assert.That(user.Organizations.Select(org => org.Id), Is.EqualTo(new[] { acceptedOrganization.Id }));
        }
    }
}
