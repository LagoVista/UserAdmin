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

        public String CreationDate { get; set; }
        public String LastAccessTokenRefresh { get; set; }
        public String LastLogin { get; set; }
    }
}
