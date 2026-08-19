using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Linq;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class OAuthWhoAmIController : Controller
    {
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("/api/oauth/whoami")]
        public IActionResult GetWhoAmI()
        {
            var audiences = User.GetAudiences().ToArray();

            return Ok(new
            {
                Subject = User.GetClaim(Claims.Subject),
                Name = User.GetClaim(Claims.Name),
                ClientId = User.GetClaim(Claims.ClientId),
                Scopes = User.GetScopes().ToArray(),

                // OpenIddict maps the resources selected for an authorization principal
                // to the access token audience (aud) claim. Once the access token has
                // been validated, GetAudiences() is therefore the authoritative view of
                // the resource servers this token was issued for.
                Resources = audiences,
                Audiences = audiences,

                Claims = User.Claims.Select(claim => new
                {
                    claim.Type,
                    claim.Value,
                }).ToArray(),
            });
        }
    }
}
