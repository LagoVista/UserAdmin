// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 86a10cd4cdf5502045c0bc43bb61cd26288e1985820dc9e9ed2236177eef08c2
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    /* Let's us bring the user manager code into user admin which is a .NET Standard 1.2 library and can't do the identity stuff. */
    public interface IUserManager
    {
        Task<InvokeResult> ChangePasswordAsync(AppUser user, string oldPassword, string newPassword);
        Task<AppUser> FindByEmailAsync(string email);
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByIdAsync(string id);
        Task<InvokeResult> UpdateAsync(AppUser appUser);
        Task<InvokeResult> CreateAsync(AppUser user, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<string> GenerateChangePhoneNumberTokenAsync(AppUser user, string phone);
        Task<InvokeResult> ChangePhoneNumberAsync(AppUser user, string phone, string token);
        Task<InvokeResult> ConfirmEmailAsync(AppUser user, string token);
        Task<InvokeResult> ResetPasswordAsync(AppUser user, string token, string newPassword);
        Task<InvokeResult> SetPreviewUserStatusAsync(string id, bool previewStatus, EntityHeader org, EntityHeader user);
        Task<InvokeResult> SetSystemAdminAsync(String userId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> ClearSystemAdminAsync(String userId, EntityHeader org, EntityHeader user);
    }
}
