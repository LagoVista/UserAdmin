using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Repos.ApplicationData.OrgMemberships
{
    /// <summary>
    /// Durable Mongo association with an explicit tombstone. A deleted membership
    /// must never be resurrected by lazy import of an old Azure Table row.
    /// </summary>
    public sealed class OrgMembershipRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public string OrgId { get; set; }
        public string UserId { get; set; }
        public string RowKey { get; set; }
        public bool IsDeleted { get; set; }

        public string UserName { get; set; }
        public string OrganizationName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Email { get; set; }
        public string EmailKey { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public string OriginalCreationDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedById { get; set; }
        public string OriginalLastUpdatedDate { get; set; }
        public string IsEndUser { get; set; }
        public string Customer { get; set; }
        public string CustomerId { get; set; }
        public string CustomerContact { get; set; }
        public string CustomerContactId { get; set; }
        public bool IsOrgAdmin { get; set; }
        public bool IsAppBuilder { get; set; }
        public string DefaultRoleId { get; set; }
        public string DefaultRole { get; set; }

        public static NormalizedId32 CreateId(string orgId, string userId)
        {
            if (String.IsNullOrWhiteSpace(orgId)) throw new ArgumentException("OrgId is required.", nameof(orgId));
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required.", nameof(userId));
            using (var sha = SHA256.Create())
            {
                var key = "useradmin-orgmembership:v1:" + orgId.Length + ":" + orgId + ":" + userId;
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
                return new NormalizedId32(BitConverter.ToString(hash, 0, 16).Replace("-", String.Empty));
            }
        }

        public static string NormalizeEmail(string email)
            => String.IsNullOrWhiteSpace(email) ? null : email.Trim().ToUpperInvariant();

        public static OrgMembershipRecord Deleted(string orgId, string userId, EntityHeader owner,
            EntityHeader actor)
        {
            return new OrgMembershipRecord
            {
                Id = CreateId(orgId, userId),
                Organization = owner,
                OrgId = orgId,
                UserId = userId,
                RowKey = OrgUser.CreateRowKey(orgId, userId),
                IsDeleted = true,
                LastUpdatedBy = actor?.Text,
                LastUpdatedById = actor?.Id
            };
        }

        public static OrgMembershipRecord FromModel(OrgUser model, EntityHeader owner)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (owner == null || String.IsNullOrWhiteSpace(owner.Id))
                throw new InvalidOperationException("SystemOwnerOrg must be configured for memberships.");
            var orgId = model.OrgId.Value;
            var userId = model.UserId.Value;
            var id = CreateId(orgId, userId);
            if (!String.IsNullOrWhiteSpace(model.PartitionKey) &&
                !String.Equals(model.PartitionKey, orgId, StringComparison.Ordinal))
                throw new ArgumentException("Membership partition must match OrgId.", nameof(model));
            if (!String.IsNullOrWhiteSpace(model.RowKey) &&
                !String.Equals(model.RowKey, OrgUser.CreateRowKey(orgId, userId), StringComparison.Ordinal))
                throw new ArgumentException("Membership row key must match org/user.", nameof(model));
            return new OrgMembershipRecord
            {
                Id = id,
                Organization = owner,
                OrgId = orgId,
                UserId = userId,
                RowKey = OrgUser.CreateRowKey(orgId, userId),
                UserName = model.UserName,
                OrganizationName = model.OrganizationName,
                ProfileImageUrl = model.ProfileImageUrl,
                Email = model.Email,
                EmailKey = NormalizeEmail(model.Email),
                CreatedBy = model.CreatedBy,
                CreatedById = model.CreatedById,
                OriginalCreationDate = model.CreationDate,
                LastUpdatedBy = model.LastUpdatedBy,
                LastUpdatedById = model.LastUpdatedById,
                OriginalLastUpdatedDate = model.LastUpdatedDate,
                IsEndUser = model.IsEndUser,
                Customer = model.Customer,
                CustomerId = model.CustomerId,
                CustomerContact = model.CustomerContact,
                CustomerContactId = model.CustomerContactId,
                IsOrgAdmin = model.IsOrgAdmin,
                IsAppBuilder = model.IsAppBuilder,
                DefaultRoleId = model.DefaultRoleId,
                DefaultRole = model.DefaultRole
            };
        }

        public OrgUser ToModel()
        {
            if (IsDeleted) return null;
            var member = new OrgUser(new NormalizedId32(OrgId), new NormalizedId32(UserId))
            {
                UserName = UserName,
                OrganizationName = OrganizationName,
                ProfileImageUrl = ProfileImageUrl,
                Email = Email,
                CreatedBy = CreatedBy,
                CreatedById = CreatedById,
                CreationDate = OriginalCreationDate,
                LastUpdatedBy = LastUpdatedBy,
                LastUpdatedById = LastUpdatedById,
                LastUpdatedDate = OriginalLastUpdatedDate,
                IsEndUser = IsEndUser,
                Customer = Customer,
                CustomerId = CustomerId,
                CustomerContact = CustomerContact,
                CustomerContactId = CustomerContactId,
                IsOrgAdmin = IsOrgAdmin,
                IsAppBuilder = IsAppBuilder,
                DefaultRoleId = DefaultRoleId,
                DefaultRole = DefaultRole
            };
            return member;
        }
    }
}
