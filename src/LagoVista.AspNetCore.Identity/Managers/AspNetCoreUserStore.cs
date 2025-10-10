
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Interfaces.Repos.Users;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AspNetCoreUserStore : IUserStore<AppUser>,
            IUserPasswordStore<AppUser>,
            IUserEmailStore<AppUser>,
            IUserPhoneNumberStore<AppUser>,
            IUserTwoFactorStore<AppUser>,
            IUserLoginStore<AppUser>,
            IUserLockoutStore<AppUser>,
            IUserSecurityStampStore<AppUser>

    {
        IAppUserRepo _userRepo;

        public AspNetCoreUserStore(IAppUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken token)
        {
            await _userRepo.CreateAsync(user);
            return IdentityResult.Success;
        }


        public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken token)
        {
            await _userRepo.UpdateAsync(user);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken token)
        {
            await _userRepo.DeleteAsync(user);
            return IdentityResult.Success;
        }

        public Task<AppUser> FindByIdAsync(string userId, CancellationToken token)
        {
            return _userRepo.FindByIdAsync(userId);
        }

        public Task<AppUser> FindByNameAsync(string userName, CancellationToken token)
        {
            return _userRepo.FindByNameAsync(userName);
        }

        public Task<String> GetUserIdAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.Id);
        }

        public Task<String> GetUserNameAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<String> GetNormalizedUserNameAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<String> GetNormalizedEmailAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.Email);
        }

        public Task SetNormalizedEmailAsync(AppUser user,  string email, CancellationToken token)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(AppUser user, string userName, CancellationToken token)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task SetNormalizedUserNameAsync(AppUser user, string userName, CancellationToken token)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }


        public Task<string> GetPasswordHashAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken token)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(AppUser user, string email, CancellationToken token)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken token)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<AppUser> FindByEmailAsync(string email, CancellationToken token)
        {
            return _userRepo.FindByEmailAsync(email);
        }

        public Task SetPhoneNumberAsync(AppUser user, string phoneNumber, CancellationToken token)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed, CancellationToken token)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken token)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task AddLoginAsync(AppUser user, UserLoginInfo login, CancellationToken token)
        {
            user.Logins.Add(new ThirdPartyLoginInfo()
            {
                LoginId = Guid.NewGuid().ToString(),
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
            });
            return Task.FromResult(0);
        }

        public Task RemoveLoginAsync(AppUser user, string loginProvider, string providerKey, CancellationToken token)
        {
            var existingLogin = user.Logins.Where(lgi => lgi.ProviderKey == lgi.ProviderKey && lgi.LoginProvider == loginProvider).FirstOrDefault();
            user.Logins.Remove(existingLogin);
            return Task.FromResult(0);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(AppUser user, CancellationToken token)
        {
            //YUCK!
            IList<UserLoginInfo> logins = new List<UserLoginInfo>();
            if (user.Logins != null)
            {
                foreach (var login in user.Logins)
                {
                    logins.Add(new UserLoginInfo(login.LoginProvider, login.ProviderKey, login.DisplayName));
                }
            }
            return Task.FromResult(logins);
        }

        public Task<AppUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken token)
        {
            return _userRepo.FindByThirdPartyLogin(loginProvider, providerKey);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(AppUser user, CancellationToken token)
        {
            if (String.IsNullOrEmpty(user.LockoutDate))
            {
                return Task.FromResult< DateTimeOffset?>(null);

            }
            else
            {
                return Task.FromResult< DateTimeOffset?>(new DateTimeOffset(user.LockoutDate.ToDateTime()));
            }
        }

        public Task SetLockoutEndDateAsync(AppUser user, DateTimeOffset? lockoutEnd, CancellationToken token)
        {
            user.LockoutDate = lockoutEnd.HasValue ? lockoutEnd.Value.Date.ToJSONString() : String.Empty;
            return Task.FromResult(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(AppUser user, CancellationToken token)
        {
            user.AccessFailedCount += 1;
            return Task.FromResult(0);
        }

        public Task ResetAccessFailedCountAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(AppUser user, bool enabled, CancellationToken token)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(AppUser user, string stamp, CancellationToken token)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(AppUser user, CancellationToken token)
        {
            return Task.FromResult(user.SecurityStamp);
        }    

        public void Dispose()
        {
            //_userRepo.Dispose();
        }
    }
}

