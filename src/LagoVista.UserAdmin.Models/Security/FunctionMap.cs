using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{

    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.FunctionMap_Title, UserAdminResources.Names.FunctionMap_Description, UserAdminResources.Names.FunctionMap_Description,
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), Icon: "icon-ae-coding-metal",
        GetListUrl: "/api/function/maps", GetUrl: "/api/function/map/{id}", SaveUrl: "/api/function/map", DeleteUrl: "/api/function//map/{id}", FactoryUrl: "/api/function/map/factory")]
    public class FunctionMap : UserAdminModelBase
    {
        public bool TopLevel { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_IsForProductLine, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsForProductLine { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Icon, IsRequired: true, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources))]
        public string Icon { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }

        public UserAccess UserAccess { get; set; }

        public List<EntityHeader> Parents { get; set; }
    }

    public enum FunctionMapFunctionTypes
    {
        ChildFunctionMap,
        FunctionView,
    }


    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.FunctionMapFunction_Title, UserAdminResources.Names.FunctionMapFunction_Description, UserAdminResources.Names.FunctionMapFunction_Description,
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), Icon: "icon-ae-coding-metal",
        GetListUrl: "/api/function/maps", GetUrl: "/api/function/map/{id}", SaveUrl: "/api/function/map", DeleteUrl: "/api/function//map/{id}", FactoryUrl: "/api/function/map/factory")]
    public class FunctionMapFunction
    {
        public string Id { get; set; }

        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Icon, IsRequired: true, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources))]
        public string Icon { get; set; }


        public int X { get; set; }
        public int Y { get; set; }

        public string Name { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }


        public EntityHeader<FunctionMapFunctionTypes> Type { get; set; }

        public EntityHeader ChildFunctionMap { get; set; }

        public EntityHeader FunctionView { get; set; }


        public List<EntityHeader> DownStream { get; set; } = new List<EntityHeader>();
        public List<EntityHeader> UpStream { get; set; } = new List<EntityHeader>();


        public UserAccess UserAccess { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_IsForProductLine, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsForProductLine { get; set; }
    }

    public class FunctiomMapFunctionView : EntityHeader
    {
        public string Path { get; set; }
    }
}

