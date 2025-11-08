// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 17b1b44fe66d133495357484059d22f2b04eecd83acc44ac95e17acdd694271d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Users
{
    [EntityDescription(Domains.UserDomain, UserAdminResources.Names.Role_Title, UserAdminResources.Names.Role_Help, UserAdminResources.Names.Role_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Role : UserAdminModelBase, IKeyedEntity, IOwnedEntity, IValidateable, INamedEntity
    {
        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Role_IsSystemRole, IsUserEditable:false,  FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsSystemRole { get; set; }

        public List<string> AuthorizedGranterRoles { get; set; } = new List<string>();

        public List<EntityHeader> InheritedRoles { get; set; } = new List<EntityHeader>(); 


        public RoleSummary CreateSummary()
        {
            return new RoleSummary()
            {
                Id = Id,
                Name = Name,
                IsPublic = IsPublic,
                Key = Key,
                OwnerOrganizationId = OwnerOrganization?.Id,
                IsSystemRole = IsSystemRole
            };
        }


        public List<EntityHeader> Pages { get; set; }
    }

    public class RoleSummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public bool IsPublic { get; set; }
        public bool IsSystemRole { get; set; }
        public string OwnerOrganizationId { get; set; }
    }
}
