using LagoVista.AspNetCore.Identity.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
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

            var response = new Dictionary<string, object>
            {
                [Claims.Subject] = subject,
            };

            if (includeProfile)
            {
                response[Claims.Name] = User.GetClaim(Claims.Name);
                response[Claims.PreferredUsername] = User.GetClaim(Claims.PreferredUsername);
            }

            if (includeEmail)
                response[Claims.Email] = User.GetClaim(Claims.Email);

            var isSystemAdmin = User.GetClaim(ClaimsFactory.IsSystemAdmin);
            if (!String.IsNullOrWhiteSpace(isSystemAdmin))
                response[ClaimsFactory.IsSystemAdmin] = isSystemAdmin;

            return Ok(response);
        }
    }
}
