using Azure;
using Azure.Data.Tables;
using LagoVista.CloudStorage.Storage;
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
        private readonly TableClient _tableClient;
        private readonly IAdminLogger _adminLogger;
        private readonly IBackgroundServiceTaskQueue _bgServiceQueue;
        private static bool _created;

        public AccessLogRepo(IUserAdminSettings settings, IBackgroundServiceTaskQueue bgServiceQueue, IAdminLogger logger) :
            base(settings.AccessLogTableStorage.AccountId, settings.AccessLogTableStorage.AccessKey, logger)
        {
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            var tableName = GetTableName();
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={settings.AccessLogTableStorage.AccountId};AccountKey={settings.AccessLogTableStorage.AccessKey}";
            _tableClient = new TableClient(connectionString, tableName);
            _bgServiceQueue = bgServiceQueue;
        }

        public void AddActivity(AccessLog accessLog)
        {
            _bgServiceQueue.QueueBackgroundWorkItemAsync(async (ct) =>
            {
                var sw = Stopwatch.StartNew();

                if (!_created)
                {
                    await _tableClient.CreateIfNotExistsAsync();
                    _created = true;
                }

                await _tableClient.AddEntityAsync<AccessLogEntity>(AccessLogEntity.FromAccessLog(accessLog));
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, $"[AccessLogRepo__AddActivity]", $"[AccessLogRepo__AddActivity] Org: {accessLog.OrgName} {accessLog.Resource} - {accessLog.ResourceId} {sw.Elapsed.TotalMilliseconds}ms");
            });
        }

        public Task AddActivityAsync(AccessLog accessLog)
        {
            return _bgServiceQueue.QueueBackgroundWorkItemAsync(async (ct) =>
            {
                var sw = Stopwatch.StartNew();

                if (!_created)
                {
                    await _tableClient.CreateIfNotExistsAsync();
                    _created = true;
                }

                await _tableClient.AddEntityAsync<AccessLogEntity>(AccessLogEntity.FromAccessLog(accessLog));
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, $"[AccessLogRepo__AddActivityAsync]", $"[AccessLogRepo__AddActivityAsync] Org: {accessLog.OrgName} {accessLog.Resource} - {accessLog.ResourceId} {sw.Elapsed.TotalMilliseconds}ms");
            });
        }

        public Task<IEnumerable<AccessLog>> GetForResourceAsync(string resourceId)
        {
            return GetByParitionIdAsync(resourceId);
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
