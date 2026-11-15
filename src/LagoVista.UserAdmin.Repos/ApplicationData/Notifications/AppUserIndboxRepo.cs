using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.Notifications
{
    /// <summary>
    /// Mongo ApplicationData replacement. User-specific partition and inverse-
    /// tick row keys remain stable so existing inbox controllers need no changes.
    /// </summary>
    public sealed class AppUserIndboxRepo : IAppUserInboxRepo
    {
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;

        public AppUserIndboxRepo(IApplicationDataStore store, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public Task AddInboxItemAsync(AppUserInboxItem item)
            => _store.InsertAsync(AppUserInboxRecord.FromModel(item,
                NotificationRecordIdentity.RequirePlatformOrg(_config)));

        public Task UpdateInboxItemAsync(AppUserInboxItem item)
            => _store.UpdateAsync(AppUserInboxRecord.FromModel(item,
                NotificationRecordIdentity.RequirePlatformOrg(_config)));

        public async Task<ListResponse<AppUserInboxItem>> GetInboxItemsAsync(
            string orgId, string userId, ListRequest listRequest, bool unreadOnly = false)
        {
            if (String.IsNullOrWhiteSpace(orgId)) throw new ArgumentNullException(nameof(orgId));
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            var partition = AppUserInboxItem.ConstructPartitionKey(orgId, userId);
            if (!NotificationRecordIdentity.TryPage(listRequest, partition, out var pageRequest, out var error))
                return ListResponse<AppUserInboxItem>.FromError(error);

            var query = new StorageQuery<AppUserInboxRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal,
                    NotificationRecordIdentity.RequirePlatformOrg(_config).Id)
                .Where(r => r.PartitionKey, StorageFilterOperator.Equal, partition)
                .OrderBy(r => r.RowKey, StorageSortDirection.Ascending)
                .WithPage(pageRequest);
            if (unreadOnly)
                query = query.Where(r => r.Viewed, StorageFilterOperator.Equal, false);

            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            return NotificationRecordIdentity.PageResponse(
                page.Items.Select(record => record.ToModel()),
                listRequest, partition, page.ContinuationToken);
        }

        public async Task<AppUserInboxItem> GetItemInboxItemAsync(string paritionKey, string itemId)
        {
            var id = NotificationRecordIdentity.For(paritionKey, itemId);
            var record = await _store.GetAsync<AppUserInboxRecord>(
                new StorageKey(id.Value, NotificationRecordIdentity.RequirePlatformOrg(_config).Id))
                .ConfigureAwait(false);
            return record != null && String.Equals(record.PartitionKey, paritionKey, StringComparison.Ordinal) &&
                String.Equals(record.RowKey, itemId, StringComparison.Ordinal)
                ? record.ToModel() : null;
        }

        public Task DeleteItemAsync(string partitionKey, string rowKey)
        {
            var id = NotificationRecordIdentity.For(partitionKey, rowKey);
            return _store.DeleteAsync<AppUserInboxRecord>(
                new StorageKey(id.Value, NotificationRecordIdentity.RequirePlatformOrg(_config).Id));
        }
    }
}
