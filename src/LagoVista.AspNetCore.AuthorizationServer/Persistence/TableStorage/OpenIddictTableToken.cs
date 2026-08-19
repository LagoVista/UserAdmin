using LagoVista.CloudStorage.Storage;
using System;

namespace LagoVista.AspNetCore.AuthorizationServer.Persistence.TableStorage
{
    /// <summary>
    /// Durable OpenIddict token record stored in Azure Table Storage.
    /// Authorization codes are represented by these records so redemption state
    /// can be shared by every authorization-server pod.
    /// </summary>
    public class OpenIddictTableToken : TableStorageEntity
    {
        public const string StorePartitionKey = "OPENIDDICT|TOKEN";

        public string Id { get; set; }
        public string ApplicationId { get; set; }
        public string AuthorizationId { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string ReferenceId { get; set; }
        public string Payload { get; set; }
        public string PropertiesJson { get; set; }
        public string CreationDateUtc { get; set; }
        public string ExpirationDateUtc { get; set; }
        public string RedemptionDateUtc { get; set; }

        public static string CreateRowKey(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            return id;
        }
    }
}
