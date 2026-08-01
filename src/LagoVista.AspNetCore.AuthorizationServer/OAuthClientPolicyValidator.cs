using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class OAuthClientPolicyValidator : IOAuthClientPolicyValidator
    {
        public bool IsRedirectUriAllowed(OAuthClientPolicy policy, string redirectUri)
        {
            return IsUsable(policy) && !String.IsNullOrWhiteSpace(redirectUri) &&
                policy.RedirectUris.Contains(redirectUri, StringComparer.Ordinal);
        }

        public bool IsGrantTypeAllowed(OAuthClientPolicy policy, string grantType)
        {
            return IsUsable(policy) && !String.IsNullOrWhiteSpace(grantType) &&
                policy.AllowedGrantTypes.Contains(grantType, StringComparer.Ordinal);
        }

        public bool AreScopesAllowed(OAuthClientPolicy policy, IEnumerable<string> scopes)
        {
            if (!IsUsable(policy)) return false;

            var requestedScopes = scopes?
                .Where(scope => !String.IsNullOrWhiteSpace(scope))
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();

            return requestedScopes.All(scope => policy.AllowedScopes.Contains(scope, StringComparer.Ordinal));
        }

        public bool IsResourceAllowed(OAuthClientPolicy policy, string resource)
        {
            return IsUsable(policy) && !String.IsNullOrWhiteSpace(resource) &&
                policy.AllowedResources.Contains(resource, StringComparer.Ordinal);
        }

        public bool IsPkceRequired(OAuthClientPolicy policy)
        {
            return IsUsable(policy) && (policy.IsPublicClient || policy.RequirePkce);
        }

        private static bool IsUsable(OAuthClientPolicy policy)
        {
            return policy != null && policy.IsActive;
        }
    }
}
