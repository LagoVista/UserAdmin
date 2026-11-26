using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    /// <summary>
    /// Shared browser session binding primitives. The host must supply a proven
    /// principal, current canonical user and trusted application/origin mapping.
    /// This service does not authenticate a request or issue a token.
    /// </summary>
    public sealed class BrowserInstallationSession
    {
        private readonly BrowserInstallationCookie _cookie;
        private readonly BrowserInstallationManager _installations;

        public BrowserInstallationSession(BrowserInstallationCookie cookie, BrowserInstallationManager installations)
        {
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
            _installations = installations ?? throw new ArgumentNullException(nameof(installations));
        }

        // Only after a successful, full human authentication. An existing
        // cookie session without the installation claim must not use this as
        // a substitute for fresh authentication.
        public async Task<AppInstance> BindProvenSessionAsync(HttpContext context, ClaimsPrincipal principal,
            AppUser user, string trustedAppId, Uri trustedOrigin)
        {
            var identity = RequireHumanPrincipal(principal, user);
            var browserId = _cookie.Read(context, trustedOrigin);
            if (browserId == null)
                throw new InvalidOperationException("Browser installation must be initialized before session binding.");
            var claims = principal.FindAll(ClaimsFactory.AppInstanceId).ToArray();
            if (claims.Length != 0)
                throw new InvalidOperationException("A fresh session must not already carry an installation claim.");
            var instance = await _installations.BindProvenUserAsync(user, trustedAppId, browserId);
            identity.AddClaim(new Claim(ClaimsFactory.AppInstanceId, instance.RowKey));
            return instance;
        }

        // Renewal/sign-out may verify an existing binding but must never
        // create one, silently replace it, or infer it from continuity alone.
        public Task<AppInstance> ReadBoundSessionAsync(HttpContext context, ClaimsPrincipal principal,
            AppUser user, string trustedAppId, Uri trustedOrigin)
        {
            RequireHumanPrincipal(principal, user);
            var browserId = _cookie.Read(context, trustedOrigin);
            var claims = principal.FindAll(ClaimsFactory.AppInstanceId).ToArray();
            if (browserId == null || claims.Length != 1)
                throw new InvalidOperationException("Browser session installation binding required.");
            return _installations.ReadBoundUserAsync(user, trustedAppId, browserId, claims[0].Value);
        }

        private static ClaimsIdentity RequireHumanPrincipal(ClaimsPrincipal principal, AppUser user)
        {
            var identities = principal?.Identities.Where(i => i.IsAuthenticated).ToArray();
            if (user == null || identities == null || identities.Length != 1 ||
                principal.Claims.Any(c => c.Type == ClaimsFactory.Anonymous &&
                    String.Equals(c.Value, Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) ||
                principal.Claims.Any(c => c.Type == ClaimsFactory.IdentityStage &&
                    !String.Equals(c.Value, ClaimsFactory.RegisteredIdentityStage, StringComparison.Ordinal)))
                throw new InvalidOperationException("Authenticated human session required.");

            var users = principal.FindAll(ClaimsFactory.CurrentUserId).ToArray();
            var names = principal.FindAll(ClaimTypes.NameIdentifier).ToArray();
            if (users.Length != 1 || names.Length != 1 ||
                users[0].Value != user.Id.Value || names[0].Value != user.Id.Value ||
                !identities[0].HasClaim(ClaimsFactory.CurrentUserId, user.Id.Value) ||
                !identities[0].HasClaim(ClaimTypes.NameIdentifier, user.Id.Value))
                throw new InvalidOperationException("Authenticated session user mismatch.");
            return identities[0];
        }
    }
}
