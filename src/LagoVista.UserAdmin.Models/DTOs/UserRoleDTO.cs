// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4fb3c633364f1b870d421640a1d7ec0f03c66b0f038fc232cf4f8ef82e9cfe26
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;

namespace LagoVista.UserAdmin.Models.DTOs
{
    [EntityDescription(
        Domains.SecurityDomain, UserAdminResources.Names.UserRoleDTO_Name, UserAdminResources.Names.UserRoleDTO_Help,
        UserAdminResources.Names.UserRoleDTO_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "roles", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Aux,
        IndexPriority: 35, IndexTagsCsv: "securitydomain,roles,dto")]
    public class UserRoleDTO : TableStorageEntity
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string RoleId { get; set; }
        public string RoleKey { get; set; }
        public string RoleName { get; set; }
        public string CreationDate { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }

        public UserRole ToUserRole()
        {
            return new UserRole()
            {
                Id = RowKey,
                User = EntityHeader.Create(UserId, UserName),
                Organization = EntityHeader.Create(OrganizationId, OrganizationName),
                Role = EntityHeader.Create(RoleId, RoleKey, RoleName),
                CreatedBy = EntityHeader.Create(CreatedById, CreatedByName),
                CreationDate = CreationDate
            };
        }
    }
}
