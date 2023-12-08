using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Feature_TItle, UserAdminResources.Names.Feature_Help, UserAdminResources.Names.Feature_Help, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Feature : IFormDescriptor, IFormDescriptorCol2
    {
        public Feature()
        {
            HelplResources = new List<HelpResource>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }



        [FormField(LabelResource: UserAdminResources.Names.Feature_Icon, IsRequired: true, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources))]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_IsLegacyNGX, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsLegacyNGX { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_Link, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Link { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Feature_MenuTitle, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string MenuTitle { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Feature_Summary, IsRequired: true, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Summary { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Menu_DoNotDisplay, HelpResource: UserAdminResources.Names.Menu_DoNotDisplay_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool DoNotDisplay { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_HelpResources, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<HelpResource> HelplResources { get; set; }

        public int SortOrder { get; set; }


        public UserAccess UserAccess { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(RestrictByDefault),
                nameof(MenuTitle),
                nameof(Icon),
                nameof(Summary)
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>() {
                nameof(IsLegacyNGX),
                nameof(Link),
                nameof(DoNotDisplay),
                nameof(Description),
            };
        }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, Key, Name);
        }
    }
}
