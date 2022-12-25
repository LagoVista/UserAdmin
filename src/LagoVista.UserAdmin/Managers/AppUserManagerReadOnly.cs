using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
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

        public AppUserManagerReadOnly(IAppUserRepo appUserRepo, IUserRoleRepo userRoleRepo, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
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
            var appUser = await _appUserRepo.FindByNameAsync(userName);
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
