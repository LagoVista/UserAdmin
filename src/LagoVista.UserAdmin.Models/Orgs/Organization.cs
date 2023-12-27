using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public enum OrgStatuses
    {
        [EnumLabel(Organization.Organization_OrgStatuses_Active, UserAdminResources.Names.Organization_OrgStatuses_Active, typeof(UserAdminResources))]
        Active,
        [EnumLabel(Organization.Organization_OrgStatuses_Deactivated, UserAdminResources.Names.Organization_OrgStatuses_Deactivated, typeof(UserAdminResources))]
        Deactivated,
        [EnumLabel(Organization.Organization_OrgStatuses_Spam, UserAdminResources.Names.Organization_OrgStatuses_Spam, typeof(UserAdminResources))]
        Spam,
    }


    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Title, UserAdminResources.Names.Organization_Help,
        UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),
        SaveUrl: "/api/org", GetUrl: "/api/org/{id}")]
    public class Organization : UserAdminModelBase, INamedEntity, IKeyedEntity, IValidateable, IOwnedEntity, IFormDescriptor
    {
        public const string Organization_OrgStatuses_Active = "active";
        public const string Organization_OrgStatuses_Deactivated = "deactivated";
        public const string Organization_OrgStatuses_Spam = "spam";

        public Organization()
        {
            Locations = new List<EntityHeader>();
            OrgStatus = EntityHeader<OrgStatuses>.Create(OrgStatuses.Active);
            Key = Guid.NewGuid().ToId();
         }

        public string Key { get; set; }

        [FormField(LabelResource:UserAdminResources.Names.Organization_Name, FieldType:FieldTypes.Text, IsRequired:true,ResourceType:typeof(UserAdminResources))]
        public string Name { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, NamespaceType: NamespaceTypes.Organization, NamespaceUniqueMessageResource: UserAdminResources.Names.Organization_NamespaceInUse, 
            FieldType:FieldTypes.NameSpace, IsRequired: true, IsUserEditable:false, ResourceType: typeof(UserAdminResources))]
        public string Namespace { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.Organization_WebSite, FieldType:FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public String WebSite { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Status, IsUserEditable:false, ResourceType: typeof(UserAdminResources))]
        public String Status { get; set; }

        public bool InitializationCompleted { get; set; }
        public string InitializationCompletedDate { get; set; }
        public EntityHeader InitializationCompletedBy { get; set; }

        public EntityHeader<OrgStatuses> OrgStatus { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_Owner, FieldType: FieldTypes.UserPicker, IsRequired: true, IsUserEditable: true,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader Owner { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_Primary_Location, IsRequired: false, IsUserEditable: true,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader PrimaryLocation { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Admin_Contact, FieldType: FieldTypes.UserPicker, IsRequired:false,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader AdminContact { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.Billing_Contact, FieldType: FieldTypes.UserPicker, IsRequired: false,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader BillingContact { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.Technical_Contact, FieldType: FieldTypes.UserPicker, IsRequired: false,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark,  ResourceType: typeof(UserAdminResources))]
        public EntityHeader TechnicalContact { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultProjectLead, HelpResource: UserAdminResources.Names.Organization_DefaultProjectLead_Help, FieldType: FieldTypes.UserPicker,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultProjectLead { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultProjectAdminLead, HelpResource: UserAdminResources.Names.Organization_DefaultProjectAdminLead_Help, FieldType: FieldTypes.UserPicker,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultProjectAdminLead { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultContributor, HelpResource: UserAdminResources.Names.Organization_DefaultContributor_Help, FieldType: FieldTypes.UserPicker,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultContributor { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultQAResource, HelpResource:UserAdminResources.Names.Organization_DefaultQAResource_Help, FieldType: FieldTypes.UserPicker, 
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultQAResource { get; set; }



        [FormField(LabelResource: UserAdminResources.Names.Organization_Locations, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public List<EntityHeader> Locations { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_LandingPage, HelpResource:UserAdminResources.Names.Organization_LandingPage_Help, FieldType:FieldTypes.Text, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string LandingPage { get; set; }

        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }
        public bool IsArchived { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Namespace),
                nameof(Owner),
                nameof(AdminContact),
                nameof(BillingContact),
                nameof(TechnicalContact),
                nameof(DefaultProjectLead),
                nameof(DefaultProjectAdminLead),
                nameof(DefaultContributor),
                nameof(DefaultQAResource),
                nameof(WebSite),
                nameof(LandingPage),
            };
        }

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
