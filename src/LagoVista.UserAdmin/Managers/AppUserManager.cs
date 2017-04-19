using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Account;
using LagoVista.Core.Managers;
using LagoVista.Core.Interfaces;

namespace LagoVista.UserAdmin.Managers
{
    public class AppUserManager : ManagerBase, IAppUserManager
    {
        IAppUserRepo _appUserRepo;

        public AppUserManager(IAppUserRepo appUserRepo, ILogger logger, IAppConfig appConfig) : base(logger, appConfig)
        {
            _appUserRepo = appUserRepo;
        }

        public async Task<InvokeResult> AddUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUserId)
        {
            ValidationCheck(user, Actions.Create);
            
            await _appUserRepo.CreateAsync(user);

            return default(InvokeResult);
        }

        public async Task<InvokeResult> DeleteUserAsync(String  id, EntityHeader deletedByUser)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);

            await _appUserRepo.DeleteAsync(appUser);

            return default(InvokeResult);
        }

        public async Task<AppUser> GetUserByIdAsync(string id, EntityHeader requestedByUser)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);
            appUser.PasswordHash = null;
            return appUser;
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName, EntityHeader requestedByUser)
        {
            var appUser = await _appUserRepo.FindByNameAsync(userName);
            appUser.PasswordHash = null;
            return appUser;
        }

        public Task<IEnumerable<EntityHeader>> SearchUsers(string firstName, string lastName, EntityHeader searchedBy)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> UpdateUserAsync(AppUser user, EntityHeader updatedByUser)
        {
            await _appUserRepo.UpdateAsync(user);

            return default(InvokeResult);
        }
    }
}
