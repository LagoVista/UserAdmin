// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 93020d708e87f707962c946b625ac5f4491a4d477ea6016fd48e2cc27d0bbaf8
// IndexVersion: 2
// --- END CODE INDEX META ---
using Azure;
using Azure.Data.Tables;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class AccessLogRepo : TableStorageBase<AccessLog>, IAccessLogRepo
    {
        private TableClient _tableClient;
        private readonly IAdminLogger _adminLogger;
        private readonly IBackgroundServiceTaskQueue _bgServiceQueue;
        private string _tableName;
        private string _connectionString;

        public AccessLogRepo(IUserAdminSettings settings, IBackgroundServiceTaskQueue bgServiceQueue, IAdminLogger logger) :
            base(settings.AccessLogTableStorage.AccountId, settings.AccessLogTableStorage.AccessKey, logger)
        {
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableName = GetTableName();
            _connectionString = $"DefaultEndpointsProtocol=https;AccountName={settings.AccessLogTableStorage.AccountId};AccountKey={settings.AccessLogTableStorage.AccessKey}";
            _tableClient = new TableClient(_connectionString, _tableName);
            _bgServiceQueue = bgServiceQueue;
        }

        public override StoragePeriod GetStoragePeriod()
        {
            return StoragePeriod.Month;
        }

        private async Task<TableClient> GetTableClient()
        {
            if (_tableName != GetTableName() || _tableClient == null)
            {
                _tableName = GetTableName();
                _tableClient = new TableClient(_connectionString, _tableName);
                await _tableClient.CreateIfNotExistsAsync();
            }

            return _tableClient;
        }


        public void AddActivity(AccessLog accessLog)
        {
            _bgServiceQueue.QueueBackgroundWorkItemAsync(async (ct) =>
            {
                try
                {
                    var tableClient = await GetTableClient();
                    await tableClient.AddEntityAsync<AccessLogEntity>(AccessLogEntity.FromAccessLog(accessLog));
                   // _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, $"[AccessLogRepo__AddActivity]", $"[AccessLogRepo__AddActivity] Org: {accessLog.OrgName} {accessLog.Resource} - {accessLog.ResourceId} {sw.Elapsed.TotalMilliseconds}ms");
                }
                catch(Exception ex)
                {
                    _adminLogger.AddException("[AccessLogRepo_AddActivity]",  ex, GetTableName().ToKVP("tableName"));
                }
            
            });
        }

        public Task AddActivityAsync(AccessLog accessLog)
        {
            return _bgServiceQueue.QueueBackgroundWorkItemAsync(async (ct) =>
            {
                var sw = Stopwatch.StartNew();
                var tableClient = await GetTableClient();
                await tableClient.AddEntityAsync<AccessLogEntity>(AccessLogEntity.FromAccessLog(accessLog));
                //_adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, $"[AccessLogRepo__AddActivityAsync]", $"[AccessLogRepo__AddActivityAsync] Org: {accessLog.OrgName} {accessLog.Resource} - {accessLog.ResourceId} {sw.Elapsed.TotalMilliseconds}ms");
            });
        }

        public Task<IEnumerable<AccessLog>> GetForResourceAsync(string resourceId)
        {
            return GetByPartitionIdAsync(resourceId);
        }

        public Task<IEnumerable<AccessLog>> GetForResourceAsync(string resourceId, string startTimeStamp, string endTimeStamp)
        {
            throw new NotImplementedException();
        }
    }

    public class AccessLogEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string DateStamp { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public string OrgName { get; set; }
        public string OrgId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public bool Authorized { get; set; }
        public string NotAuthorizedReason { get; set; }

        public static AccessLogEntity FromAccessLog(AccessLog accessLog)
        {
            return new AccessLogEntity()
            {
                RowKey = accessLog.RowKey,
                Timestamp = DateTime.UtcNow,
                ETag = ETag.All,
                PartitionKey = accessLog.PartitionKey,
                DateStamp = accessLog.DateStamp,
                Resource = accessLog.Resource,
                ResourceId = accessLog.ResourceId,
                OrgName = accessLog.OrgName,
                OrgId = accessLog.OrgId,
                UserId = accessLog.UserId,
                UserName = accessLog.UserName,
                Action = accessLog.Action,
                Details = accessLog.Details,
                Authorized = accessLog.Authorized,
                NotAuthorizedReason = accessLog.NotAuthorizedReason

            };
        }
    }

}
