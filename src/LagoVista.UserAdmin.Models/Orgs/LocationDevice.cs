using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public class LocationDevice
    {
        [FKeyProperty("Device", "Device.Id = {0}")]
        public EntityHeader Device { get; set; }
    }
}
