using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Security
{
    public enum ModuleStatus
    {
        [EnumLabel(Module.ModuleStatus_Development, UserAdminResources.Names.ModuleStatus_Development, typeof(UserAdminResources))]
        Development,

        [EnumLabel(Module.ModuleStatus_Preview, UserAdminResources.Names.ModuleStatus_Preview, typeof(UserAdminResources))]
        Preview,

        [EnumLabel(Module.ModuleStatus_Alpha, UserAdminResources.Names.ModuleStatus_Alpha, typeof(UserAdminResources))]
        Alpha,

        [EnumLabel(Module.ModuleStatus_Beta, UserAdminResources.Names.ModuleStatus_Beta, typeof(UserAdminResources))]
        Beta,

        [EnumLabel(Module.ModuleStatus_Live, UserAdminResources.Names.ModuleStatus_Live, typeof(UserAdminResources))]
        Live,

        [EnumLabel(Module.ModuleStatus_Retired, UserAdminResources.Names.ModuleStatus_Retired, typeof(UserAdminResources))]
        Retired,
    }

    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Module_Title, UserAdminResources.Names.Module_Help, UserAdminResources.Names.Module_Help, 
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), Icon: "icon-ae-coding-metal",
        GetListUrl: "/api/modules", GetUrl: "/api/module/{id}", SaveUrl: "/api/module", DeleteUrl: "/api/module/{id}", FactoryUrl: "/api/module/factory")]
    public class Module : UserAdminModelBase, IKeyedEntity, INamedEntity, IOwnedEntity, IValidateable, IFormDescriptor, IFormDescriptorCol2, IFormDescriptorBottom, ISummaryFactory
    {
        public const string ModuleStatus_Development = "development";
        public const string ModuleStatus_Preview = "preview";
        public const string ModuleStatus_Alpha = "alpha";
        public const string ModuleStatus_Beta = "beta";
        public const string ModuleStatus_Live = "Live";
        public const string ModuleStatus_Retired = "retired";

        public Module()
        {
            Status = EntityHeader<ModuleStatus>.Create(ModuleStatus.Development);
            Features = new List<Feature>();
            Areas = new List<Area>();
            HelplResources = new List<HelpResource>();
            DesktopSupport = true;
            PhoneSupport = true;
            TabletSupport = true;
            AreaCategories = new List<UiCategory>();
            UiCategory = new EntityHeader() { Id = "48C14BE40FDA4E9587EFA66502F05F82", Key = "other", Text = "Other" }; 
        }
       
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

        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Category, WaterMark: UserAdminResources.Names.Common_Category_Select, IsRequired: false, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader UiCategory { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_AreaCategories, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/module/uicategory/factory", ResourceType: typeof(UserAdminResources))]
        public List<UiCategory> AreaCategories { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_DesktopSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool DesktopSupport { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_PhoneSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool PhoneSupport { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_TabletSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool TabletSupport { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_IsLegacyNGX, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsLegacyNGX { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_IsExternalLink, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsExternalLink { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_OpenInNewTab, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool OpenInNewPage { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_Link, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Link { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_HelpResources, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<HelpResource> HelplResources { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_IsForProductLine, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsForProductLine { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_SortOrder, IsRequired:true, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources))]
        public int SortOrder { get; set; }

        public List<Area> Areas { get; set; }

        public List<Feature> Features { get; set; }

        public UserAccess UserAccess { get; set; }


        public ModuleSummary CreateSummary()
        {
            return new ModuleSummary()
            {
                Id = Id,
                Name = Name,
                CardIcon = CardIcon,
                CardTitle = CardTitle,
                CardSummary = CardSummary,
                StatusLabel = Status.Text,
                Status = Status.Id,
                Key = Key,
                SortOrder = SortOrder,
                Description = Description,
                IsPublic = IsPublic,
                Link = Link,
                IsExternalLink = IsExternalLink,
                IsLegacyNGX = IsLegacyNGX,
                OpenInNewPage = OpenInNewPage,
                RestrictByDefault = RestrictByDefault,
                OwnerOrgId = OwnerOrganization.Id,
                UiCategory = UiCategory,
                DesktopSupport = DesktopSupport,
                TabletSupport = TabletSupport,
                PhoneSupport = PhoneSupport,
                IsForProductLine = IsForProductLine,
            };
        }

        public List<string> GetFormFieldsBottom()
        {
            return new List<string>()
            {
                nameof(AreaCategories)
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Status),
                nameof(SortOrder),
                nameof(IsLegacyNGX),
                nameof(Link),
                nameof(IsExternalLink),
                nameof(OpenInNewPage),
                nameof(DesktopSupport),
                nameof(TabletSupport),
                nameof(PhoneSupport),
                nameof(IsForProductLine),
                nameof(Description)
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>() {
                nameof(Name),
                nameof(UiCategory),
                nameof(Key),
                nameof(IsPublic),
                nameof(RestrictByDefault),
                nameof(CardTitle),
                nameof(CardIcon),
                nameof(CardSummary),
              };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Modules_Title, UserAdminResources.Names.Module_Help, UserAdminResources.Names.Module_Help,
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), Icon: "icon-ae-coding-metal",
        GetListUrl: "/api/modules", GetUrl: "/api/module/{id}", SaveUrl: "/api/module", DeleteUrl: "/api/module/{id}", FactoryUrl: "/api/module/factory")]
    public class ModuleSummary : SummaryData
    {
        public string CardIcon { get; set; }
        public string CardTitle { get; set; }
        public string CardSummary { get; set; }
        public string StatusLabel { get; set; }
        public string Status { get; set; }
        public bool OpenInNewPage { get; set; }
        public string Link { get; set; }
        public bool IsLegacyNGX { get; set; }
        public bool IsExternalLink { get; set; }
        public bool IsForProductLine { get; set; }
        public bool DesktopSupport { get; set; }
        public bool PhoneSupport { get; set; }
        public bool TabletSupport { get; set; }

        public EntityHeader UiCategory { get; set; }

        public string OwnerOrgId { get; set; }

        public int SortOrder { get; set; }
        public bool RestrictByDefault { get; set; }

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
}
