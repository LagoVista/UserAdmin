// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 991994d6cda9a252ee382f63bccc5b3c6d718f9476c5f1e1644e9b436e0c6dea
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class AppUserManagerReadOnly : ManagerBase, IAppUserManagerReadOnly
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly IAppConfig _appConfig;

        public AppUserManagerReadOnly(IAppUserRepo appUserRepo, IUserRoleRepo userRoleRepo, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appUserRepo));
        }

        public async Task<AppUser> GetUserByIdAsync(string appUserId, EntityHeader org, EntityHeader requestedByUser)
        {
            if(String.IsNullOrEmpty(appUserId))
            {
                throw new ArgumentNullException(nameof(appUserId));
            }

            var appUser = await _appUserRepo.FindByIdAsync(appUserId);
            if(appUser == null)
            {
                throw new LagoVista.Core.Exceptions.RecordNotFoundException(nameof(AppUser), appUserId);
            }

            var userRoles = await _userRoleRepo.GetRolesForUserAsync(appUserId, org.Id);

            appUser.CurrentOrganizationRoles = userRoles.Select(ur => ur.ToEntityHeader()).ToList();
            appUser.PasswordHash = null;

            /* The user should always be able to get it's own account */
            if (requestedByUser.Id != appUserId)
            {
                await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Read, requestedByUser, org);
            }

            return appUser;
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName, EntityHeader org, EntityHeader requestedByUser)
        {
            var appUser = await _appUserRepo.FindByEmailAsync(userName);
            
            if(appUser == null)
            {
                return null;
            }

            // we use this for integration tests in the dev environment, shouldn't be a security hole.
            if(_appConfig.Environment == Environments.Production ||
                _appConfig.Environment == Environments.Staging)
                await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Read, requestedByUser, org);

            appUser.PasswordHash = null;
            return appUser;
        }
        public Task<IEnumerable<EntityHeader>> SearchUsers(string firstName, string lastName, EntityHeader searchedBy)
        {
            throw new NotImplementedException();
        }
    }
}
