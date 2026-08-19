using OpenIddict.Abstractions;
using OpenIddict.Server;
using System;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    /// <summary>
    /// Supplies the client-application validation OpenIddict normally performs through
    /// its application manager when the server is running in degraded mode.
    /// </summary>
    public class OAuthAuthorizationRequestValidationHandler : IOpenIddictServerHandler<ValidateAuthorizationRequestContext>
    {
        private readonly IOAuthClientPolicyResolver _policyResolver;
        private readonly IOAuthClientPolicyValidator _policyValidator;

        public OAuthAuthorizationRequestValidationHandler(
            IOAuthClientPolicyResolver policyResolver,
            IOAuthClientPolicyValidator policyValidator)
        {
            _policyResolver = policyResolver ?? throw new ArgumentNullException(nameof(policyResolver));
            _policyValidator = policyValidator ?? throw new ArgumentNullException(nameof(policyValidator));
        }

        public async ValueTask HandleAsync(ValidateAuthorizationRequestContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var request = context.Request;
            var policy = await _policyResolver.GetByClientIdAsync(request.ClientId);

            if (policy == null || !policy.IsActive)
            {
                context.Reject(
                    error: Errors.UnauthorizedClient,
                    description: "The OAuth client is unknown or disabled.");
                return;
            }

            if (!_policyValidator.IsGrantTypeAllowed(policy, GrantTypes.AuthorizationCode))
            {
                context.Reject(
                    error: Errors.UnauthorizedClient,
                    description: "The OAuth client is not permitted to use the authorization code flow.");
                return;
            }

            if (!_policyValidator.IsRedirectUriAllowed(policy, request.RedirectUri))
            {
                context.Reject(
                    error: Errors.InvalidRequest,
                    description: "The redirect URI is not registered for this OAuth client.");
                return;
            }

            if (!_policyValidator.AreScopesAllowed(policy, request.GetScopes()))
            {
                context.Reject(
                    error: Errors.InvalidScope,
                    description: "One or more requested scopes are not permitted for this OAuth client.");
                return;
            }

            var resources = request.GetResources();
            if (resources.Length > 1)
            {
                context.Reject(
                    error: Errors.InvalidTarget,
                    description: "At most one registered resource may be requested.");
                return;
            }

            if (resources.Length == 1 && !_policyValidator.IsResourceAllowed(policy, resources[0]))
            {
                context.Reject(
                    error: Errors.InvalidTarget,
                    description: "The requested resource is not registered for this OAuth client.");
                return;
            }

            if (_policyValidator.IsPkceRequired(policy))
            {
                if (String.IsNullOrWhiteSpace(request.CodeChallenge))
                {
                    context.Reject(
                        error: Errors.InvalidRequest,
                        description: "A PKCE code challenge is required.");
                    return;
                }

                if (!String.Equals(request.CodeChallengeMethod, CodeChallengeMethods.Sha256, StringComparison.Ordinal))
                {
                    context.Reject(
                        error: Errors.InvalidRequest,
                        description: "The PKCE code challenge method must be S256.");
                    return;
                }
            }

            // In degraded mode OpenIddict cannot resolve the registered redirect URI itself.
            // Mark the exact URI we validated as trusted so it can be used for the response.
            context.SetRedirectUri(request.RedirectUri);
        }
    }
}
