using OpenIddict.Abstractions;
using OpenIddict.Server;
using System;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    /// <summary>
    /// Supplies client-policy validation for token requests when OpenIddict runs in degraded mode.
    /// Authorization-code integrity, redirect-uri continuity and PKCE verifier validation remain
    /// OpenIddict responsibilities.
    /// </summary>
    public class OAuthTokenRequestValidationHandler : IOpenIddictServerHandler<ValidateTokenRequestContext>
    {
        private readonly IOAuthClientPolicyResolver _policyResolver;
        private readonly IOAuthClientPolicyValidator _policyValidator;

        public OAuthTokenRequestValidationHandler(
            IOAuthClientPolicyResolver policyResolver,
            IOAuthClientPolicyValidator policyValidator)
        {
            _policyResolver = policyResolver ?? throw new ArgumentNullException(nameof(policyResolver));
            _policyValidator = policyValidator ?? throw new ArgumentNullException(nameof(policyValidator));
        }

        public async ValueTask HandleAsync(ValidateTokenRequestContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var request = context.Request;

            if (!request.IsAuthorizationCodeGrantType())
            {
                context.Reject(
                    error: Errors.UnsupportedGrantType,
                    description: "Only the authorization code grant is supported.");
                return;
            }

            var policy = await _policyResolver.GetByClientIdAsync(request.ClientId);
            if (policy == null || !policy.IsActive)
            {
                context.Reject(
                    error: Errors.InvalidClient,
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
        }
    }
}
