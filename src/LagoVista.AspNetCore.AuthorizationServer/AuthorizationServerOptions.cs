using System;
using System.Collections.Generic;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class AuthorizationServerOptions
    {
        public Uri Issuer { get; set; }
        public bool UseDevelopmentCertificates { get; set; } = true;
        public bool DisableAccessTokenEncryption { get; set; } = true;

        /// <summary>
        /// Secret id used to persist the shared OpenIddict signing/encryption certificate
        /// in ISecureStorage when development certificates are disabled.
        /// </summary>
        public string SharedCertificateSecretId { get; set; } = "openiddict-server-certificate:v1";

        public List<string> Scopes { get; set; } = new List<string>();
    }
}
