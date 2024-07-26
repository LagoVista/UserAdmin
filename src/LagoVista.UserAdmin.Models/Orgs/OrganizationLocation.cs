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
using System.Linq;
using LagoVista.Core.Models.Geo;

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

        /// <summary>
        /// Latitude and longitude for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_GeoLocation, FieldType: FieldTypes.GeoLocation, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public GeoLocation GeoLocation { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.OrgLocation_PhoneNumber, FieldType: FieldTypes.Phone, IsRequired: false, ResourceType: typeof(UserAdminResources))]

        public string PhoneNumber { get; set; }


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


        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string Notes { get; set; }
      
        /// <summary>
        /// A description that can be added to this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType:FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public String Description { get; set; }

        public List<OrgLocationDiagramReference> DiagramReferences { get; set; } = new List<OrgLocationDiagramReference>();
       

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
                AdminContact = AdminContact,
                NumberDevices = Devices.Count
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
            };
        }

        public List<string> GetAdvancedFieldsCol2()
        {
            return new List<string>()
            {
                nameof(AdminContact),
                nameof(TechnicalContact),
                nameof(GeoLocation),
                nameof(PhoneNumber),
                nameof(Description),
                nameof(Notes),
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
        public override string ToString()
        {
            return Namespace;
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }


        public string ToHTML(string site)
        {
            var bldr = new StringBuilder();

            bldr.Append($"{Name}<br/>");

            if (!String.IsNullOrEmpty(PhoneNumber))
                bldr.Append($"<a href='tel:{PhoneNumber}'>{PhoneNumber}<br/>");

            if (!String.IsNullOrEmpty(Addr1))
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

            if (GeoLocation != null && GeoLocation.Latitude.HasValue && GeoLocation.Longitude.HasValue)
                bldr.Append($"<a href='https://www.google.com/maps/search/?api=1&query={GeoLocation.Latitude},{GeoLocation.Longitude}'>View on Map</a>");

            if (!String.IsNullOrEmpty(Description))
                bldr.Append($"<div>{Description}</div>");

            if (!String.IsNullOrEmpty(Notes))
                bldr.Append($"<div>{Notes}</div>");

            if (DiagramReferences.Any())
            {
                bldr.Append($"<h3>Diagrams</h3>");
                foreach (var diagram in DiagramReferences)
                {
                    bldr.Append($"<div><a href='{site}/public/diagram/{diagram.LocationDiagram.Id}/{diagram.LocationDiagramLayer.Id}/{diagram.LocationDiagramShape.Id}'>{diagram.LocationDiagram.Text}/{diagram.LocationDiagramShape.Text}/{diagram.LocationDiagramShape.Text}</a></div>");
                }
            }

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

        public int NumberDevices { get; set; }
    }

    public class OrgLocationDiagramReference
    {
        public OrgLocationDiagramReference()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }
        public EntityHeader LocationDiagram { get; set; }
        public EntityHeader LocationDiagramLayer { get; set; }
        public EntityHeader LocationDiagramShape { get; set; }

    }
}
