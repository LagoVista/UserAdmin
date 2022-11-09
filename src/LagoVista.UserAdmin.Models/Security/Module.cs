using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

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

    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Module_Title, UserAdminResources.Names.Module_Help, UserAdminResources.Names.Module_Help, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Module : UserAdminModelBase, IOwnedEntity, IValidateable
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
            Areas = new List<Area>();
        }


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

        [FormField(LabelResource: UserAdminResources.Names.Common_Status, IsRequired: true, FieldType: FieldTypes.Picker, EnumType:typeof(ModuleStatus), WaterMark: UserAdminResources.Names.ModuleStatus_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<ModuleStatus> Status { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }


        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

        public int SortOrder { get; set; }

        public List<Area> Areas { get; set; }



        public ModuleSummary CreateSummary()
        {
            return new ModuleSummary()
            {
                Id = Id,
                Name = Name,
                CardIcon = CardIcon,
                CardTitle = CardTitle,
                StatusLabel = Status.Text,
                Status = Status.Id,
                Key = Key,
                Description = Description,
                IsPublic = IsPublic,
            };
        }
    }

    public class ModuleSummary : SummaryData
    {
        public string CardIcon { get; set; }
        public string CardTitle { get; set; }
        public string StatusLabel { get; set; }
        public string Status { get; set; }
    }
}
