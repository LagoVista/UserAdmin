using LagoVista.Core.Attributes;
using LagoVista.Core.Geo;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using System;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationViewModels, UserAdminResources.Names.LocationVM_Title, UserAdminResources.Names.LocationVM_Help, UserAdminResources.Names.LocationUser_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class LocationViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.Location_LocationName, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String LocationName { get; set; }

        /// <summary>
        /// Latitude and longitude for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_GeoLocation, FieldType: FieldTypes.GeoLocation, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public IGeoLocation GeoLocation { get; set; }

        /// <summary>
        /// Primary Address
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_Address1, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String Addr1 { get; set; }

        /// <summary>
        /// Secondary Address
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_Address2, ResourceType: typeof(UserAdminResources))]
        public String Addr2 { get; set; }
        /// <summary>
        /// City for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_City, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String City { get; set; }
        /// <summary>
        /// State or province for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_State, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String StateProvince { get; set; }
        /// <summary>
        /// Postal code for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_PostalCode, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String PostalCode { get; set; }
        /// <summary>
        /// Country for this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Location_Country, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String Country { get; set; }

        /// <summary>
        /// A description that can be added to this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public String Description { get; set; }
        /// <summary>
        /// Notes that can be added to this location
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public String Notes { get; set; }

        public virtual void MapToOrganizationLocation(OrgLocation location)
        {
            location.Name = LocationName;
            location.Addr1 = Addr1;
            location.Addr2 = Addr2;
            location.City = City;
            location.StateProvince = StateProvince;
            location.PostalCode = PostalCode;
            location.Country = Country;
            location.Notes = Notes;
            location.Description = Description;
        }
    }
}
