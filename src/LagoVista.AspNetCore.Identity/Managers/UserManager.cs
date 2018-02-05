using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using LagoVista.Core;
using System;
using LagoVista.Core.Models;
using LagoVista.Core.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Resources;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.AspNetCore.Identity.Managers
{
    /* Let's us bring the user manager code into user admin which is a .NET Standard 1.2 library and can't do the identity stuff. */
    public class UserManager : ManagerBase, IUserManager
    {
        private readonly UserManager<AppUser> _userManager;

        public UserManager(UserManager<AppUser> userManager, IAdminLogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) :
            base(logger, appConfig, dependencyManager, security)
        {
            _userManager = userManager;
        }

        public Task<AppUser> FindByIdAsync(string id)
        {
            return _userManager.FindByIdAsync(id);
        }

        public Task<AppUser> FindByNameAsync(string userName)
        {
            return _userManager.FindByNameAsync(userName);
        }

        public async Task<InvokeResult> CreateAsync(AppUser appUser, string password)
        {
            return (await _userManager.CreateAsync(appUser, password)).ToInvokeResult();
        }

        public async Task<InvokeResult> UpdateAsync(AppUser appUser)
        {
            var org = appUser.CurrentOrganization ?? EntityHeader.Create(Guid.Empty.ToId(), "????");

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Update, org, appUser.ToEntityHeader());

            return (await _userManager.UpdateAsync(appUser)).ToInvokeResult();
        }

        public async Task<InvokeResult> SetPreviewUserStatusAsync(string id, bool previewStatus, EntityHeader org, EntityHeader user)
        {
            var editingUser = await _userManager.FindByIdAsync(id);
            if(!editingUser.IsSystemAdmin)
            {
                return InvokeResult.FromErrors(new ErrorMessage() { Message = "Must be a System Admin to enable/disable preview user status." });
            }

            await LogEntityActionAsync(user.Id, typeof(AppUser).Name, previewStatus ? "SetAsPreviewUser" : "SetAsNonPreviewUser", org, user);

            var loadedUser = await _userManager.FindByIdAsync(id);
            loadedUser.IsPreviewUser = previewStatus;
            await _userManager.UpdateAsync(loadedUser);
            return InvokeResult.Success;
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(AppUser user)
        {
            return _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public Task<string> GenerateChangePhoneNumberTokenAsync(AppUser user, string phone)
        {
            return _userManager.GenerateChangePhoneNumberTokenAsync(user, phone);
        }

        public async Task<InvokeResult> ChangePhoneNumberAsync(AppUser user, string phone, string token)
        {
            var result = await _userManager.ChangePhoneNumberAsync(user, phone, token);
            if(result.Succeeded)
            {
                await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "ConfirmedPhoneNumber", user.CurrentOrganization, user.ToEntityHeader());
            }
            else
            {
                await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "FailedConfirmingPhoneNumber", user.CurrentOrganization, user.ToEntityHeader());
            }

            return result.ToInvokeResult();
        }

        public async Task<InvokeResult> ConfirmEmailAsync(AppUser user, string token)
        {

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "ConfirmedEmail", user.CurrentOrganization, user.ToEntityHeader());
            }
            else
            {
                await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "FailedConfirmingEmail", user.CurrentOrganization, user.ToEntityHeader());
            }

            return result.ToInvokeResult();
        }

        public async Task<InvokeResult> ChangePasswordAsync(AppUser user, string oldPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "ChangedPassword", user.CurrentOrganization, user.ToEntityHeader());
            }
            else
            {
                await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "FailedChangePasswordAttempt", user.CurrentOrganization, user.ToEntityHeader());
            }

            return result.ToInvokeResult();
        }

        public Task<AppUser> FindByEmailAsync(string email)
        {
            return _userManager.FindByEmailAsync(email);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        {
            await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "RequestPassordChange", user.CurrentOrganization, user.ToEntityHeader());
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<InvokeResult> ResetPasswordAsync(AppUser user, string token, string newPassword)
        {
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "PasswordReset", user.CurrentOrganization, user.ToEntityHeader());
            }
            else
            {
                await LogEntityActionAsync(user.Id, typeof(AppUser).Name, "FailedAttemptToResetPassword", user.CurrentOrganization, user.ToEntityHeader());
            }

            return result.ToInvokeResult();
        }

        public async Task<InvokeResult> SetSystemAdminAsync(string userId, EntityHeader org, EntityHeader user)
        {
            var updateByUser = await _userManager.FindByIdAsync(user.Id);
            if(!updateByUser.IsSystemAdmin)
            {
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthNotSysAdmin.ToErrorMessage());
            }

            var updatedUser = await _userManager.FindByIdAsync(userId);
            updatedUser.IsSystemAdmin = true;
            updatedUser.LastUpdatedBy = user;
            updatedUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            await _userManager.UpdateAsync(updateByUser);

            await LogEntityActionAsync(userId, typeof(AppUser).Name, "SetAsSystemAdmin", org, user);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> ClearSystemAdminAsync(string userId, EntityHeader org, EntityHeader user)
        {
            var updateByUser = await _userManager.FindByIdAsync(user.Id);
            if (!updateByUser.IsSystemAdmin)
            {
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthNotSysAdmin.ToErrorMessage());
            }

            var updatedUser = await _userManager.FindByIdAsync(userId);
            updatedUser.IsSystemAdmin = false;
            updatedUser.LastUpdatedBy = user;
            updatedUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            await _userManager.UpdateAsync(updateByUser);

            await LogEntityActionAsync(userId, typeof(AppUser).Name, "ClearSystemAdmin", org, user);

            return InvokeResult.Success;
        }
    }
}
