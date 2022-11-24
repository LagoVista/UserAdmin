using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Feature_TItle, UserAdminResources.Names.Feature_Help, UserAdminResources.Names.Feature_Help, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Feature
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }

        public UserAccess UserAccess { get; set; }

    }
}
