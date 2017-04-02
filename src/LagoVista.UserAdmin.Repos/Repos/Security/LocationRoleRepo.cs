using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Security;

namespace LagoVista.UserAdmin.Repos.Security
{
    public class LocationRoleRepo : TableStorageBase<LocationAccountRoles>, ILocationRoleRepo
    {
        public LocationRoleRepo(IAppUserManagementSettings settings, ILogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }


        public Task<LocationAccountRoles> AddRoleForAccountAsync(EntityHeader location, EntityHeader account, EntityHeader role, EntityHeader addeBy)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LocationAccountRoles>> GetAccountsForRoleInLocationAsync(string roleId, string locationId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LocationAccountRoles>> GetRolesForAccountInLocationAsync(string locationId, string accountId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAllRolesForAccountInLocationAsync(EntityHeader location, EntityHeader acount, EntityHeader revokedBy)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRoleForAccountInLocationAsync(EntityHeader location, EntityHeader role, EntityHeader account, EntityHeader revokedBy)
        {
            throw new NotImplementedException();
        }
    }
}
