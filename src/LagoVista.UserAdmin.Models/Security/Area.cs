using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Area_Title, UserAdminResources.Names.Area_Help, UserAdminResources.Names.Area_Help, EntityDescriptionAttribute.EntityTypes.Dto, 
        typeof(UserAdminResources), FactoryUrl: "/api/module/area/factory")]
    public class Area : IFormDescriptor, IFormDescriptorCol2, IFormDescriptorBottom
    {
        public Area()
        {
            Id = Guid.NewGuid().ToId();
            Pages = new List<Page>();
            Status = EntityHeader<ModuleStatus>.Create(ModuleStatus.Development);
            Features = new List<Feature>();
            HelplResources = new List<HelpResource>();
            DesktopSupport = true;
            PhoneSupport = true;
            TabletSupport = true;
            PageCategories = new List<UiCategory>();
        }

        public string Id { get; set; }

        public int SortOrder { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardIcon, IsRequired: true, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources))]
        public string CardIcon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardTitle, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string CardTitle { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardSummary, IsRequired: true, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string CardSummary { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Status, IsRequired: true, FieldType: FieldTypes.Picker, EnumType: typeof(ModuleStatus), WaterMark: UserAdminResources.Names.ModuleStatus_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<ModuleStatus> Status { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource:UserAdminResources.Names.Module_RestrictByDefault_Help,  FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_DesktopSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool DesktopSupport { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_PhoneSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool PhoneSupport { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_TabletSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool TabletSupport { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Category, WaterMark: UserAdminResources.Names.Common_Category_Select, IsRequired: false, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader UiCategory { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Area_PageCategories, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/module/uicategory/factory", ResourceType: typeof(UserAdminResources))]
        public List<UiCategory> PageCategories { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Menu_DoNotDisplay, HelpResource: UserAdminResources.Names.Menu_DoNotDisplay_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool DoNotDisplay { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_IsLegacyNGX, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsLegacyNGX { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_HelpResources, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<HelpResource> HelplResources { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_Link, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Link { get; set; }


        public List<Page> Pages { get; set; } = new List<Page>();

        public List<Feature> Features = new List<Feature>();

        public UserAccess UserAccess { get; set; }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, Key, Name);
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Status),
                nameof(DoNotDisplay),
                nameof(IsLegacyNGX),
                nameof(Link),
                nameof(DesktopSupport),
                nameof(TabletSupport),
                nameof(PhoneSupport),
                nameof(Description)
            };
         }

        public List<string> GetFormFieldsBottom()
        {
            return new List<string>()
            {
                nameof(PageCategories)
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(RestrictByDefault),
                nameof(CardTitle),
                nameof(CardIcon),
                nameof(CardSummary)
            };
        }
    }
}
