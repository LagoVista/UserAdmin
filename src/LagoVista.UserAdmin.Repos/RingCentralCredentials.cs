using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos
{
    public class RingCentralCredentials : IRingCentralCredentials
    {
        public string RingCentralClientId { get; }

        public string RingCentralClientSecret { get; }

        public string RingCentralJWT { get; }

        public string RingCentralUrl { get; }

        public RingCentralCredentials(IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection("RingCentral");
            RingCentralClientId = section.Require("ClientId");
            RingCentralClientSecret = section.Require("ClientSecret");
            RingCentralJWT = section.Require("JWT");
            RingCentralUrl = section.Require("URL");
        }
    }
}
