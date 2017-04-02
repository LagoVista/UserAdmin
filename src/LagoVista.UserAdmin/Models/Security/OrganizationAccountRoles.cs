using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.OrganizationAccountRole_Title, UserAdminResources.Names.OrganizationAccountRole_Title, UserAdminResources.Names.OrganizationAccountRole_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class OrganizationAccountRoles : TableStorageEntity
    {
        public EntityHeader Location { get; set; }
        public EntityHeader Account { get; set; }
        public EntityHeader Role { get; set; }
    }
}
