using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Geo;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.Core;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Location_Title, UserAdminResources.Names.Organization_Location_Help, 
        UserAdminResources.Names.Organization_Location_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),
        GetListUrl:"/api/org/locations", GetUrl:"/api/org/location/:id", SaveUrl:"/api/org/location", FactoryUrl:"/api/org/location/factory", DeleteUrl:"/api/org/location" , Icon: "icon-fo-internet-2")]
    public class OrgLocation : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IIconEntity, IFormDescriptor, IFormDescriptorAdvanced, IFormDescriptorAdvancedCol2, ISummaryFactory
    {
        public OrgLocation()
        {
            Id = Guid.NewGuid().ToId();
            Icon = "icon-fo-internet-2";
        }

        [FormField(LabelResource: UserAdminResources.Names.Organization, IsRequired:true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader Organization { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Category, FieldType: FieldTypes.Category, WaterMark: UserAdminResources.Names.Common_Category_Select, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public EntityHeader Category { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }


        /// <summary>
        /// Name space to be used for any devices at this location.  It will build upon the accounts name space.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, NamespaceType: NamespaceTypes.Location, NamespaceUniqueMessageResource: UserAdminResources.Names.OrganizationLocation_NamespaceInUse, FieldType: FieldTypes.NameSpace, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public String Namespace { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Location_LocationName, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key, RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        /// <summary>
        /// Latitude and longitude for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_GeoLocation, FieldType: FieldTypes.GeoLocation, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public IGeoLocation GeoLocation { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Location_RoomNumber, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string RoomNumber { get; set; }

        /// <summary>
        /// Primary Address
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_Address1, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public String Addr1 { get; set; }

        /// <summary>
        /// Secondary Address
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_Address2, ResourceType: typeof(UserAdminResources))]
        public String Addr2 { get; set; }
        /// <summary>
        /// City for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_City, ResourceType: typeof(UserAdminResources))]
        public String City { get; set; }
        /// <summary>
        /// State or province for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_State, ResourceType: typeof(UserAdminResources))]
        public String StateProvince { get; set; }
        /// <summary>
        /// Postal code for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_PostalCode,  ResourceType: typeof(UserAdminResources))]
        public String PostalCode { get; set; }

        /// <summary>
        /// Devices for a Location
        /// </summary>
        public List<LocationDevice> Devices { get; set; } = new List<LocationDevice>();

        /// <summary>
        /// Country for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_Country, ResourceType: typeof(UserAdminResources))]
        public String Country { get; set; }

        /// <summary>
        /// Main Contact for this Location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Admin_Contact, IsRequired: false, FieldType:FieldTypes.UserPicker,  WaterMark:UserAdminResources.Names.OrgLocation_AdminContact_Select, 
            ResourceType: typeof(UserAdminResources))]
        public EntityHeader AdminContact { get; set; }

        /// <summary>
        /// The technical contact for this location.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Technical_Contact, IsRequired: false, FieldType: FieldTypes.UserPicker, WaterMark: UserAdminResources.Names.OrgLocation_TechnicalContact_Select, 
            ResourceType: typeof(UserAdminResources))]
        public EntityHeader TechnicalContact { get; set; }


        /// <summary>
        /// A description that can be added to this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType:FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public String Description { get; set; }
        /// <summary>
        /// Notes that can be added to this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType:FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public String Notes { get; set; }
        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

        public OrgLocationSummary CreateSummary()
        {
            return new OrgLocationSummary()
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Icon = Icon,
                IsPublic = IsPublic,
                Key = Key,
                City = City,
                StateProvince = StateProvince,
                RoomNumber = RoomNumber,
                AdminContact = AdminContact
            };
        }        

        public List<string> GetAdvancedFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Category),
                nameof(RoomNumber),
                nameof(Addr1),
                nameof(Addr2),
                nameof(City),
                nameof(StateProvince),
                nameof(Country),
                nameof(Description),
            };
        }

        public List<string> GetAdvancedFieldsCol2()
        {
            return new List<string>()
            {
                nameof(AdminContact),
                nameof(TechnicalContact),
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Category),
                nameof(City),
                nameof(StateProvince),
                nameof(Description),
            };
        }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, Name);
         
        }

        public override string ToString()
        {
            return Namespace;
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }


        public string ToHTML()
        {
            var bldr = new StringBuilder();

            bldr.Append($"{Name}<br/>");
            if(!String.IsNullOrEmpty(Addr1))
                bldr.Append($"{Addr1}<br/>");
            if (!String.IsNullOrEmpty(Addr2))
                bldr.Append($"{Addr2}<br/>");
            if (!String.IsNullOrEmpty(RoomNumber))
                bldr.Append($"{UserAdminResources.Location_RoomNumber}: {RoomNumber}<br/>");

            if (!String.IsNullOrEmpty(City))
                bldr.Append(City);

            if (!String.IsNullOrEmpty(StateProvince))
                bldr.Append(StateProvince);

            if (!String.IsNullOrEmpty(City) || !String.IsNullOrEmpty(StateProvince))
                bldr.Append("<br />");

            return bldr.ToString();
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Locations_Title, UserAdminResources.Names.Organization_Location_Help,
        UserAdminResources.Names.Organization_Location_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources),
        GetListUrl: "/api/org/locations", GetUrl: "/api/org/location/:id", SaveUrl: "/api/org/location", FactoryUrl: "/api/org/location/factory", DeleteUrl: "/api/org/location", Icon: "icon-fo-internet-2")]
    public class OrgLocationSummary : SummaryData
    {
        public EntityHeader AdminContact { get; set; }
        public string RoomNumber { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
    }
}
