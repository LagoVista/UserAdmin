// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c5efb7411a3e3708f02dc71581fc8d7aa0ae893e351091a2ba437269fc6effc2
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Users
{
    public class LocationUserRepo : TableStorageBase<LocationUser>, ILocationUserRepo
    {
        public LocationUserRepo(IUserAdminSettings settings, IAdminLogger logger) : 
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task AddUserToLocationAsync(LocationUser locationUser)
        {
            return InsertAsync(locationUser);
        }
       
        public Task<IEnumerable<LocationUser>> GetUsersForLocationAsync(string locationId)
        {
            return GetByParitionIdAsync(locationId);
        }

        public Task<IEnumerable<LocationUser>> GetLocationsForUserAsync(string userId)
        {
            return GetByFilterAsync(FilterOptions.Create(nameof(LocationUser.UserId), FilterOptions.Operators.Equals, userId));
        }

        public async Task RemoveUserFromLocationAsync(string locationId, string userId, EntityHeader removedBy)
        {
            var locationUser = await GetAsync(LocationUser.CreateRowId(locationId, userId));
            await RemoveAsync(locationUser);
        }
    }
}
