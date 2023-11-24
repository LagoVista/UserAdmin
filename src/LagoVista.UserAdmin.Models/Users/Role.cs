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
        
        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public String Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
         RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public String Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_IsPublic, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsPublic { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Role_IsSystemRole, IsUserEditable:false,  FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsSystemRole { get; set; }

        public List<string> AuthorizedGranterRoles { get; set; } = new List<string>();

        public List<EntityHeader> InheritedRoles { get; set; } = new List<EntityHeader>(); 

        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

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

        public EntityHeader ToEntityHeader()
        {
            return new EntityHeader()
            {
                Id = Id,
                Key = Key,
                Text = Name
            };
        }
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
