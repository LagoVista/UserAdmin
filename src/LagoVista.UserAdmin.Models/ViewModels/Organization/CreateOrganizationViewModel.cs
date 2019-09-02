using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using System;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.CreateOrganizationVM_Title, UserAdminResources.Names.CreateOrganizationVM_Help, UserAdminResources.Names.CreateOrganizationVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class CreateOrganizationViewModel :  IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.Organization_Name, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_CreateGettingStartedData, HelpResource: UserAdminResources.Names.Organization_CreateGettingStartedData_Help, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public bool CreateGettingsStartedData { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_WebSite, ResourceType: typeof(UserAdminResources))]
        public String WebSite { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, FieldType: FieldTypes.NameSpace, NamespaceType: NamespaceTypes.Organization, IsRequired: true, NamespaceUniqueMessageResource: UserAdminResources.Names.Organization_NamespaceInUse, ResourceType: typeof(UserAdminResources))]
        public String Namespace { get; set; }

        public void MapToOrganization(Models.Orgs.Organization organization)
        {
            organization.Name = Name;
            organization.Namespace = Namespace;
            organization.WebSite = WebSite;
            
        }
    }
}
