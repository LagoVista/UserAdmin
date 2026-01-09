// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e6489a86dff7cc63eabb90ae2c741736334d9fd2b8966ea0108f5b9dd45936d2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.AI.Models;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Page_Title, UserAdminResources.Names.Page_Help, UserAdminResources.Names.Page_Help, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),
        FactoryUrl: "/api/module/page/factory")]
    public class Page : IFormDescriptor, IFormDescriptorCol2
    {
        public Page()
        {
            Features = new List<Feature>();
            Status = EntityHeader<ModuleStatus>.Create(ModuleStatus.Development);
            Id = Guid.NewGuid().ToId();
            DesktopSupport = true;
            PhoneSupport = true;
            TabletSupport = true;
            HelplResources = new List<HelpResource>();
        }

        public int SortOrder { get; set; }

        public string Id { get; set; }


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


        [FormField(LabelResource: UserAdminResources.Names.Module_IsLegacyNGX, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsLegacyNGX { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_Link, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Link { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_DesktopSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool DesktopSupport { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_PhoneSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool PhoneSupport { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_TabletSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool TabletSupport { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_HelpResources, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<HelpResource> HelplResources { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Category, WaterMark: UserAdminResources.Names.Common_Category_Select, IsRequired: false, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader UiCategory { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Status, IsRequired: true, FieldType: FieldTypes.Picker, EnumType: typeof(ModuleStatus), WaterMark: UserAdminResources.Names.ModuleStatus_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<ModuleStatus> Status { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Menu_DoNotDisplay, HelpResource: UserAdminResources.Names.Menu_DoNotDisplay_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool DoNotDisplay { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Page_IsForProductLine, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsForProductLine { get; set; }

        public List<Feature> Features { get; set; } = new List<Feature>();

        public UserAccess UserAccess { get; set; }

        public Task<List<EntityRagContent>> GetRagContentAsync()
        {
            throw new System.NotImplementedException();
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(UiCategory),
                nameof(Key),
                nameof(RestrictByDefault),
                nameof(CardTitle),
                nameof(CardIcon),
                nameof(CardSummary),
            }; 
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
                nameof(IsForProductLine),
                nameof(Description),
            };
        }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, Key, Name);
        }
    }
}
