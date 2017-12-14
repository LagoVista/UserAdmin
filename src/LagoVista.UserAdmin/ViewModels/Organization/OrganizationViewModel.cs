using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using System;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationViewModels, UserAdminResources.Names.OrganizationVM_Title, UserAdminResources.Names.OrganizationVM_Help, UserAdminResources.Names.OrganizationVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class OrganizationViewModel
    {
        [FormField(LabelResource: UserAdminResources.Names.Organization_Name, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, IsUserEditable: false, FieldType: FieldTypes.NameSpace, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Namespace { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Organization_WebSite, ResourceType: typeof(UserAdminResources))]
        public String WebSite { get; set; }

    }
}
