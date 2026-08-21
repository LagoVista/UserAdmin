// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cc445af142eac307e4155367d2d3dac56ee6258eb0100bfb1201b82fd1289d57
// IndexVersion: 2
// --- END CODE INDEX META ---
namespace LagoVista.UserAdmin
{
    public interface IOAuthSettings
    {
        OAuthConfig FaceBookOAuth { get; }
        OAuthConfig TwitterOAuth { get; }
        OAuthConfig GitHubOAuth { get; }
        OAuthConfig LinkedInOAuth { get;  }
        OAuthConfig AmazonOAuth { get; }
        OAuthConfig MicrosoftOAuth { get; }
        OAuthConfig GoogleOAuth { get;  }
    }

    public class OAuthConfig
    {
        public OAuthConfig(string clientId, string secret, string returnUrl)
        {
            ClientId = clientId;
            Secret = secret;
            ReturnUrl = returnUrl;
        }

        public OAuthConfig(string clientId, string secretId, string secret, string returnUrl)
        {
            ClientId = clientId;
            ClientSecretId = secretId;
            Secret = secret;
            ReturnUrl = returnUrl;
        }

        public string ClientId { get;  }
        public string ClientSecretId { get; }
        public string Secret { get; }   

        public string ReturnUrl {get; }
    }
}
