using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Repos.ApplicationData.Associations
{
    /// <summary>
    /// Stable Mongo identity for an association, independent of legacy Azure
    /// PartitionKey and RowKey. The length-prefix prevents ambiguous pairs.
    /// </summary>
    public static class AssociationRecordIdentity
    {
        public static NormalizedId32 Create(string kind, string parentId, string memberId)
        {
            if (String.IsNullOrWhiteSpace(kind)) throw new ArgumentException("Association kind is required.", nameof(kind));
            if (String.IsNullOrWhiteSpace(parentId)) throw new ArgumentException("Parent identifier is required.", nameof(parentId));
            if (String.IsNullOrWhiteSpace(memberId)) throw new ArgumentException("Member identifier is required.", nameof(memberId));

            var value = "useradmin-association:v1:" + kind.Length + ":" + kind +
                ":" + parentId.Length + ":" + parentId + ":" + memberId;
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                return new NormalizedId32(BitConverter.ToString(hash, 0, 16).Replace("-", String.Empty));
            }
        }

        public static EntityHeader RequirePlatformOrg(IAppConfig config)
        {
            var org = config?.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg must be configured for association storage.");
            return org;
        }
    }

    /// <summary>Durable team membership. No TTL; deletion is explicit.</summary>
    public sealed class TeamMembershipRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public string TeamId { get; set; }
        public string UserId { get; set; }
        public TeamUser Member { get; set; }

        public static TeamMembershipRecord FromModel(TeamUser member, EntityHeader platformOrg)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (String.IsNullOrWhiteSpace(member.TeamId) || String.IsNullOrWhiteSpace(member.UserId))
                throw new ArgumentException("Team membership requires a team and user.", nameof(member));
            if (platformOrg == null || String.IsNullOrWhiteSpace(platformOrg.Id))
                throw new InvalidOperationException("SystemOwnerOrg must be configured.");

            if (!String.IsNullOrEmpty(member.PartitionKey) &&
                !String.Equals(member.PartitionKey, member.TeamId, StringComparison.Ordinal))
                throw new ArgumentException("Team member partition must match TeamId.", nameof(member));
            if (!String.IsNullOrEmpty(member.RowKey) &&
                !String.Equals(member.RowKey, TeamUser.CreateRowKey(member.TeamId, member.UserId), StringComparison.Ordinal))
                throw new ArgumentException("Team member row key must match the team/user relationship.", nameof(member));

            member.PartitionKey = member.TeamId;
            member.RowKey = TeamUser.CreateRowKey(member.TeamId, member.UserId);
            return new TeamMembershipRecord
            {
                Id = AssociationRecordIdentity.Create("team-user", member.TeamId, member.UserId),
                Organization = platformOrg,
                TeamId = member.TeamId,
                UserId = member.UserId,
                Member = member
            };
        }
    }

    /// <summary>Durable asset-set association. No TTL; deletion is explicit.</summary>
    public sealed class ManagedAssetAssociationRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public string AssetSetId { get; set; }
        public string AssetId { get; set; }
        public ManagedAsset Association { get; set; }

        public static ManagedAssetAssociationRecord FromModel(ManagedAsset member, EntityHeader platformOrg)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (String.IsNullOrWhiteSpace(member.AssetSetId) || String.IsNullOrWhiteSpace(member.AssetId))
                throw new ArgumentException("Managed asset requires asset set and asset identifiers.", nameof(member));
            if (platformOrg == null || String.IsNullOrWhiteSpace(platformOrg.Id))
                throw new InvalidOperationException("SystemOwnerOrg must be configured.");

            // The original ManagedAsset constructor did not populate PartitionKey.
            // Normalize it here instead of depending on the Table Storage convention.
            if (!String.IsNullOrEmpty(member.PartitionKey) &&
                !String.Equals(member.PartitionKey, member.AssetSetId, StringComparison.Ordinal))
                throw new ArgumentException("Managed asset partition must match AssetSetId.", nameof(member));
            if (!String.IsNullOrEmpty(member.RowKey) &&
                !String.Equals(member.RowKey, ManagedAsset.CreateRowKey(member.AssetSetId, member.AssetId), StringComparison.Ordinal))
                throw new ArgumentException("Managed asset row key must match the asset-set relationship.", nameof(member));

            member.PartitionKey = member.AssetSetId;
            member.RowKey = ManagedAsset.CreateRowKey(member.AssetSetId, member.AssetId);
            return new ManagedAssetAssociationRecord
            {
                Id = AssociationRecordIdentity.Create("managed-asset", member.AssetSetId, member.AssetId),
                Organization = platformOrg,
                AssetSetId = member.AssetSetId,
                AssetId = member.AssetId,
                Association = member
            };
        }
    }
}
