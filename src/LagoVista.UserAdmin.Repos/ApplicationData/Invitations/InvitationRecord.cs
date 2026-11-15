using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Repos.ApplicationData.Invitations
{
    /// <summary>
    /// One durable Mongo document per invitation. No TTL: accepted/declined and
    /// revoked invitations remain available for organization history.
    /// </summary>
    public sealed class InvitationRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }
        public Invitation Invitation { get; set; }
        public string OrgId { get; set; }
        public string EmailKey { get; set; }
        public string StatusKey { get; set; }
        public bool VisibleAsActive { get; set; }
        public DateTime IssuedUtc { get; set; }

        public static NormalizedId32 CreateId(string invitationId)
        {
            if (String.IsNullOrWhiteSpace(invitationId))
                throw new ArgumentException("Invitation ID is required.", nameof(invitationId));
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(
                    "useradmin-invitation:v1:" + invitationId));
                return new NormalizedId32(BitConverter.ToString(bytes, 0, 16).Replace("-", String.Empty));
            }
        }

        public static string NormalizeEmail(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Invitation email is required.", nameof(email));
            return email.Trim().ToUpperInvariant();
        }

        public static bool VisibleActive(Invitation.StatusTypes status)
        {
            return status == Invitation.StatusTypes.New ||
                   status == Invitation.StatusTypes.Sent ||
                   status == Invitation.StatusTypes.Resent ||
                   status == Invitation.StatusTypes.Replaced;
        }

        public static InvitationRecord FromModel(Invitation model, EntityHeader platformOrg)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (String.IsNullOrWhiteSpace(model.OrganizationId) ||
                !String.Equals(model.OrganizationId, model.PartitionKey, StringComparison.Ordinal))
                throw new ArgumentException("Invitation organization and partition must match.", nameof(model));
            if (platformOrg == null || String.IsNullOrWhiteSpace(platformOrg.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for invitations.");

            model.Email = NormalizeEmail(model.Email);
            DateTime dateSent;
            try { dateSent = String.IsNullOrWhiteSpace(model.DateSent) ? DateTime.UtcNow : model.DateSent.ToDateTime().ToUniversalTime(); }
            catch { dateSent = DateTime.UtcNow; }

            return new InvitationRecord
            {
                Id = CreateId(model.RowKey),
                Organization = platformOrg,
                Invitation = model,
                OrgId = model.OrganizationId,
                EmailKey = model.Email,
                StatusKey = model.Status.ToString(),
                VisibleAsActive = VisibleActive(model.Status),
                IssuedUtc = dateSent
            };
        }
    }
}
