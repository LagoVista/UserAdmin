// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ad7b7c331754d90b3c17615c947af4b13c5debd51cced5cd3e3a81df604240c9
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Models.Apps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Apps
{
    public class AppInstanceRepo : TableStorageBase<AppInstance>, IAppInstanceRepo
    {
        public AppInstanceRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task AddAppInstanceAsync(AppInstance appInstance)
        {
            return InsertAsync(appInstance);
        }

        public Task<AppInstance> GetAppInstanceAsync(string userId, string appInstanceId)
        {
            return GetAsync(userId, appInstanceId, false);
        }

        public Task<IEnumerable<AppInstance>> GetForUserAsync(string userId)
        {
            return GetByPartitionIdAsync(userId);
        }

        public Task UpdateAppInstanceAsync(AppInstance appInstance)
        {
            return UpdateAsync(appInstance);
        }
    }
}
