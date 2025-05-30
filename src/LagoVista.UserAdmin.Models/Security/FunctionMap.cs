using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Resources;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{

    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.FunctionMap_Title, UserAdminResources.Names.FunctionMap_Description, UserAdminResources.Names.FunctionMap_Description,
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), Icon: "icon-ae-coding-metal",
        GetListUrl: "/api/function/maps", GetUrl: "/api/function/map/{id}", SaveUrl: "/api/function/map", DeleteUrl: "/api/function/map/{id}", FactoryUrl: "/api/function/map/factory")]
    public class FunctionMap : UserAdminModelBase, IFormDescriptor
    {
        public bool TopLevel { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_IsForProductLine, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsForProductLine { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Icon, IsRequired: true, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources))]
        public string Icon { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }

        public UserAccess UserAccess { get; set; }

        public List<EntityHeader> Parents { get; set; } = new List<EntityHeader>();

        [FormField(LabelResource: UserAdminResources.Names.FunctionMap_Functions, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/function/map/function/factory", ResourceType: typeof(UserAdminResources))]
        public List<FunctionMapFunction> Functions { get; set; } = new List<FunctionMapFunction>();

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(RestrictByDefault),
                nameof(Functions)
            };
        }
    }

    public enum FunctionMapFunctionTypes
    {
        [EnumLabel(FunctionMapFunction.Type_ChildFunctionMap, UserAdminResources.Names.FunctionMapFunction_Type_ChildFunctionMap, typeof(UserAdminResources))] 
        ChildFunctionMap,
        
        [EnumLabel(FunctionMapFunction.Type_FunctionView, UserAdminResources.Names.FunctionMapFunction_Type_FunctionView, typeof(UserAdminResources))] 
        FunctionView,
    }


    public enum IconSize
    {
        [EnumLabel(FunctionMapFunction.IconSize_Small, UserAdminResources.Names.Common_Icon_Size_Small, typeof(UserAdminResources))]
        Small,
        [EnumLabel(FunctionMapFunction.IconSize_Medium, UserAdminResources.Names.Common_Icon_Size_Medium, typeof(UserAdminResources))]
        Medium,
        [EnumLabel(FunctionMapFunction.IconSize_Large, UserAdminResources.Names.Common_Icon_Size_Large, typeof(UserAdminResources))]
        Large,
        [EnumLabel(FunctionMapFunction.IconSize_Large, UserAdminResources.Names.Common_Icon_Size_ExtraLarge, typeof(UserAdminResources))]
        ExtraLarge
    }

    public enum IconShape
    {
        [EnumLabel(FunctionMapFunction.IconShape_Square, UserAdminResources.Names.FunctionMapFunction_IconShape_Square, typeof(UserAdminResources))]
        Square,
        [EnumLabel(FunctionMapFunction.IconShape_Landscape, UserAdminResources.Names.FunctionMapFunction_IconShape_Landscape, typeof(UserAdminResources))]
        Landscape,
        [EnumLabel(FunctionMapFunction.IconShape_Portrait, UserAdminResources.Names.FunctionMapFunction_IconShape_Portrait, typeof(UserAdminResources))]
        Portrait
    }


    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.FunctionMapFunction_Title, UserAdminResources.Names.FunctionMapFunction_Description, UserAdminResources.Names.FunctionMapFunction_Description,
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), Icon: "icon-ae-coding-metal", FactoryUrl: "/api/function/map/function/factory")]
    public class FunctionMapFunction : IFormDescriptor, IFormConditionalFields
    {
        public const string IconSize_Small = "small";
        public const string IconSize_Medium = "medium";
        public const string IconSize_Large = "large";
        public const string IconSize_ExtraLarge = "xlarge";

        public const string IconShape_Square = "square";
        public const string IconShape_Landscape = "landscape";
        public const string IconShape_Portrait = "portrait";

        public const string Type_ChildFunctionMap = "childfunctionmap";
        public const string Type_FunctionView = "functionview";

        public string Id { get; set; } = Guid.NewGuid().ToId();

        [CloneOptions(false)]
        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Key, HelpResource: LagoVistaCommonStrings.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: LagoVistaCommonStrings.Names.Common_Key_Validation, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Icon, IsRequired: false, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources))]
        public string Icon { get; set; }


        public int X { get; set; }
        public int Y { get; set; }

        [CloneOptions(false)]
        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true, IsUserEditable: true)]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.FunctionMapFunction_Type, WaterMark: UserAdminResources.Names.FunctionMapFunction_Type_Select, IsRequired:true,
            EnumType: typeof(FunctionMapFunctionTypes), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<FunctionMapFunctionTypes> Type { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_IconSize, WaterMark: UserAdminResources.Names.Common_IconSize_Select, EnumType: typeof(IconSize), IsRequired: true, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<IconSize> IconSize { get; set; } = EntityHeader<Security.IconSize>.Create(Security.IconSize.Medium);

        [FormField(LabelResource: UserAdminResources.Names.FunctionMapFunction_IconShape, WaterMark: UserAdminResources.Names.FunctionMapFunction_IconShape_Select, EnumType: typeof(IconShape), IsRequired: true, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<IconShape> IconShape { get; set; } = EntityHeader<Security.IconShape>.Create(Security.IconShape.Square);


        [FormField(LabelResource: UserAdminResources.Names.FunctionMapFunction_ImageIcon, FieldType: FieldTypes.FileUpload, UploadUrl: "/api/media/resource/public/upload", ResourceType: typeof(UserAdminResources))]
        public EntityHeader ImageIcon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.FunctionMapFunction_ChildFunctionMap, WaterMark: UserAdminResources.Names.FunctionMapFunction_ChildFunctionMap_Select,
                    FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader ChildFunctionMap { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.FunctionMapFunction_FuctionView, WaterMark: UserAdminResources.Names.FunctionMapFunction_FuctionView_SelectView,
                    CustomFieldType: "viewpicker", FieldType: FieldTypes.CustomerPicker, ResourceType: typeof(UserAdminResources))]
        public FunctionMapFunctionLinkedView FunctionView { get; set; }


        public List<EntityHeader> DownStream { get; set; } = new List<EntityHeader>();
        public List<EntityHeader> UpStream { get; set; } = new List<EntityHeader>();


        public UserAccess UserAccess { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_IsForProductLine, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsForProductLine { get; set; }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(ChildFunctionMap), nameof(FunctionView) },
                 Conditionals = new List<FormConditional>()
                 {
                     new FormConditional()
                     {
                         Field = nameof(Type),
                         Value = FunctionMapFunction.Type_ChildFunctionMap,
                         RequiredFields = new List<string>() { nameof(ChildFunctionMap) },
                         VisibleFields = new List<string>() { nameof(ChildFunctionMap) },
                     },
                     new FormConditional() {
                         Field = nameof(Type),
                         Value = FunctionMapFunction.Type_FunctionView,
                         RequiredFields = new List<string>() { nameof(FunctionView) },
                         VisibleFields = new List<string>() { nameof(FunctionView) },
                     }
                 }
            };
       }

        [CustomValidator]
        public void Validate(ValidationResult result, Actions action)
        {
            if (EntityHeader.IsNullOrEmpty(ImageIcon) && String.IsNullOrEmpty(Icon))
            {
                result.AddUserError("Either Uplaoded Image Icon or Icon is required.");
            }
        }


        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(ImageIcon),
                nameof(IconSize),
                nameof(IconShape),
                nameof(Type)
            };
        }
    }

    public class FunctionMapFunctionLinkedView : EntityHeader
    {
        public string Path { get; set; }
    }
}

