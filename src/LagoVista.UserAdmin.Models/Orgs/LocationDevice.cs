// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 05220ae29a5d6790a7048f1ea134f1cb94a4a427b5f4e0d2b5701eef6382e674
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;


namespace LagoVista.UserAdmin.Models.Orgs
{
    public class LocationDevice : IValidateable
    {
        [FormField(IsRequired:true)]
        public EntityHeader DeviceRepo { get; set; }

        [FormField(IsRequired: true)]
        [FKeyProperty("Device", WhereClause: "Device.Id = {0}")]
        public EntityHeader Device { get; set; }
    }
}
