using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    /// <summary>
    /// Scoped coordinator for the existing application-cookie signing event.
    /// A host opts in with a verified application/origin resolver; no new auth scheme.
    /// A fresh proof marker is set only around an existing trusted sign-in operation.
    /// </summary>
    public sealed class BrowserInstallationSignIn
    {
        private readonly BrowserInstallationSession _session;
        private readonly Func<HttpContext, Task<(string AppId, Uri Origin)>> _resolveApplication;
        private AppUser _provenUser;
        private ClaimsPrincipal _issuedPrincipal;
        private bool _signedOut;

        public BrowserInstallationSignIn(BrowserInstallationSession session,
            Func<HttpContext, Task<(string AppId, Uri Origin)>> resolveApplication)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _resolveApplication = resolveApplication ?? throw new ArgumentNullException(nameof(resolveApplication));
        }

        public IDisposable BeginProvenSignIn(AppUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var previous = _provenUser;
            _provenUser = user;
            return new ProofScope(() => _provenUser = previous);
        }

        public void ClearIssuedSession()
        {
            _issuedPrincipal = null;
            _signedOut = true;
        }

        public async Task ApplyAsync(CookieSigningInContext context, Func<string, Task<AppUser>> loadUser)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (loadUser == null) throw new ArgumentNullException(nameof(loadUser));
            if (context.Scheme.Name != IdentityConstants.ApplicationScheme) return;

            var principal = context.Principal;
            // No caller can provide a trusted claim through the sign-in payload.
            var supplied = principal?.FindAll(ClaimsFactory.AppInstanceId).ToArray();
            if (supplied?.Length > 0)
                foreach (var identity in principal.Identities)
                    foreach (var claim in identity.FindAll(ClaimsFactory.AppInstanceId).ToArray())
                        identity.RemoveClaim(claim);

            var userClaims = principal?.FindAll(ClaimsFactory.CurrentUserId).ToArray();
            if (userClaims == null || userClaims.Length != 1 ||
                principal.HasClaim(ClaimsFactory.Anonymous, Boolean.TrueString) ||
                principal.Claims.Any(c => c.Type == ClaimsFactory.IdentityStage &&
                    c.Value != ClaimsFactory.RegisteredIdentityStage)) return;

            try
            {
                var binding = await _resolveApplication(context.HttpContext);
                if (String.IsNullOrWhiteSpace(binding.AppId) || binding.Origin == null) return;
                var user = await loadUser(userClaims[0].Value);
                if (user == null) return;

                if (_provenUser != null && _provenUser.Id == user.Id)
                {
                    await _session.BindProvenSessionAsync(context.HttpContext, principal, user,
                        binding.AppId, binding.Origin);
                }
                else
                {
                    // Only a previously validated application cookie, or the
                    // bound cookie just issued in this request, can be renewed.
                    var previous = _issuedPrincipal;
                    if (previous == null)
                    {
                        if (_signedOut) return;
                        var authenticated = await context.HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
                        if (!authenticated.Succeeded) return;
                        previous = authenticated.Principal;
                    }
                    var instance = await _session.ReadBoundSessionAsync(context.HttpContext, previous, user,
                        binding.AppId, binding.Origin);
                    var identity = principal.Identities.Single(i => i.IsAuthenticated);
                    if (!identity.HasClaim(ClaimsFactory.CurrentUserId, user.Id.Value) ||
                        principal.FindAll(ClaimTypes.NameIdentifier).Any(c => c.Value != user.Id.Value) ||
                        !identity.HasClaim(ClaimTypes.NameIdentifier, user.Id.Value)) return;
                    identity.AddClaim(new Claim(ClaimsFactory.AppInstanceId, instance.RowKey));
                }
                _issuedPrincipal = principal.Clone();
                _signedOut = false;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception)
            {
                // Installation-aware features remain unavailable; ordinary
                // authentication is not dependent on installation storage.
                // Do not expose persistence/cookie details or mint an unverified claim.
                foreach (var identity in principal.Identities)
                    foreach (var claim in identity.FindAll(ClaimsFactory.AppInstanceId).ToArray())
                        identity.RemoveClaim(claim);
                _issuedPrincipal = null;
            }
        }

        private sealed class ProofScope : IDisposable
        {
            private Action _end;
            public ProofScope(Action end) => _end = end;
            public void Dispose() { var end = _end; _end = null; end?.Invoke(); }
        }
    }
}
