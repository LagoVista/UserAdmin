using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.OrganizationUserRole_Title, UserAdminResources.Names.Organization_Help, UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class OrgUser : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        /* Id will becomposed o */

        public OrgUser(string orgId, string userId)
        {
            RowKey = CreateRowKey(orgId, userId);
            PartitionKey = $"{orgId}";
            UserId = userId;
            OrgId = orgId;
        }

        [FormField(IsRequired: true)]
        public String UserId { get; set; }
        [FormField(IsRequired: true)]
        public String UserName { get; set; }
        [FormField(IsRequired: true)]
        public String OrgId { get; set; }
        [FormField(IsRequired: true)]
        public String OrganizationName { get; set; }
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

        public bool IsOrgAdmin { get; set; }
        public bool IsAppBuilder { get; set; }

        public static String CreateRowKey(String orgId, String userId)
        {
            return $"{orgId}.{userId}";
        }
    }
}
