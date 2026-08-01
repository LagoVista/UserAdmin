using System.Collections.Generic;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public interface IOAuthClientPolicyValidator
    {
        bool IsRedirectUriAllowed(OAuthClientPolicy policy, string redirectUri);
        bool IsGrantTypeAllowed(OAuthClientPolicy policy, string grantType);
        bool AreScopesAllowed(OAuthClientPolicy policy, IEnumerable<string> scopes);
        bool IsResourceAllowed(OAuthClientPolicy policy, string resource);
        bool IsPkceRequired(OAuthClientPolicy policy);
    }
}
