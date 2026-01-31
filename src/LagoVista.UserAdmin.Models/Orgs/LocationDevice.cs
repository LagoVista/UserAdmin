// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 05220ae29a5d6790a7048f1ea134f1cb94a4a427b5f4e0d2b5701eef6382e674
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(
        Domains.OrgLocations,
        UserAdminResources.Names.LocationDevice_Name,
        UserAdminResources.Names.LocationDevice_Help,
        UserAdminResources.Names.LocationDevice_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class LocationDevice : IValidateable
    {
        [FormField(IsRequired: true)]
        public EntityHeader DeviceRepo { get; set; }

        [FormField(IsRequired: true)]
        [FKeyProperty("Device", WhereClause: "Device.Id = {0}")]
        public EntityHeader Device { get; set; }
    }
}
