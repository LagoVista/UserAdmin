using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{
    [EntityDescription(Domains.MiscDomain, UserAdminResources.Names.AuthView_Title, UserAdminResources.Names.AuthView_Help, UserAdminResources.Names.AuthView_Description,
         EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-ae-computer-programming",
         ListUIUrl: "/sysadmin/testing/views", EditUIUrl: "/sysadmin/testing/view/{id}", CreateUIUrl: "/sysadmin/testing/view/add", PreviewUIUrl: "/sysadmin/testing/view/{id}/preview", 
         SaveUrl: "/api/sys/testing/authview", GetListUrl: "/api/sys/testing/authviews", FactoryUrl: "/api/sys/testing/authview/factory", DeleteUrl: "/api/sys/testing/authview/{id}", GetUrl: "/api/sys/testing/authview/{id}")]
    public class AuthView : EntityBase, ISummaryFactory, IValidateable, IFormDescriptor
    {
        [FormField(LabelResource: UserAdminResources.Names.AuthView_Route, HelpResource: UserAdminResources.Names.AuthView_Route_Help, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Route { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AuthView_ViewId, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string ViewId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AuthView_Actions, HelpResource: UserAdminResources.Names.AuthView_Actions_Help, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/sys/testing/auth/view/action/factory", ResourceType: typeof(UserAdminResources))]
        public List<AuthFieldAction> Actions { get; set; } = new List<AuthFieldAction>();

        [FormField(LabelResource: UserAdminResources.Names.AuthView_Fields,  FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/sys/testing/auth/view/field/factory", ResourceType: typeof(UserAdminResources))]
        public List<AuthViewField> Fields { get; set; } = new List<AuthViewField>();




        public AuthViewSummary CreateSummary()
        {
            var summary = new AuthViewSummary();
            summary.Populate(this);
            summary.ViewId = this.ViewId;
            summary.Route = this.Route;
            return summary;
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(ViewId),
                nameof(Category),
                nameof(Route),
                nameof(Actions),
                nameof(Fields)
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }

    }

    [EntityDescription(Domains.MiscDomain, UserAdminResources.Names.AuthViewField_Title, UserAdminResources.Names.AuthViewField_Help, UserAdminResources.Names.AuthViewField_Description,
        EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), FactoryUrl: "/api/sys/testing/authview/field/factory")]
    public class AuthViewField : IFormDescriptor, IValidateable
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: UserAdminResources.Names.AuthViewField_Name, HelpResource: UserAdminResources.Names.AuthViewField_Name_Help, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.AuthViewField_FieldType, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string FieldType { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.AuthViewField_Finder, HelpResource: UserAdminResources.Names.AuthViewField_Finder_Help, FieldType: FieldTypes.Text, IsRequired:true, ResourceType: typeof(UserAdminResources))]
        public string Finder { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(FieldType),
                nameof(Finder),
            };
        }
    }


    [EntityDescription(
        Domains.MiscDomain,
        UserAdminResources.Names.AuthFieldAction_Title,
        UserAdminResources.Names.AuthFieldAction_Help,
        UserAdminResources.Names.AuthFieldAction_Description,
        EntityDescriptionAttribute.EntityTypes.SimpleModel,
        typeof(UserAdminResources), Icon: "icon-ae-computer-programming", FactoryUrl: "/api/sys/testing/authview/action/factory")]
    public class AuthFieldAction : IFormDescriptor, IValidateable
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: UserAdminResources.Names.AuthFieldAction_Name, HelpResource: UserAdminResources.Names.AuthFieldAction_Name_Help, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AuthFieldAction_Finder, HelpResource: UserAdminResources.Names.AuthFieldAction_Finder_Help, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Finder { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Finder),
            };
        }
    }


    [EntityDescription(Domains.MiscDomain, UserAdminResources.Names.AuthViews_Title, UserAdminResources.Names.AuthView_Help, UserAdminResources.Names.AuthView_Description,
         EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-ae-computer-programming",
         ListUIUrl: "/sysadmin/authviews", EditUIUrl: "/sysadmin/authview/{id}", CreateUIUrl: "/sysadmin/authview/add", PreviewUIUrl: "/sysadmin/authview/{id}/preview",
         SaveUrl: "/api/sys/testing/authview", GetListUrl: "/api/sys/testing/authviews", FactoryUrl: "/api/sys/testing/authview/factory", DeleteUrl: "/api/sys/testing/authview/{id}", GetUrl: "/api/sys/testing/authview/{id}")]
    public class AuthViewSummary : SummaryData
    {
        public string Route { get; set; }
        public string ViewId { get; set; }
    }
}
