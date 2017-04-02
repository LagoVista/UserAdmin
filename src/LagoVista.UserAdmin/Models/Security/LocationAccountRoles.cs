using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.LocationAccountRole_Title, UserAdminResources.Names.LocationAccountRole_Help, UserAdminResources.Names.LocationAccountRole_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class LocationAccountRoles : TableStorageEntity
    {
        public EntityHeader Location { get; set; }
        public EntityHeader Account { get; set; }
        public EntityHeader Role { get; set; }
    }
}
