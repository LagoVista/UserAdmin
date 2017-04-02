using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Geo;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Location_Title, UserAdminResources.Names.Organization_Location_Help, UserAdminResources.Names.Organization_Location_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class OrganizationLocation : UserManagementBase, INamedEntity, IValidateable
    {
        [FormField(LabelResource: Resources.UserAdminResources.Names.Organization, IsRequired:true, ResourceType: typeof(Resources.UserAdminResources))]
        public EntityHeader Organization { get; set; }

        /// <summary>
        /// Name space to be used for any devices at this location.  It will build upon the accounts name space.
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Namespce, NamespaceType: NamespaceTypes.Location, NamespaceUniqueMessageResource: Resources.UserAdminResources.Names.OrganizationLocation_NamespaceInUse, FieldType: FieldTypes.NameSpace, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public String Namespace { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Location_LocationName, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public String Name { get; set; }
       
        /// <summary>
        /// Latitude and longitude for this location
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Location_GeoLocation, FieldType: FieldTypes.GeoLocation, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public IGeoLocation GeoLocation { get; set; }

        /// <summary>
        /// Primary Address
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Location_Address1, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public String Addr1 { get; set; }

        /// <summary>
        /// Secondary Address
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Location_Address2, ResourceType: typeof(Resources.UserAdminResources))]
        public String Addr2 { get; set; }
        /// <summary>
        /// City for this location
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Location_City, ResourceType: typeof(Resources.UserAdminResources))]
        public String City { get; set; }
        /// <summary>
        /// State or province for this location
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Location_State, ResourceType: typeof(Resources.UserAdminResources))]
        public String StateProvince { get; set; }
        /// <summary>
        /// Postal code for this location
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Location_PostalCode,  ResourceType: typeof(Resources.UserAdminResources))]
        public String PostalCode { get; set; }

        /// <summary>
        /// Country for this location
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Location_Country, ResourceType: typeof(Resources.UserAdminResources))]
        public String Country { get; set; }

        /// <summary>
        /// Main Contact for this Location
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Admin_Contact, IsRequired: true,  ResourceType: typeof(Resources.UserAdminResources))]
        public EntityHeader AdminContact { get; set; }

        /// <summary>
        /// The technical contact for this location.
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Technical_Contact, IsRequired: true,  ResourceType: typeof(Resources.UserAdminResources))]
        public EntityHeader TechnicalContact { get; set; }


        /// <summary>
        /// A description that can be added to this location
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Description, FieldType:FieldTypes.MultiLineText, ResourceType: typeof(Resources.UserAdminResources))]
        public String Description { get; set; }
        /// <summary>
        /// Notes that can be added to this location
        /// </summary>
        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Notes, FieldType:FieldTypes.MultiLineText, ResourceType: typeof(Resources.UserAdminResources))]
        public String Notes { get; set; }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, Name);
         
        }

        public override string ToString()
        {
            return Namespace;
        }
    }
}
