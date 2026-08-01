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
            return Ok(new
            {
                Subject = User.GetClaim(Claims.Subject),
                Name = User.GetClaim(Claims.Name),
                ClientId = User.GetClaim(Claims.ClientId),
                Scopes = User.GetScopes().ToArray(),
                Resources = User.GetResources().ToArray(),
                Claims = User.Claims.Select(claim => new
                {
                    claim.Type,
                    claim.Value,
                }).ToArray(),
            });
        }
    }
}
