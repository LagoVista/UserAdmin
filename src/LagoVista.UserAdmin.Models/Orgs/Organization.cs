using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Title, UserAdminResources.Names.Organization_Help, UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Organization : UserAdminModelBase, INamedEntity, IValidateable, IOwnedEntity
    {
        public Organization()
        {
            Locations = new List<EntityHeader>();
        }

        [FormField(LabelResource:UserAdminResources.Names.Organization_Name, IsRequired:true,ResourceType:typeof(UserAdminResources))]
        public string Name { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, NamespaceType: NamespaceTypes.Organization, NamespaceUniqueMessageResource: UserAdminResources.Names.Organization_NamespaceInUse, FieldType:FieldTypes.NameSpace, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Namespace { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Organization_WebSite, ResourceType: typeof(UserAdminResources))]
        public String WebSite { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_Status, IsUserEditable:false, ResourceType: typeof(UserAdminResources))]
        public String Status { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Organization_Primary_Location, IsRequired: false, IsUserEditable: true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader PrimaryLocation { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Admin_Contact, IsRequired:true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader AdminContact { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Billing_Contact, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader BillingContact { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Technical_Contact, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader TechnicalContact { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Organization_Locations, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public List<EntityHeader> Locations { get; set; }
        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

        public EntityHeader ToEntityHeader()
        {
            return new EntityHeader()
            {
                Id = Id,
                Text = Name,
            };
        }

        public override string ToString()
        {
            return Namespace;
        }

    }
}
