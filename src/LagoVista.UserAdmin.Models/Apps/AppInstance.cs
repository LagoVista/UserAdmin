// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f99b489bcea5cd6218c3ce5f86beb4c1a8292b2f81dee1aad9b2c252751620d0
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Apps
{
    public class AppInstance : TableStorageEntity
    {
        public AppInstance(string appInstanceId, string userId)
        {
            RowKey = appInstanceId;
            PartitionKey = userId;
        }

        public String UserId { get; set; }

        public String AppId { get; set; }
        public String ClientType { get; set; }
        public String DeviceId { get; set; }
        public String CreationDate { get; set; }
        public String LastAccessTokenRefresh { get; set; }
        public String LastLogin { get; set; }
    }
}
