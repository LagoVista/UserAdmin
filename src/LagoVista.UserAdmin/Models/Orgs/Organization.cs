﻿using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
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

        [FormField(LabelResource:Resources.UserAdminResources.Names.Organization_Name, IsRequired:true,ResourceType:typeof(Resources.UserAdminResources))]
        public string Name { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Namespace, NamespaceType: NamespaceTypes.Organization, NamespaceUniqueMessageResource: Resources.UserAdminResources.Names.Organization_NamespaceInUse, FieldType:FieldTypes.NameSpace, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public string Namespace { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Organization_WebSite, ResourceType: typeof(Resources.UserAdminResources))]
        public String WebSite { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Status, IsUserEditable:false, ResourceType: typeof(Resources.UserAdminResources))]
        public String Status { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Organization_Primary_Location, IsRequired: true, IsUserEditable: false, ResourceType: typeof(Resources.UserAdminResources))]
        public EntityHeader PrimaryLocation { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Admin_Contact, IsRequired:true, ResourceType: typeof(Resources.UserAdminResources))]
        public EntityHeader AdminContact { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Billing_Contact, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public EntityHeader BillingContact { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Technical_Contact, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public EntityHeader TechnicalContact { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.Organization_Locations, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
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
