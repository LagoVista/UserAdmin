using LagoVista.AspNetCore.Identity.Managers;
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
using System.Text.Json;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class AuthorizationController : Controller
    {
        private const string BuildVersionClaim = "com.lagovista.buildversion";

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
                return Redirect($"/oidc/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var projectedClaims = OidcClaimProjection.FromPrincipal(authentication.Principal);
            if (String.IsNullOrWhiteSpace(projectedClaims.Subject))
                return Reject(Errors.AccessDenied, "The authenticated user does not have a stable subject identifier.");

            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            identity.AddClaim(new Claim(Claims.Subject, projectedClaims.Subject));

            if (!String.IsNullOrWhiteSpace(projectedClaims.Name))
            {
                var nameClaim = new Claim(Claims.Name, projectedClaims.Name);
                if (scopes.Contains(Scopes.Profile, StringComparer.Ordinal))
                    nameClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
                else
                    nameClaim.SetDestinations(Destinations.AccessToken);
                identity.AddClaim(nameClaim);
            }

            if (!String.IsNullOrWhiteSpace(projectedClaims.PreferredUsername))
            {
                var preferredUsernameClaim = new Claim(Claims.PreferredUsername, projectedClaims.PreferredUsername);
                if (scopes.Contains(Scopes.Profile, StringComparer.Ordinal))
                    preferredUsernameClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
                else
                    preferredUsernameClaim.SetDestinations(Destinations.AccessToken);
                identity.AddClaim(preferredUsernameClaim);
            }

            if (!String.IsNullOrWhiteSpace(projectedClaims.Email) && scopes.Contains(Scopes.Email, StringComparer.Ordinal))
            {
                identity.AddClaim(new Claim(Claims.Email, projectedClaims.Email)
                    .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
            }

            if (!String.IsNullOrWhiteSpace(projectedClaims.IsSystemAdmin))
            {
                identity.AddClaim(new Claim(ClaimsFactory.IsSystemAdmin, projectedClaims.IsSystemAdmin)
                    .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
            }

            var teamRole = OidcTeamRoleProjection.GetTeamRole(scopes, projectedClaims.IsSystemAdmin);
            if (!String.IsNullOrWhiteSpace(teamRole))
            {
                identity.AddClaim(new Claim(AuthorizationServerConstants.ClaimTeamRole, teamRole)
                    .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
            }

            // TEMPORARY DIAGNOSTIC: proves which published host build issued the ID token.
            identity.AddClaim(new Claim(BuildVersionClaim, GetBuildVersion())
                .SetDestinations(Destinations.IdentityToken));

            identity.AddClaim(new Claim(Claims.ClientId, policy.ClientId).SetDestinations(Destinations.AccessToken));

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(scopes);
            if (resources.Length == 1)
                principal.SetResources(resources);

            if (policy.AccessTokenLifetimeMinutes.HasValue && policy.AccessTokenLifetimeMinutes.Value > 0)
                principal.SetAccessTokenLifetime(TimeSpan.FromMinutes(policy.AccessTokenLifetimeMinutes.Value));

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet(AuthorizationServerConstants.EndSessionEndpoint)]
        [HttpPost(AuthorizationServerConstants.EndSessionEndpoint)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LogoutAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
                throw new InvalidOperationException("The OIDC end session request could not be resolved.");

            // This is a protocol endpoint, so ASP.NET antiforgery validation is intentionally not used.
            // Requiring an OpenID Connect id_token_hint prevents a bare cross-site request from silently
            // terminating the local NuvIoT browser session. Registered post-logout redirects are still
            // validated through the UserAdmin-backed OpenIddict application store.
            if (String.IsNullOrWhiteSpace(request.IdTokenHint))
                return Reject(Errors.InvalidRequest, "An id_token_hint is required to terminate the NuvIoT authentication session.");

            await HttpContext.SignOutAsync();

            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static string GetBuildVersion()
        {
            const string versionFile = "version.json";
            if (!System.IO.File.Exists(versionFile))
                return "????";

            using var document = JsonDocument.Parse(System.IO.File.ReadAllText(versionFile));
            if (document.RootElement.TryGetProperty("Version", out var version))
                return version.GetString() ?? "????";

            return "????";
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
