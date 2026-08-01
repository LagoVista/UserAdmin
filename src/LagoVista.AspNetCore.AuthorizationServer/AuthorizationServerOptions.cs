using System;
using System.Collections.Generic;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class AuthorizationServerOptions
    {
        public Uri Issuer { get; set; }
        public bool UseDevelopmentCertificates { get; set; }
        public bool DisableAccessTokenEncryption { get; set; } = true;
        public List<string> Scopes { get; set; } = new List<string>();
    }
}
