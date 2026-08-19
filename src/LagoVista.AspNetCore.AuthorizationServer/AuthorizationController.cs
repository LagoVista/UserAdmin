using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class AuthorizationController : Controller
    {
        private readonly IOAuthClientPolicyResolver _policyResolver;
        private readonly IOAuthClientPolicyValidator _policyValidator;

        public AuthorizationController(IOAuthClientPolicyResolver policyResolver, IOAuthClientPolicyValidator policyValidator)
        {
            _policyResolver = policyResolver ?? throw new ArgumentNullException(nameof(policyResolver));
            _policyValidator = policyValidator ?? throw new ArgumentNullException(nameof(policyValidator));
        }

        [HttpGet(AuthorizationServerConstants.AuthorizationEndpoint)]
        [HttpPost(AuthorizationServerConstants.AuthorizationEndpoint)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AuthorizeAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
                throw new InvalidOperationException("The OAuth authorization request could not be resolved.");

            var policy = await _policyResolver.GetByClientIdAsync(request.ClientId);
            if (policy == null || !policy.IsActive)
                return Reject(Errors.UnauthorizedClient, "The OAuth client is unknown or disabled.");

            if (!String.Equals(request.ResponseType, ResponseTypes.Code, StringComparison.Ordinal))
                return Reject(Errors.UnsupportedResponseType, "Only the authorization code response type is supported.");

            if (!_policyValidator.IsGrantTypeAllowed(policy, GrantTypes.AuthorizationCode))
                return Reject(Errors.UnauthorizedClient, "The OAuth client is not permitted to use the authorization code flow.");

            if (!_policyValidator.IsRedirectUriAllowed(policy, request.RedirectUri))
                return Reject(Errors.InvalidRequest, "The redirect URI is not registered for this OAuth client.");

            var scopes = request.GetScopes();
            if (!_policyValidator.AreScopesAllowed(policy, scopes))
                return Reject(Errors.InvalidScope, "One or more requested scopes are not permitted for this OAuth client.");

            var resources = request.GetResources();
            if (resources.Length > 1)
                return Reject(Errors.InvalidTarget, "At most one registered resource may be requested.");

            if (resources.Length == 1 && !_policyValidator.IsResourceAllowed(policy, resources[0]))
                return Reject(Errors.InvalidTarget, "The requested resource is not registered for this OAuth client.");

            if (_policyValidator.IsPkceRequired(policy))
            {
                if (String.IsNullOrWhiteSpace(request.CodeChallenge))
                    return Reject(Errors.InvalidRequest, "A PKCE code challenge is required.");

                if (!String.Equals(request.CodeChallengeMethod, CodeChallengeMethods.Sha256, StringComparison.Ordinal))
                    return Reject(Errors.InvalidRequest, "The PKCE code challenge method must be S256.");
            }

            var authentication = await HttpContext.AuthenticateAsync();
            if (authentication?.Principal?.Identity?.IsAuthenticated != true)
            {
                var returnUrl = Request.GetEncodedPathAndQuery();
                return RedirectToAction(
                    nameof(OidcController.Login),
                    "Oidc",
                    new { returnUrl });
            }

            var subject = authentication.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? authentication.Principal.FindFirstValue(Claims.Subject);

            if (String.IsNullOrWhiteSpace(subject))
                return Reject(Errors.AccessDenied, "The authenticated user does not have a stable subject identifier.");

            var email = authentication.Principal.FindFirstValue(ClaimTypes.Email)
                ?? authentication.Principal.FindFirstValue(Claims.Email);

            var displayName = authentication.Principal.Identity.Name
                ?? email
                ?? subject;

            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            identity.AddClaim(new Claim(Claims.Subject, subject));

            var nameClaim = new Claim(Claims.Name, displayName);
            if (scopes.Contains(Scopes.Profile, StringComparer.Ordinal))
                nameClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
            else
                nameClaim.SetDestinations(Destinations.AccessToken);
            identity.AddClaim(nameClaim);

            if (!String.IsNullOrWhiteSpace(email) && scopes.Contains(Scopes.Email, StringComparer.Ordinal))
            {
                identity.AddClaim(new Claim(Claims.Email, email)
                    .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
            }

            identity.AddClaim(new Claim(Claims.ClientId, policy.ClientId).SetDestinations(Destinations.AccessToken));

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(scopes);
            if (resources.Length == 1)
                principal.SetResources(resources);

            if (policy.AccessTokenLifetimeMinutes.HasValue && policy.AccessTokenLifetimeMinutes.Value > 0)
                principal.SetAccessTokenLifetime(TimeSpan.FromMinutes(policy.AccessTokenLifetimeMinutes.Value));

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private IActionResult Reject(string error, string description)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
            });

            return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
