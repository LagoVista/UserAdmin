using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Repos.ApplicationData.Invitations;
using LagoVista.UserAdmin.Repos.OperationalData.SecureLinks;
using NUnit.Framework;
using System;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class InvitationAndSecureLinkStorageTests
    {
        private static readonly EntityHeader PlatformOrg = new EntityHeader
        {
            Id = "82A7FA17E3E44920A2C12E08F327D589",
            Text = "Platform"
        };

        [Test]
        public void Invitation_Identity_Is_Stable_And_Does_Not_Depend_On_Status()
        {
            var id = Guid.NewGuid().ToId();
            Assert.That(InvitationRecord.CreateId(id).Value.Length, Is.EqualTo(32));
            Assert.That(InvitationRecord.CreateId(id).Value,
                Is.EqualTo(InvitationRecord.CreateId(id).Value));
            Assert.That(InvitationRecord.CreateId(id).Value,
                Is.Not.EqualTo(InvitationRecord.CreateId(Guid.NewGuid().ToId()).Value));
        }

        [Test]
        public void Invitation_Mapping_Normalizes_Email_And_Tracks_Status()
        {
            var invitation = NewInvitation();
            invitation.Email = "Friend@Example.com";
            var record = InvitationRecord.FromModel(invitation, PlatformOrg);
            Assert.That(record.OrgId, Is.EqualTo("org-1"));
            Assert.That(record.EmailKey, Is.EqualTo("FRIEND@EXAMPLE.COM"));
            Assert.That(record.StatusKey, Is.EqualTo(nameof(Invitation.StatusTypes.Sent)));
            Assert.That(record.VisibleAsActive, Is.True);
            Assert.That(record.Invitation.RowKey, Is.EqualTo(invitation.RowKey));
            Assert.That(record.Organization.Id, Is.EqualTo(PlatformOrg.Id));
        }

        [Test]
        public void Invitation_VisibleActive_Preserves_List_Semantics()
        {
            Assert.That(InvitationRecord.VisibleActive(Invitation.StatusTypes.New), Is.True);
            Assert.That(InvitationRecord.VisibleActive(Invitation.StatusTypes.Sent), Is.True);
            Assert.That(InvitationRecord.VisibleActive(Invitation.StatusTypes.Resent), Is.True);
            Assert.That(InvitationRecord.VisibleActive(Invitation.StatusTypes.Replaced), Is.True);
            Assert.That(InvitationRecord.VisibleActive(Invitation.StatusTypes.Accepted), Is.False);
            Assert.That(InvitationRecord.VisibleActive(Invitation.StatusTypes.Revoked), Is.False);
        }

        [Test]
        public void SecureLink_Mapping_Preserves_Access_And_Revocation_State()
        {
            var link = NewSecureLink();
            link.AccessCount = 3;
            link.Expired = true;
            link.LastAccess = "2026-11-15T14:00:00Z";
            var saved = SecureLinkOperationalRecord.FromModel(link);
            var restored = saved.ToModel();
            Assert.That(saved.OrganizationId, Is.EqualTo("org-1"));
            Assert.That(saved.Id, Is.EqualTo("link-1"));
            Assert.That(restored.AccessCount, Is.EqualTo(3));
            Assert.That(restored.LastAccess, Is.EqualTo(link.LastAccess));
            Assert.That(restored.Expired, Is.True);
            Assert.That(restored.Expires, Is.EqualTo(link.Expires));
            Assert.That(restored.DestinationLink, Is.EqualTo(link.DestinationLink));
        }

        [Test]
        public void SecureLinks_Are_Invalid_When_Expired_Or_Revoked()
        {
            var now = DateTime.UtcNow;
            var link = NewSecureLink();
            Assert.That(SecureLinkRepo.IsEligible(link, now), Is.True);

            link.Expired = true;
            Assert.That(SecureLinkRepo.IsEligible(link, now), Is.False);
            link.Expired = false;
            link.Expires = now.AddSeconds(-1).ToJSONString();
            Assert.That(SecureLinkRepo.IsEligible(link, now), Is.False);
            link.Expires = "not-a-valid-date";
            Assert.That(SecureLinkRepo.IsEligible(link, now), Is.False);
        }

        [Test]
        public void SecureLink_Organization_Must_Match_Partition()
        {
            var link = NewSecureLink();
            link.OrgId = "other-org";
            Assert.That(SecureLinkRepo.IsEligible(link, DateTime.UtcNow), Is.False);
            Assert.Throws<InvalidOperationException>(
                () => SecureLinkOperationalRecord.FromModel(link));
        }

        [Test]
        public void SecureLinks_Have_Bounded_Physical_Retention()
        {
            Assert.That(SecureLinkRepo.Retention, Is.EqualTo(TimeSpan.FromDays(365)));
        }

        private static Invitation NewInvitation()
        {
            return new Invitation
            {
                PartitionKey = "org-1",
                RowKey = Guid.NewGuid().ToId(),
                OrganizationId = "org-1",
                Email = "friend@example.com",
                DateSent = DateTime.UtcNow.ToJSONString(),
                Status = Invitation.StatusTypes.Sent
            };
        }

        private static SecureLink NewSecureLink()
        {
            return new SecureLink
            {
                PartitionKey = "org-1",
                RowKey = "link-1",
                OrgId = "org-1",
                OrgName = "Org",
                DestinationLink = "/welcome",
                ForUserId = "user-1",
                Expires = DateTime.UtcNow.AddMinutes(5).ToJSONString()
            };
        }
    }
}
