using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    [AllowAnonymous]
    [Route("oidc")]
    public class OidcController : Controller
    {
        private readonly ISignInManager _signInManager;

        public OidcController(ISignInManager signInManager)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        [HttpGet("login")]
        public IActionResult Login([FromQuery] string returnUrl)
        {
            if (!IsValidOidcReturnUrl(returnUrl))
                return BadRequest("The OIDC return URL is invalid.");

            if (User?.Identity?.IsAuthenticated == true)
                return LocalRedirect(returnUrl);

            return View("Login", new OidcLoginViewModel
            {
                ReturnUrl = returnUrl,
            });
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(OidcLoginViewModel model)
        {
            if (model == null || !IsValidOidcReturnUrl(model.ReturnUrl))
                return BadRequest("The OIDC return URL is invalid.");

            if (!ModelState.IsValid)
                return View("Login", model);

            var result = await _signInManager.PasswordSignInAsync(new AuthLoginRequest
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe,
                LockoutOnFailure = true,
            });

            if (!result.Successful)
            {
                // Keep the OIDC login surface deliberately non-enumerating. The underlying
                // sign-in manager retains the detailed audit result and lockout behavior.
                ModelState.AddModelError(String.Empty, "Unable to sign in with those credentials.");
                model.Password = String.Empty;
                return View("Login", model);
            }

            return LocalRedirect(model.ReturnUrl);
        }

        private bool IsValidOidcReturnUrl(string returnUrl)
        {
            if (String.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return false;

            // This surface exists only to resume an authorization request. Do not let it
            // become a general-purpose post-login redirector.
            return returnUrl.StartsWith(AuthorizationServerConstants.AuthorizationEndpoint, StringComparison.OrdinalIgnoreCase);
        }
    }
}
