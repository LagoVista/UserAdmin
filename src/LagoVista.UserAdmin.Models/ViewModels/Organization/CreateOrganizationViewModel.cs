using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using System;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.Core.Interfaces;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.CreateOrganizationVM_Title, 
        UserAdminResources.Names.CreateOrganizationVM_Help, UserAdminResources.Names.CreateOrganizationVM_Description, 
        EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources),
        Icon: "icon-ae-building", FactoryUrl: "/api/org/factory", SaveUrl: "/api/org", CreateUIUrl: "/organization/createorg")]
    public class CreateOrganizationViewModel :  IValidateable, IFormDescriptor
    {
        [FormField(LabelResource: UserAdminResources.Names.Organization_Name, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_CreateGettingStartedData, FieldType:FieldTypes.CheckBox, 
            HelpResource: UserAdminResources.Names.Organization_CreateGettingStartedData_Help, ResourceType: typeof(UserAdminResources))]
        public bool CreateGettingStartedData { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_WebSite, ResourceType: typeof(UserAdminResources))]
        public String WebSite { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, FieldType: FieldTypes.NameSpace, NamespaceType: NamespaceTypes.Organization, IsRequired: true, NamespaceUniqueMessageResource: UserAdminResources.Names.Organization_NamespaceInUse, ResourceType: typeof(UserAdminResources))]
        public String Namespace { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Namespace),
                nameof(CreateGettingStartedData),
                nameof(WebSite),
            };
        }

        public void MapToOrganization(Models.Orgs.Organization organization)
        {
            organization.Name = Name;
            organization.Namespace = Namespace;
            organization.WebSite = WebSite;
            
        }
    }
}
