using LagoVista.CloudStorage.Storage;
using System;

namespace LagoVista.UserAdmin.Repos.Repos.Commo
{
    /// <summary>
    /// External SendGrid message lookup. The external message ID is the Cassandra
    /// partition and clustering key so resolving an email does not require a scan
    /// or knowledge of the recipient's organization.
    /// </summary>
    public class SentEmailXRefOperationalRecord : IOperationalDataRecord
    {
        public string Id { get; set; }
        public string OrganizationId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string SentEmailOrganizationId { get; set; }
        public string SentEmailRowKey { get; set; }
    }

    public class SentEmailXRef
    {
        public string SentEmailRowKey { get; set; }
        public string OrgId { get; set; }
    }
}
