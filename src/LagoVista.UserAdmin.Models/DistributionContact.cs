using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models
{
    public class DistributionContact : EntityHeader
    {
        public string Email { get; set; }
        public string Phone { get; set; }

        public bool SendEmail { get; set; }

        public bool SendText { get; set; }
    }
}
