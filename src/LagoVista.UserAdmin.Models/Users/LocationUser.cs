// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7387402c4d97a6ef22419ed5df696605960ae90900310ed11ef7792864e09080
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Users
{
    [EntityDescription(Domains.UserDomain, UserAdminResources.Names.LocationUser_Title, UserAdminResources.Names.LocationUser_Help, UserAdminResources.Names.LocationUser_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class LocationUser : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        /* Id will becomposed o */

        public LocationUser(string orgId, string locationId, string userId)
        {
            RowKey = CreateRowId(locationId, userId);
            PartitionKey = $"{locationId}";
            UserId = userId;
            LocationId = locationId;
            OrganizationId = orgId;
        }

        [FormField(IsRequired: true)]
        public String UserId { get; set; }
        [FormField(IsRequired: true)]
        public String UsersName { get; set; }
        [FormField(IsRequired: true)]
        public String OrganizationId { get; set; }
        [FormField(IsRequired: true)]
        public String OrganizationName { get; set; }
        [FormField(IsRequired: true)]
        public String LocationId { get; set; }
        [FormField(IsRequired: true)]
        public String LocationName { get; set; }
        [FormField(IsRequired: true)]
        public String ProfileImageUrl { get; set; }
        [FormField(IsRequired: true)]
        public String Email { get; set; }
        [FormField(IsRequired: true)]
        public String CreatedBy { get; set; }
        [FormField(IsRequired: true)]
        public String CreatedById { get; set; }
        [FormField(IsRequired: true)]
        public String CreationDate { get; set; }
        [FormField(IsRequired: true)]
        public String LastUpdatedBy { get; set; }
        [FormField(IsRequired: true)]
        public String LastUpdatedById { get; set; }
        [FormField(IsRequired: true)]
        public String LastUpdatedDate { get; set; }

        public static string CreateRowId(String locationId, String userId)
        {
            return $"{locationId}.{userId}";
        }
    }
}
