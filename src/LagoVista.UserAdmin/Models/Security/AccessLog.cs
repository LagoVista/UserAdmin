using LagoVista.Core.Models;
using System;
using LagoVista.Core;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    public class AccessLog : TableStorageEntity
    {
        public AccessLog(string resource, string resourceId)
        {
            RowKey = DateTime.Now.ToInverseTicksRowKey();
            Resource = resource;
            PartitionKey = resourceId;
            DateStamp = DateTime.Now.ToJSONString();
        }

        public string DateStamp { get; set; }
        public string Resource { get; set; }
        public string OrgName { get; set; }
        public string OrgId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
    }
}
