using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
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

        public Task<IEnumerable<LocationAccount>> GetLocationsForAccountAsync(string accountId)
        {
            return GetByFilterAsync(FilterOptions.Create(nameof(LocationAccount.AccountId), FilterOptions.Operators.Equals, accountId));
        }

        public async Task RemoveAccountFromLocationAsync(string locationId, string accountId, EntityHeader removedBy)
        {
            var locationAccount = await GetAsync(LocationAccount.CreateRowId(locationId, accountId));
            await RemoveAsync(locationAccount);
        }
    }
}
