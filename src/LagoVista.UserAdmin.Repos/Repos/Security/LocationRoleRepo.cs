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

        public Task AddRoleForAccountAsync(LocationUserRole locationUseRole)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConfirmUserInRoleAsync(string locationId, string accountId, string roleId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LocationUserRole>> GetAccountsForRoleInLocationAsync(string roleId, string locationId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LocationUserRole>> GetRolesForAccountInLocationAsync(string locationId, string accountId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAllRolesForAccountInLocationAsync(string accountId, string locationId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRoleForAccountInLocationAsync(string accountId, string locationId, string roleId)
        {
            throw new NotImplementedException();
        }
    }
}
