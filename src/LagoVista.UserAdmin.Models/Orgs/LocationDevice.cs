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
