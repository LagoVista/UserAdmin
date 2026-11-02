using LagoVista.AspNetCore.Identity.Managers;
using System;
using System.Linq;
using System.Security.Claims;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public sealed class OidcClaimProjection
    {
        public string Subject { get; init; }
        public string Name { get; init; }
        public string GivenName { get; init; }
        public string Surname { get; init; }
        public string PreferredUsername { get; init; }
        public string Email { get; init; }
        public string IsSystemAdmin { get; init; }
        public string CurrentOrgId { get; init; }
        public string CurrentOrgName { get; init; }
        public string CurrentOrgNamespace { get; init; }
        public string IsAppBuilder { get; init; }
        public string EmailVerified { get; init; }

        public static OidcClaimProjection FromPrincipal(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject);

            var email = Normalize(principal.FindFirstValue(ClaimTypes.Email)
                ?? principal.FindFirstValue(OpenIddict.Abstractions.OpenIddictConstants.Claims.Email));

            var preferredUsername = Normalize(principal.FindFirstValue(ClaimsFactory.CurrentUserName))
                ?? Normalize(principal.Identity?.Name)
                ?? email
                ?? subject;

            var givenName = Normalize(principal.FindFirstValue(ClaimTypes.GivenName));
            var surname = Normalize(principal.FindFirstValue(ClaimTypes.Surname));
            var displayName = String.Join(" ", new[] { givenName, surname }.Where(value => !String.IsNullOrWhiteSpace(value)));
            if (String.IsNullOrWhiteSpace(displayName))
                displayName = preferredUsername ?? email ?? subject;

            var isSystemAdmin = Normalize(principal.FindFirstValue(ClaimsFactory.IsSystemAdmin));
            var currentOrgId = Normalize(principal.FindFirstValue(ClaimsFactory.CurrentOrgId));
            var currentOrgName = Normalize(principal.FindFirstValue(ClaimsFactory.CurrentOrgName));
            var currentOrgNamespace = Normalize(principal.FindFirstValue(ClaimsFactory.CurrentOrgNamespace));
            var isAppBuilder = Normalize(principal.FindFirstValue(ClaimsFactory.IsAppBuilder));
            var emailVerified = Normalize(principal.FindFirstValue(ClaimsFactory.EmailVerified));

            return new OidcClaimProjection
            {
                Subject = subject,
                Name = displayName,
                GivenName = givenName,
                Surname = surname,
                PreferredUsername = preferredUsername,
                Email = email,
                IsSystemAdmin = isSystemAdmin,
                CurrentOrgId = currentOrgId,
                CurrentOrgName = currentOrgName,
                CurrentOrgNamespace = currentOrgNamespace,
                IsAppBuilder = isAppBuilder,
                EmailVerified = emailVerified,
            };
        }

        private static string Normalize(string value)
        {
            if (String.IsNullOrWhiteSpace(value) || value == ClaimsFactory.None)
                return null;

            return value;
        }
    }
}
