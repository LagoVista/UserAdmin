using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Feature_TItle, UserAdminResources.Names.Feature_Help, UserAdminResources.Names.Feature_Help, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Feature
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        public string Name { get; set; }        
        public string Key { get; set; }
        public string Description { get; set; }
    }
}
