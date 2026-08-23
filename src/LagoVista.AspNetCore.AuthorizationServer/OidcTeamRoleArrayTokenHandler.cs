using Microsoft.IdentityModel.JsonWebTokens;
using OpenIddict.Server;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    /// <summary>
    /// Restores the DOKS team_role claim to an explicit JSON array immediately before
    /// the final identity token is serialized. A one-element array carried through the
    /// authorization code is reconstructed by IdentityModel as a scalar string claim.
    /// </summary>
    public sealed class OidcTeamRoleArrayTokenHandler : IOpenIddictServerHandler<GenerateTokenContext>
    {
        public ValueTask HandleAsync(GenerateTokenContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!String.Equals(context.TokenType, TokenTypeIdentifiers.IdentityToken, StringComparison.Ordinal))
                return ValueTask.CompletedTask;

            RestoreTeamRoleArray(context.Principal);
            return ValueTask.CompletedTask;
        }

        public static void RestoreTeamRoleArray(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var roles = principal.FindAll(AuthorizationServerConstants.ClaimTeamRole)
                .Select(claim => claim.Value)
                .Where(value => !String.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (roles.Length == 0)
                return;

            foreach (var identity in principal.Identities)
            {
                foreach (var claim in identity.FindAll(AuthorizationServerConstants.ClaimTeamRole).ToArray())
                    identity.RemoveClaim(claim);
            }

            var targetIdentity = principal.Identities.FirstOrDefault();
            if (targetIdentity == null)
                return;

            targetIdentity.AddClaim(new Claim(
                AuthorizationServerConstants.ClaimTeamRole,
                JsonSerializer.Serialize(roles),
                JsonClaimValueTypes.JsonArray));
        }
    }
}
