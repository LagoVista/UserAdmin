using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using System;
using LagoVista.UserAdmin.Models.Account;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;

namespace LagoVista.UserAdmin.Repos.Account
{
    public class LocationAccountRepo : TableStorageBase<LocationAccount>, ILocationAccountRepo
    {
        public LocationAccountRepo(IUserAdminSettings settings, ILogger logger) : 
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task AddAccountToLocationAsync(LocationAccount locationAccount)
        {
            return InsertAsync(locationAccount);
        }
       
        public Task<IEnumerable<LocationAccount>> GetAccountsForLocationAsync(string locationId)
        {
            return GetByParitionIdAsync(locationId);
        }

        public Task<IEnumerable<LocationAccount>> GetLocationsForAccountAsync(string userId)
        {
            return GetByFilterAsync(FilterOptions.Create("UserId", FilterOptions.Operators.Equals, userId));
        }

        public async Task RemoveAccountFromLocationAsync(string locationId, string accountId, EntityHeader removedBy)
        {
            var rowKey =  LocationAccount.CreateRowId(locationId, accountId);
            var locationAccount = await GetAsync(rowKey);
            await RemoveAsync(locationAccount);
        }
    }
}
