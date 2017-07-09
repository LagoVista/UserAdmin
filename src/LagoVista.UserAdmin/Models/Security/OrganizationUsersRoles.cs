using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.OrganizationUserRole_Title, UserAdminResources.Names.OrganizationUserRole_Title, UserAdminResources.Names.OrganizationUserRole_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class OrganizationUserRole : TableStorageEntity
    {
        public OrganizationUserRole(EntityHeader organization, EntityHeader user)
        {
            PartitionKey = organization.Id;
            OrganizationId = organization.Id;
            OrganizationName = organization.Text;
            UserId = user.Id;
            UserName = user.Text;
            RowKey = GetRowKey();
        }

        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string RoleName { get; set; }
        public string RoleId { get; set; }

        public string GetRowKey(string orgId, string userId)
        {
            return $"{orgId}.{userId}";
        }

        public string GetRowKey()
        {
            return $"{OrganizationId}.{UserId}";
        }

        public EntityHeader ToEntityHeader()
        {
            return new EntityHeader()
            {
                Id = RoleId,
                Text = RoleName
            };
        }
    }
}
