using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class SignInManager : ISignInManager
    {
        IAdminLogger _adminLogger;
        public SignInManager(IAdminLogger adminLogger)
        {
            _adminLogger = adminLogger;
        }

        SignInManager<AppUser> _signinManager;
        public SignInManager(SignInManager<AppUser> signInManager)
        {
            _signinManager = signInManager;
        }

        public Task SignInAsync(AppUser user)
        {
            return _signinManager.SignInAsync(user, true);
        }

        public async Task<InvokeResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var signInResult = await _signinManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

            if (signInResult.Succeeded)
            {
                return InvokeResult.Success;
            }

            if (signInResult.IsLockedOut)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", UserAdminErrorCodes.AuthUserLockedOut.Message, new KeyValuePair<string, string>("email", userName));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthUserLockedOut.ToErrorMessage());
            }

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", UserAdminErrorCodes.AuthInvalidCredentials.Message, new KeyValuePair<string, string>("email", userName));
            return InvokeResult.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage());
        }
    }
}
