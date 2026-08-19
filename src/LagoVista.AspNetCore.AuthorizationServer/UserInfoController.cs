using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Linq;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class UserInfoController : Controller
    {
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet(AuthorizationServerConstants.UserInfoEndpoint)]
        [HttpPost(AuthorizationServerConstants.UserInfoEndpoint)]
        public IActionResult GetUserInfo()
        {
            var subject = User.GetClaim(Claims.Subject);
            if (String.IsNullOrWhiteSpace(subject))
                return Unauthorized();

            var scopes = User.GetScopes().ToArray();
            var includeProfile = scopes.Contains(Scopes.Profile, StringComparer.Ordinal);
            var includeEmail = scopes.Contains(Scopes.Email, StringComparer.Ordinal);

            return Ok(new
            {
                sub = subject,
                name = includeProfile ? User.GetClaim(Claims.Name) : null,
                preferred_username = includeProfile ? User.GetClaim(Claims.Name) : null,
                email = includeEmail ? User.GetClaim(Claims.Email) : null,
            });
        }
    }
}
