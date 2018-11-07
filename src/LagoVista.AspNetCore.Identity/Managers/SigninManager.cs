using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class SignInManager : ManagerBase, ISignInManager
    {
        IAdminLogger _adminLogger;
        IUserManager _userManager;
        IOrganizationManager _orgManager;
        SignInManager<AppUser> _signinManager;
        public SignInManager(IAdminLogger adminLogger, IDependencyManager depManager, ISecurity security, IAppConfig appConfig, IUserManager userManager, IOrganizationManager orgManager, SignInManager<AppUser> signInManager)
            : base(adminLogger, appConfig, depManager, security)
        {
            _signinManager = signInManager;
            _adminLogger = adminLogger;
            _orgManager = orgManager;
            _userManager = userManager;
        }

        public Task SignInAsync(AppUser user)
        {
            return _signinManager.SignInAsync(user, true);
        }

        public async Task<InvokeResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var signInResult = await _signinManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

            var appUser = await _userManager.FindByEmailAsync(userName);
            if (appUser == null)
            {
                await LogEntityActionAsync(userName, typeof(AppUser).Name, $"Could not find user with account [{userName}].", appUser.CurrentOrganization, appUser.ToEntityHeader());
                return InvokeResult.FromError($"Could not find user [{userName}].");
            }

            if(appUser.IsAccountDisabled)
            {
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed - Account Disabled", appUser.CurrentOrganization, appUser.ToEntityHeader());
                return InvokeResult.FromError($"Account [{userName}] is disabled.");
            }

            if (appUser.CurrentOrganization != null)
            {
                var isOrgAdmin = await _orgManager.IsUserOrgAdminAsync(appUser.CurrentOrganization.Id, appUser.Id);
                if (isOrgAdmin != appUser.IsOrgAdmin)
                {
                    appUser.IsOrgAdmin = isOrgAdmin;
                    await _userManager.UpdateAsync(appUser);
                }
            }

            if (signInResult.Succeeded)
            {
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin", appUser.CurrentOrganization, appUser.ToEntityHeader());
                return InvokeResult.Success;
            }

            if (signInResult.IsLockedOut)
            {

                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed - Locked Out", appUser.CurrentOrganization, appUser.ToEntityHeader());

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", UserAdminErrorCodes.AuthUserLockedOut.Message, new KeyValuePair<string, string>("email", userName));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthUserLockedOut.ToErrorMessage());
            }

            await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed", appUser.CurrentOrganization, appUser.ToEntityHeader());


            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", UserAdminErrorCodes.AuthInvalidCredentials.Message, new KeyValuePair<string, string>("email", userName));
            return InvokeResult.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage());
        }
    }
}
