using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using System;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.CreateOrganizationVM_Title, UserAdminResources.Names.CreateOrganizationVM_Help, UserAdminResources.Names.CreateOrganizationVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class CreateOrganizationViewModel : LocationViewModel, IValidateable
    {
        [FormField(LabelResource: Resources.UserAdminResources.Names.Organization_Name, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public string Name { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Organization_WebSite, ResourceType: typeof(Resources.UserAdminResources))]
        public String WebSite { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Namespace, FieldType: FieldTypes.NameSpace, NamespaceType: NamespaceTypes.Organization, IsRequired: true, NamespaceUniqueMessageResource: Resources.UserAdminResources.Names.Organization_NamespaceInUse, ResourceType: typeof(Resources.UserAdminResources))]
        public String Namespace { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Namespace, HelpResource: Resources.UserAdminResources.Names.LocationNamespace_Help, NamespaceType: NamespaceTypes.Location, FieldType: FieldTypes.NameSpace, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public String LocationNamespace { get; set; }

        public void MapToOrganization(Models.Orgs.Organization organization)
        {
            organization.Name = Name;
            organization.Namespace = Namespace;
            organization.WebSite = WebSite;
        }

        public override void MapToOrganizationLocation(Models.Orgs.OrgLocation location)
        {
            base.MapToOrganizationLocation(location);
            location.Namespace = LocationNamespace;
        }

    }
}
