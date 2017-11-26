using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class AppUserManagerReadOnly : ManagerBase, IAppUserManagerReadOnly
    {
        IAppUserRepo _appUserRepo;

        public AppUserManagerReadOnly(IAppUserRepo appUserRepo, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _appUserRepo = appUserRepo;
        }

        public async Task<AppUser> GetUserByIdAsync(string id, EntityHeader org, EntityHeader requestedByUser)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);
            appUser.PasswordHash = null;

            /* The user should always be able to get it's own account */
            if (requestedByUser.Id != id)
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
