using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin
{
    public interface IRingCentralCredentials
    {
        public string RingCentralClientId { get; }
        public string RingCentralClientSecret { get; }
        public string RingCentralJWT { get; }
        public string RingCentralUrl { get; }
    }
}
