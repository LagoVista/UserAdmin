using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Account
{
    [EntityDescription(Domains.UserDomain, UserAdminResources.Names.Role_Title, UserAdminResources.Names.Role_Help, UserAdminResources.Names.Role_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Role : UserAdminModelBase, INamedEntity
    {
        public String Name { get; set; }
        public String OrganizationName { get; set; }
    }
}
