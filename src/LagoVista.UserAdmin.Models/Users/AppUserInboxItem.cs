// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0829a0788c3e06f317381bfe47d3bfa486166429baf6da4d72daf6e77573fd5f
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Repos.Repos.Account;
using System;

namespace LagoVista.UserAdmin.Models.Users
{
    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.AppUserInboxItem_Name,
        UserAdminResources.Names.AppUserInboxItem_Help,
        UserAdminResources.Names.AppUserInboxItem_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class AppUserInboxItem : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        public bool Viewed { get; set; }
        public string ViewedTimeStamp { get; set; }

        public string UserId { get; set; }
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
                RowKey = RowKey,
                PartitionKey = PartitionKey,
                Title = Title,
                Viewed = Viewed,
                Scope = "user",
                CreationDate = CreationDate,
                CreatedBy = CreatedBy,
            };
        }
    }

    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.NewAppUserInboxItemItem_Name, UserAdminResources.Names.NewAppUserInboxItemItem_Help,
        UserAdminResources.Names.NewAppUserInboxItemItem_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "notifications", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Aux,
        IndexPriority: 30, IndexTagsCsv: "organizationdomain,notifications,runtimeartifact,dto")]
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
