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
            PartitionKey = resourceId;
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
