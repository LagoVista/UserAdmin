// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 56c8d8aac08af0411f7cbb9b7afd19997ee9188d9b714fad03878b87dcba69f2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using System;
using LagoVista.Core;

namespace LagoVista.UserAdmin.Models.Security
{
    public class AccessLog : TableStorageEntity
    {
        public AccessLog(string resource, string resourceId)
        {
            RowKey = DateTime.Now.ToInverseTicksRowKey();
            Resource = resource;
            PartitionKey = resourceId.Replace("-", "");
            ResourceId = resourceId;
            DateStamp = DateTime.Now.ToJSONString();
        }

        public string DateStamp { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public string OrgName { get; set; }
        public string OrgId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public bool Authorized { get; set; }
        public string NotAuthorizedReason { get; set; }
    }
}
