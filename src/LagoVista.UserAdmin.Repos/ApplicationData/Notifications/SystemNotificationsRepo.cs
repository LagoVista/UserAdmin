using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.Notifications
{
    /// <summary>
    /// Mongo-backed organization and public notifications. Entries are durable
    /// and deleted explicitly, matching the previous repository's lifecycle.
    /// </summary>
    public sealed class SystemNotificationsRepo : ISystemNotificationsRepo
    {
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;

        public SystemNotificationsRepo(IApplicationDataStore store, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task AddSystemNotificationAsync(SystemNotification item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            // Match the old display order: Index increases as notifications arrive.
            var latestQuery = new StorageQuery<SystemNotificationRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, PlatformOrgId())
                .OrderBy(r => r.Index, StorageSortDirection.Descending)
                .WithPage(new StoragePageRequest(1));
            var latest = await _store.QueryAsync(latestQuery).ConfigureAwait(false);
            var max = latest.Items.Count > 0 ? latest.Items[0].Index : 0;
            if (max == Int32.MaxValue)
                throw new InvalidOperationException("System notification sequence is exhausted.");
            item.Index = max + 1;
            await _store.InsertAsync(SystemNotificationRecord.FromModel(item,
                NotificationRecordIdentity.RequirePlatformOrg(_config))).ConfigureAwait(false);
        }

        public Task UpdateSystemNotificationAsync(SystemNotification item)
            => _store.UpdateAsync(SystemNotificationRecord.FromModel(item,
                NotificationRecordIdentity.RequirePlatformOrg(_config)));

        public async Task<IEnumerable<SystemNotification>> GetSystemAndPublicNotifications(string orgId)
        {
            if (String.IsNullOrWhiteSpace(orgId)) throw new ArgumentNullException(nameof(orgId));
            var results = new List<SystemNotification>();
            await LoadPartitionAsync("public", results).ConfigureAwait(false);
            if (!String.Equals(orgId, "public", StringComparison.Ordinal))
                await LoadPartitionAsync(orgId, results).ConfigureAwait(false);
            return results.OrderByDescending(item => item.Index)
                .ThenBy(item => item.RowKey, StringComparer.Ordinal).ToList();
        }

        public Task DeleteSystemNotificationAsync(string partitionKey, string rowKey)
        {
            var id = NotificationRecordIdentity.For(partitionKey, rowKey);
            return _store.DeleteAsync<SystemNotificationRecord>(
                new StorageKey(id.Value, PlatformOrgId()));
        }

        public async Task<ListResponse<SystemNotification>> GetOrgNotifications(
            string orgId, ListRequest listRequest)
        {
            if (String.IsNullOrWhiteSpace(orgId)) throw new ArgumentNullException(nameof(orgId));
            if (!NotificationRecordIdentity.TryPage(listRequest, orgId, out var pageRequest, out var error))
                return ListResponse<SystemNotification>.FromError(error);

            var query = PartitionQuery(orgId).WithPage(pageRequest);
            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            return NotificationRecordIdentity.PageResponse(
                page.Items.Select(record => record.ToModel()),
                listRequest, orgId, page.ContinuationToken);
        }

        private StorageQuery<SystemNotificationRecord> PartitionQuery(string partition)
            => new StorageQuery<SystemNotificationRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, PlatformOrgId())
                .Where(r => r.PartitionKey, StorageFilterOperator.Equal, partition)
                .OrderBy(r => r.RowKey, StorageSortDirection.Ascending);

        private async Task LoadPartitionAsync(string partition, List<SystemNotification> output)
        {
            string continuation = null;
            do
            {
                var page = await _store.QueryAsync(
                    PartitionQuery(partition).WithPage(new StoragePageRequest(1000, continuation)))
                    .ConfigureAwait(false);
                output.AddRange(page.Items.Select(record => record.ToModel()));
                continuation = page.ContinuationToken;
            } while (!String.IsNullOrWhiteSpace(continuation));
        }

        private string PlatformOrgId() => NotificationRecordIdentity.RequirePlatformOrg(_config).Id;
    }
}
