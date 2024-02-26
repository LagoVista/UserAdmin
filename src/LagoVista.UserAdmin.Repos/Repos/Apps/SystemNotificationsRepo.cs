using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Apps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Apps
{
    public class SystemNotificationsRepo : TableStorageBase<SystemNotification>, ISystemNotificationsRepo
    {
        public SystemNotificationsRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        public async Task AddSystemNotificationAsync(SystemNotification item)
        {
            var items = await GetByFilterAsync();
            if (items.Any())
                item.Index = items.First().Index + 1;
            else
                item.Index = 1;

            await InsertAsync(item);
        }

        public Task UpdateSystemNotificationAsync(SystemNotification item)
        {
            return UpdateAsync(item);
        }

        public async Task<IEnumerable<SystemNotification>> GetSystemAndPublicNotifications(string orgId)
        {
            var filteredItems = new List<FilterOptions>();
            var publicResults = await GetByParitionIdAsync("public");
            var orgReults = await GetByParitionIdAsync(orgId);

            var results = new List<SystemNotification>();
            results.AddRange(publicResults);
            results.AddRange(orgReults);

            return new List<SystemNotification>(results.OrderByDescending(col => col.Index));
        }

        public Task DeleteSystemNotificationAsync(string partitionKey, string rowKey)
        {
            return RemoveAsync(partitionKey, rowKey);
        }

        public async Task<ListResponse<SystemNotification>> GetOrgNotifications(string orgId, ListRequest listRequest)
        {
            return await GetPagedResultsAsync(orgId, listRequest);
        }
    }
}
