using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin
{
    public interface IRingCentralCredentials
    {
        public string RingCentralClientId { get; }
        public string RingCentralClientSecret { get; set; }
        public string RingCentralJWT { get; set; }
        public string RingCentralUrl { get; set; }
    }
}
