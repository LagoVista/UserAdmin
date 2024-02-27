using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Repos.Repos.Account;
using System;

namespace LagoVista.UserAdmin.Models.Users
{
    public class AppUserInboxItem : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        public bool Viewed { get; set; }        
        public string ViewedTimeStamp { get; set; }


        public String UserId { get; set; }
        public string OrgId { get; set; }

        public string Type { get; set; }
        
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

        public static string ConstructPartitionKey(string orgId, string userId)
        {
            return $"{orgId}-{userId}";
        }

        public InboxItem ToInboxItem()
        {
            return new InboxItem()
            {
                Icon = Icon,
                Link = Link,
                Type = Type,
                Summary = Summary,
                PartitionKey = PartitionKey,
                RowKey = RowKey,
                Title = Title,
                Viewed = Viewed,
                Scope = "user",
            };
        }
    }

    public class NewAppUserInboxItemItem
    {
        public EntityHeader Organiation { get; set; }
        public EntityHeader ForUser { get; set; }
        public EntityHeader CreatedByUser { get; set; }

        public string Type { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }

        public AppUserInboxItem CreateInboxItem()
        {
            var timeStamp = DateTime.UtcNow.ToJSONString();

            return new AppUserInboxItem()
            {
                RowKey = DateTime.UtcNow.ToInverseTicksRowKey(),
                PartitionKey = AppUserInboxItem.ConstructPartitionKey(Organiation.Id, ForUser.Id),

                UserId = ForUser.Id,
                OrgId = Organiation.Id,

                Type = Type,

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
