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
            return GetByParitionIdAsync(userId);
        }

        public Task UpdateAppInstanceAsync(AppInstance appInstance)
        {
            return UpdateAsync(appInstance);
        }
    }
}
