using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Models.Security;
using System;

namespace LagoVista.UserAdmin.Repos.OperationalData.SecureLinks
{
    // CQL supports scalar columns; no nested Azure Table entity is serialized.
    public sealed class SecureLinkOperationalRecord : IConditionalOperationalDataRecord
    {
        public string OrganizationId { get; set; }
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public long Version { get; set; }

        public string OrgName { get; set; }
        public string CreatedByUserId { get; set; }
        public string CreatedByUser { get; set; }
        public string OriginalCreationDate { get; set; }
        public string ForUserId { get; set; }
        public string ForUserName { get; set; }
        public string DestinationLink { get; set; }
        public string Generator { get; set; }
        public string Expires { get; set; }
        public bool Expired { get; set; }
        public int AccessCount { get; set; }
        public string LastAccess { get; set; }

        public static SecureLinkOperationalRecord FromModel(SecureLink link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));
            if (String.IsNullOrWhiteSpace(link.PartitionKey) || String.IsNullOrWhiteSpace(link.RowKey))
                throw new ArgumentException("Secure link requires its organization and row key.", nameof(link));
            if (!String.IsNullOrWhiteSpace(link.OrgId) &&
                !String.Equals(link.OrgId, link.PartitionKey, StringComparison.Ordinal))
                throw new InvalidOperationException("Secure link organization scope mismatch.");

            return new SecureLinkOperationalRecord
            {
                OrganizationId = link.PartitionKey,
                Id = link.RowKey,
                OrgName = link.OrgName,
                CreatedByUserId = link.CreatedByUserId,
                CreatedByUser = link.CreatedByUser,
                OriginalCreationDate = link.CreationDate,
                ForUserId = link.ForUserId,
                ForUserName = link.ForUserName,
                DestinationLink = link.DestinationLink,
                Generator = link.Generator,
                Expires = link.Expires,
                Expired = link.Expired,
                AccessCount = link.AccessCount,
                LastAccess = link.LastAccess
            };
        }

        public SecureLink ToModel()
        {
            return new SecureLink
            {
                PartitionKey = OrganizationId,
                RowKey = Id,
                OrgId = OrganizationId,
                OrgName = OrgName,
                CreatedByUserId = CreatedByUserId,
                CreatedByUser = CreatedByUser,
                CreationDate = OriginalCreationDate,
                ForUserId = ForUserId,
                ForUserName = ForUserName,
                DestinationLink = DestinationLink,
                Generator = Generator,
                Expires = Expires,
                Expired = Expired,
                AccessCount = AccessCount,
                LastAccess = LastAccess
            };
        }
    }
}
