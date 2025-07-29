using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.Core;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LagoVista.Core.Models.Geo;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Location_Title, UserAdminResources.Names.Organization_Location_Help,
        UserAdminResources.Names.Organization_Location_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),
        GetListUrl: "/api/org/locations", GetUrl: "/api/org/location/{id}", SaveUrl: "/api/org/location", FactoryUrl: "/api/org/location/factory", DeleteUrl: "/api/org/location/{id}", Icon: "icon-fo-office")]
    public class OrgLocation : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IIconEntity, IFormDescriptor, IFormDescriptorAdvanced, IFormDescriptorAdvancedCol2, ISummaryFactory
    {
        public OrgLocation()
        {
            Id = Guid.NewGuid().ToId();
            Icon = "icon-fo-office";
            TimeZone = new EntityHeader()
            {
                Id = "UTC",
                Text = "(UTC) Coordinated Universal Time",
            };
        }

        [FormField(LabelResource: UserAdminResources.Names.Organization, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader Organization { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }


        /// <summary>
        /// NickName space to be used for any devices at this location.  It will build upon the accounts name space.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, NamespaceType: NamespaceTypes.Location, NamespaceUniqueMessageResource: UserAdminResources.Names.OrganizationLocation_NamespaceInUse, FieldType: FieldTypes.NameSpace, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public String Namespace { get; set; }

        /// <summary>
        /// Latitude and longitude for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_GeoLocation, FieldType: FieldTypes.GeoLocation, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public GeoLocation GeoLocation { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.OrgLocation_GeoBoundingBox, HelpResource: UserAdminResources.Names.OrgLocation_GeoBoundingBox_Help, FieldType: FieldTypes.Custom, CustomFieldType: "boudningpolygon", IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public List<GeoLocation> GeoPointsBoundingBox { get; set; } = new List<GeoLocation>();


        [FormField(LabelResource: UserAdminResources.Names.OrgLocation_PhoneNumber, FieldType: FieldTypes.Phone, IsRequired: false, ResourceType: typeof(UserAdminResources))]

        public string PhoneNumber { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Location_RoomNumber, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string RoomNumber { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_TimeZome, IsRequired: true, WaterMark: UserAdminResources.Names.Common_TimeZome_Picker, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader TimeZone { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Customer, IsRequired: false, FieldType: FieldTypes.CustomerPicker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader Customer { get; set; }

        /// <summary>
        /// Primary Address
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Address1, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public String Addr1 { get; set; }

        /// <summary>
        /// Secondary Address
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Address2, ResourceType: typeof(UserAdminResources))]
        public String Addr2 { get; set; }
        /// <summary>
        /// City for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_City, ResourceType: typeof(UserAdminResources))]
        public String City { get; set; }
        /// <summary>
        /// State or province for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_State, ResourceType: typeof(UserAdminResources))]
        public String StateProvince { get; set; }
        /// <summary>
        /// Postal code for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_PostalCode, ResourceType: typeof(UserAdminResources))]
        public String PostalCode { get; set; }

        /// <summary>
        /// Devices for a Location
        /// </summary>
        public List<LocationDevice> Devices { get; set; } = new List<LocationDevice>();

        /// <summary>
        /// Country for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Country, ResourceType: typeof(UserAdminResources))]
        public String Country { get; set; }

        /// <summary>
        /// Main Contact for this Location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Admin_Contact, IsRequired: false, FieldType: FieldTypes.UserPicker, WaterMark: UserAdminResources.Names.OrgLocation_AdminContact_Select,
            ResourceType: typeof(UserAdminResources))]
        public EntityHeader AdminContact { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.OrgLocation_DistributionList, IsRequired: false, EntityHeaderPickerUrl: "/api/distros", FieldType: FieldTypes.EntityHeaderPicker, WaterMark: UserAdminResources.Names.OrgLocation_DistributionList_Select,
            ResourceType: typeof(UserAdminResources))]
        public EntityHeader DistributionList { get; set; }

        /// <summary>
        /// The technical contact for this location.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Technical_Contact, IsRequired: false, FieldType: FieldTypes.UserPicker, WaterMark: UserAdminResources.Names.OrgLocation_TechnicalContact_Select,
            ResourceType: typeof(UserAdminResources))]
        public EntityHeader TechnicalContact { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.OrgLocation_Diagram, IsRequired: false, EntityHeaderPickerUrl: "/api/org/location/diagrams", FieldType: FieldTypes.EntityHeaderPicker, WaterMark: UserAdminResources.Names.OrgLocation_Diagram_Select,
            ResourceType: typeof(UserAdminResources))]
        public EntityHeader LocationDiagram { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.OrgLocation_DeviceRepository, IsRequired: false, EntityHeaderPickerUrl: "/api/devicerepos", FieldType: FieldTypes.EntityHeaderPicker, WaterMark: UserAdminResources.Names.OrgLocation_DeviceRepository_Select,
            ResourceType: typeof(UserAdminResources))]
        public EntityHeader DeviceRepository { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.OrgLocation_PrimaryDevice, IsRequired: false, FieldType: FieldTypes.DevicePicker, WaterMark:UserAdminResources.Names.OrgLocation_PrimaryDevice_Select,  ResourceType: typeof(UserAdminResources))]
        public EntityHeader PrimaryDevice { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string Notes { get; set; }

        /// <summary>
        /// A description that can be added to this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public String Description { get; set; }

        public string HeroTitle { get; set; }

        public string HeroImage { get; set; }

        public List<OrgLocationDiagramReference> DiagramReferences { get; set; } = new List<OrgLocationDiagramReference>();


        [FormField(LabelResource: UserAdminResources.Names.Organization_SubLocations, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/org/location/sublocation/factory", ResourceType: typeof(UserAdminResources))]
        public List<SubLocation> SubLocations { get; set; } = new List<SubLocation>();

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
                Category = Category?.Text,
                CategoryKey = Category?.Key,
                CategoryId = Category?.Id,
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
                nameof(PostalCode),
                nameof(Country),
            };
        }

        public List<string> GetAdvancedFieldsCol2()
        {
            return new List<string>()
            {
                nameof(TimeZone),
                nameof(AdminContact),
                nameof(TechnicalContact),
                nameof(GeoLocation),
                nameof(GeoPointsBoundingBox),
                nameof(DeviceRepository),
                nameof(PrimaryDevice),
                nameof(LocationDiagram),
                nameof(DistributionList),
                nameof(Customer),
                nameof(PhoneNumber),
                nameof(Description),
                nameof(Notes),
                nameof(SubLocations),
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

            bldr.Append($"<h1>{Name}</h1>");

            if (!String.IsNullOrEmpty(PhoneNumber))
            {
                bldr.Append("<h5>Primary Phone</h5>");
                bldr.Append($"<a href='tel:{PhoneNumber}'>{PhoneNumber}</a><br /><br />");
            }

            if(!String.IsNullOrEmpty(Addr1) && !String.IsNullOrEmpty(City))
            {
                bldr.Append("<h5>Street Address</h5>");
                var strAddr = String.Empty;                
                if(!String.IsNullOrEmpty(Addr1))
                    strAddr += $"{Addr1},";

                if (!String.IsNullOrEmpty(City))
                    strAddr += $" {City}";

                if (!String.IsNullOrEmpty(StateProvince))
                    strAddr += $" {StateProvince}";

                bldr.Append($"<a href='https://www.google.com/maps/place/{Uri.EscapeDataString(strAddr)}' >");
            }

            if (!String.IsNullOrEmpty(Addr1))
                bldr.Append($"{Addr1}<br/>");
            if (!String.IsNullOrEmpty(Addr2))
                bldr.Append($"{Addr2}<br/>");
            if (!String.IsNullOrEmpty(RoomNumber))
                bldr.Append($"{UserAdminResources.Location_RoomNumber}: {RoomNumber}<br/>");

            if (!String.IsNullOrEmpty(City))
                bldr.Append(City);

            if (!String.IsNullOrEmpty(StateProvince))
            {
                if (!String.IsNullOrEmpty(City))
                    bldr.Append(", ");
                bldr.Append(StateProvince);
            }

            if (!String.IsNullOrEmpty(City) || !String.IsNullOrEmpty(StateProvince))
                bldr.Append("<br />");


            if (!String.IsNullOrEmpty(Addr1) && !String.IsNullOrEmpty(City))
                bldr.Append("</a><br  /><br />");

            if (GeoLocation != null && GeoLocation.Latitude.HasValue && GeoLocation.Longitude.HasValue)
                bldr.Append($"<a href='https://www.google.com/maps/search/?api=1&query={GeoLocation.Latitude},{GeoLocation.Longitude}'>View on Map</a>");

            if (!String.IsNullOrEmpty(Description))
                bldr.Append($"<div>{Description}</div>");

            if (!String.IsNullOrEmpty(Notes))
                bldr.Append($"<div>{Notes}</div>");
            
            return bldr.ToString();
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Locations_Title, UserAdminResources.Names.Organization_Location_Help,
        UserAdminResources.Names.Organization_Location_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources),
        GetListUrl: "/api/org/locations", GetUrl: "/api/org/location/{id}", SaveUrl: "/api/org/location", FactoryUrl: "/api/org/location/factory", DeleteUrl: "/api/org/location/{id}", Icon: "icon-fo-internet-2")]
    public class OrgLocationSummary : SummaryData
    {
        public EntityHeader AdminContact { get; set; }
        public string RoomNumber { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }

        public int NumberDevices { get; set; }
    }

    public class OrgLocationDiagramReference : IValidateable
    {
        public OrgLocationDiagramReference()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }
        public EntityHeader LocationDiagram { get; set; }
        public EntityHeader LocationDiagramLayer { get; set; }
        public EntityHeader LocationDiagramShape { get; set; }


        [CustomValidator]
        public void Validate(ValidationResult result, Actions action)
        {
            if (EntityHeader.IsNullOrEmpty(LocationDiagram)) result.AddUserError("Diagram is required");
            if (EntityHeader.IsNullOrEmpty(LocationDiagramLayer)) result.AddUserError("Layer is required");
            if (EntityHeader.IsNullOrEmpty(LocationDiagramShape)) result.AddUserError("Shape is required");
        }
    }

    public class DeviceReference : IValidateable
    {
        public EntityHeader DeviceRepository { get; set; }
        public EntityHeader Device { get; set; }

        [CustomValidator]
        public void Validate(ValidationResult result, Actions action)
        {
            if (EntityHeader.IsNullOrEmpty(DeviceRepository)) result.AddUserError("Device Repository is required");
            if (EntityHeader.IsNullOrEmpty(Device)) result.AddUserError("Device is required");
        }
    }


}