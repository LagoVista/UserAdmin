using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class AppUserIndboxRepo : TableStorageBase<AppUserInboxItem>, IAppUserInboxRepo
    {
        public AppUserIndboxRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        public Task AddInboxItemAsync(AppUserInboxItem item)
        {
            return InsertAsync(item);
        }

        public Task UpdateInboxItemAsync(AppUserInboxItem item)
        {
            return UpdateAsync(item);
        }

        public Task<ListResponse<AppUserInboxItem>> GetInboxItemsAsync(string orgId, string userId, ListRequest listRequest, bool unreadOnly = false)
        {
            var filteredItems = new List<FilterOptions>();
            if (unreadOnly)
                filteredItems.Add(FilterOptions.Create(nameof(AppUserInboxItem.Viewed), FilterOptions.Operators.Equals, "true") );

            return GetPagedResultsAsync(AppUserInboxItem.ConstructPartitionKey(orgId, userId), listRequest, filteredItems.ToArray());
        }

        public Task<AppUserInboxItem> GetItemInboxItemAsync(string paritionKey, string itemId)
        {
            return GetAsync(paritionKey, itemId);
        }
    }
}
