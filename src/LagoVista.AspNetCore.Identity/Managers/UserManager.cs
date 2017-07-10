using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using LagoVista.Core;
using System;

namespace LagoVista.AspNetCore.Identity.Managers
{
    /* Let's us bring the user manager code into user admin which is a .NET Standard 1.2 library and can't do the identity stuff. */
    public class UserManager : IUserManager
    {
        private readonly UserManager<AppUser> _userManager;

        public UserManager(UserManager<AppUser> userManager)
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

        public async Task<InvokeResult> CreateAsync(AppUser user, string password)
        {
            return (await _userManager.CreateAsync(user, password)).ToInvokeResult();
        }

        public async Task<InvokeResult> UpdateAsync(AppUser appUser)
        {
            return (await _userManager.UpdateAsync(appUser)).ToInvokeResult();
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
            return (await _userManager.ChangePhoneNumberAsync(user, phone, token)).ToInvokeResult();
        }

        public async Task<InvokeResult> ConfirmEmailAsync(AppUser user, string token)
        {
            return (await _userManager.ConfirmEmailAsync(user, token)).ToInvokeResult();
        }

        public async Task<InvokeResult> ChangePasswordAsync(AppUser user, string oldPassword, string newPassword)
        {
            return (await _userManager.ChangePasswordAsync(user, oldPassword, newPassword)).ToInvokeResult();
        }

        public Task<AppUser> FindByEmailAsync(string email)
        {
            return _userManager.FindByEmailAsync(email);
        }

        public Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        {
            return _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<InvokeResult> ResetPasswordAsync(AppUser user, string token, string newPassword)
        {
            return (await _userManager.ResetPasswordAsync(user, token, newPassword)).ToInvokeResult();
        }
    }
}
