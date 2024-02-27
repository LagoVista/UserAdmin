using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Repos.Repos.Account;
using System;

namespace LagoVista.UserAdmin.Models.Apps
{
    public class SystemNotification : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        public string OrgId { get; set; }

        public int Index { get; set; }

        public string Icon { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }


        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public string CreationDate { get; set; }

        public string LastUpdatedBy { get; set; }
        public string LastUpdatedById { get; set; }
        public string LastUpdatedDate { get; set; }


        public InboxItem ToInboxItem()
        {
            return new InboxItem()
            {
                Icon = Icon,
                Link = Link,
                Summary = Summary,
                Index = Index,
                Viewed = true,
                PartitionKey = PartitionKey,
                RowKey = RowKey,
                Title = Title,
                Type = "system",
                Scope = "system"
            };
        }
    }

    public class NewSystemNoficiation
    {
        public bool Public { get; set; }
        public EntityHeader Organiation { get; set; }
        public EntityHeader CreatedByUser { get; set; }

        public string Title { get; set; }
        public string Icon { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }

        public SystemNotification ToSystemNotification()
        {
            var timeStamp = DateTime.UtcNow.ToJSONString();

            return new SystemNotification()
            {
                RowKey = DateTime.UtcNow.ToInverseTicksRowKey(),
                PartitionKey = Public ? "public" : Organiation.Id,

                Icon = Icon,
                Title = Title,
                Summary = Summary,
                Link = Link,

                CreatedBy = CreatedByUser.Text,
                CreatedById = CreatedByUser.Id,

                LastUpdatedBy = CreatedByUser.Text,
                LastUpdatedById = CreatedByUser.Id,

                CreationDate = timeStamp,
                LastUpdatedDate = timeStamp,
            };
        }
    }
}
