using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos
{
    public class OAuthSettings : IOAuthSettings
    {
        public OAuthConfig FaceBookOAuth { get; }

        public OAuthConfig TwitterOAuth { get; }

        public OAuthConfig GitHubOAuth { get; }

        public OAuthConfig LinkedInOAuth { get; }

        public OAuthConfig AmazonOAuth { get; }

        public OAuthConfig MicrosoftOAuth { get; }

        public OAuthConfig GoogleOAuth { get; }

        public OAuthSettings(IConfiguration configuration)
        {
            var section = configuration.GetSection("OAuth");
            foreach (var child in new[] { "GitHub", "Google", "LinkedIn", "Microsoft", "Twitter", "FaceBook" /*, "Amazon" */})
            {
                var childSection = section.GetSection(child);
                var clientId = childSection.Require("ClientId");
                var secret = childSection.Require("Secret");
                var returnUrl = childSection.Require("ReturnUrl");

                switch(child)
                {
                    case "GitHub":
                        GitHubOAuth = new OAuthConfig(clientId, secret, returnUrl);
                        break;
                    case "Google":
                        GoogleOAuth = new OAuthConfig(clientId, secret, returnUrl);
                        break;
                    case "LinkedIn":
                        LinkedInOAuth = new OAuthConfig(clientId, secret, returnUrl);
                        break;
                    case "Microsoft":
                        var secretId = childSection.Require("SecretId");

                        MicrosoftOAuth = new OAuthConfig(clientId, secretId, secret, returnUrl);

                        //settings.Settings = new System.Collections.Generic.Dictionary<string, string>
                        //{
                        //    { "Microsoft__SecretId", secretId },
                        //};
                        break;
                    case "Twitter":
                        TwitterOAuth = new OAuthConfig(clientId, secret, returnUrl);
                        break;
                    case "FaceBook":
                        FaceBookOAuth = new OAuthConfig(clientId, secret, returnUrl);
                        break;
                        //case "Amazon":
                        //    AmazonOAuth = new OAuthConfig(clientId, secret);
                        //    break;
                }
            }
        }
    }
}
