using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Security
{
    public class LocationRoleRepo : TableStorageBase<LocationUserRole>, ILocationRoleRepo
    {
        public LocationRoleRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task AddRoleForUserAsync(LocationUserRole locationUseRole)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConfirmUserInRoleAsync(string locationId, string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LocationUserRole>> GetUsersInRoleForLocationAsync(string roleId, string locationId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LocationUserRole>> GetRolesForUserInLocationAsync(string locationId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAllRolesForUserInLocationAsync(string userId, string locationId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRoleForUserInLocationAsync(string userId, string locationId, string roleId)
        {
            throw new NotImplementedException();
        }
    }
}
