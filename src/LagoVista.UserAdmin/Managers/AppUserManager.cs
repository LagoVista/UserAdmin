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

        public AppUserManager(IAppUserRepo appUserRepo, IDependencyManager depManager, ISecurity security, ILogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _appUserRepo = appUserRepo;
        }

        public async Task<InvokeResult> AddUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUser)
        {
            ValidationCheck(user, Actions.Create);

            await AuthorizeAsync(user, AuthorizeResult.AuthorizeActions.Update, updatedByUser, org);

            await _appUserRepo.CreateAsync(user);

            return new InvokeResult(); 
        }

        public async Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);
            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Read, user, org);

            return await CheckForDepenenciesAsync(appUser);
        }

        public async Task<InvokeResult> DeleteUserAsync(String  id, EntityHeader org, EntityHeader deletedByUser)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Delete, deletedByUser, org);      

            await _appUserRepo.DeleteAsync(appUser);

            return new InvokeResult();
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

        public async Task<InvokeResult> UpdateUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUser)
        {
            ValidationCheck(user, Actions.Update);

            await AuthorizeAsync(user, AuthorizeResult.AuthorizeActions.Update, updatedByUser, org);

            await _appUserRepo.UpdateAsync(user);

            return new InvokeResult();
        }
    }
}