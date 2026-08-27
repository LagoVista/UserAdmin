// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 93020d708e87f707962c946b625ac5f4491a4d477ea6016fd48e2cc27d0bbaf8
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class AccessLogRepo : IAccessLogRepo
    {
        private readonly IActivityRecordStore<AccessLog> _store;
        private readonly IAdminLogger _adminLogger;
        private readonly IBackgroundServiceTaskQueue _bgServiceQueue;

        public AccessLogRepo(
            IActivityRecordStore<AccessLog> store,
            IBackgroundServiceTaskQueue bgServiceQueue,
            IAdminLogger logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _bgServiceQueue = bgServiceQueue ?? throw new ArgumentNullException(nameof(bgServiceQueue));
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static void ConfigureStorage(StorageDefinition<AccessLog> definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            definition
                .PartitionBy(log => log.OrganizationId)
                .BucketBy(StoragePeriod.Month)
                .Index(log => log.ResourceId)
                .Index(log => log.UserId)
                .Index(log => log.Action)
                .Index(log => log.Authorized);
        }

        public void AddActivity(AccessLog accessLog)
        {
            _ = _bgServiceQueue.QueueBackgroundWorkItemAsync(async ct =>
            {
                try
                {
                    PrepareForInsert(accessLog);
                    await _store.InsertAsync(accessLog, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _adminLogger.AddException("[AccessLogRepo_AddActivity]", ex);
                }
            });
        }

        public Task AddActivityAsync(AccessLog accessLog)
        {
            return _bgServiceQueue.QueueBackgroundWorkItemAsync(async ct =>
            {
                PrepareForInsert(accessLog);
                await _store.InsertAsync(accessLog, ct).ConfigureAwait(false);
            });
        }

        public async Task<IEnumerable<AccessLog>> GetForResourceAsync(
            string organizationId,
            string resourceId,
            DateTime start,
            DateTime end)
        {
            var query = CreateQuery(organizationId, start, end)
                .Where(log => log.ResourceId, StorageFilterOperator.Equal, resourceId);

            return (await _store.QueryAsync(query).ConfigureAwait(false)).Items;
        }

        public async Task<IEnumerable<AccessLog>> GetForUserAsync(
            string organizationId,
            string userId,
            DateTime start,
            DateTime end)
        {
            var query = CreateQuery(organizationId, start, end)
                .Where(log => log.UserId, StorageFilterOperator.Equal, userId);

            return (await _store.QueryAsync(query).ConfigureAwait(false)).Items;
        }

        private static HistoryQuery<AccessLog> CreateQuery(string organizationId, DateTime start, DateTime end)
        {
            if (String.IsNullOrWhiteSpace(organizationId)) throw new ArgumentNullException(nameof(organizationId));

            return new HistoryQuery<AccessLog>()
                .Where(log => log.OrganizationId, StorageFilterOperator.Equal, organizationId)
                .Between(start, end);
        }

        private static void PrepareForInsert(AccessLog accessLog)
        {
            if (accessLog == null) throw new ArgumentNullException(nameof(accessLog));
            if (String.IsNullOrWhiteSpace(accessLog.OrganizationId))
            {
                throw new InvalidOperationException("AccessLog requires OrganizationId before it can be persisted.");
            }

            if (String.IsNullOrWhiteSpace(accessLog.Id)) accessLog.Id = Guid.NewGuid().ToString("N");
            if (accessLog.CreationDate == default) accessLog.CreationDate = DateTime.UtcNow;
        }
    }
}
