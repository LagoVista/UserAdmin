// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 197f91258ac8b8cc69c2b035f5a500455a340c5b3a5115be2b156bb3d04ad25c
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.OrganizationUserRole_Title, UserAdminResources.Names.Organization_Help, UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class OrgUser : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        public OrgUser(string orgId, string userId)
        {
            RowKey = CreateRowKey(orgId, userId);
            PartitionKey = $"{orgId}";
            UserId = userId;
            OrgId = orgId;
        }

        public String UserId { get; set; }
        public String UserName { get; set; }
        public String OrgId { get; set; }
        public String OrganizationName { get; set; }
        public String ProfileImageUrl { get; set; }
        public String Email { get; set; }
        public String CreatedBy { get; set; }
        public String CreatedById { get; set; }
        public String CreationDate { get; set; }
        public String LastUpdatedBy { get; set; }
        public String LastUpdatedById { get; set; }
        public String LastUpdatedDate { get; set; }

        public string IsEndUser { get; set; }
        public string Customer { get; set; }
        public string CustomerId { get; set; }
        public string CustomerContact { get; set; }
        public string CustomerContactId { get; set; }

        public bool IsOrgAdmin { get; set; }
        public bool IsAppBuilder { get; set; }

        public string DefaultRoleId { get; set; }
        public string DefaultRole { get; set; }

        public static String CreateRowKey(String orgId, String userId)
        {
            return $"{orgId}.{userId}";
        }
    }
}
