using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Apps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Apps
{
    /// <summary>
    /// Read-only Azure compatibility for existing AppInstances. Remove only after
    /// production backfill and count/key reconciliation have been verified.
    /// </summary>
    public sealed class LegacyAppInstanceReader : TableStorageBase<AppInstance>
    {
        public LegacyAppInstanceReader(IUserAdminSettings settings, IAdminLogger logger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger) { }

        public Task<AppInstance> GetLegacyAsync(string userId, string instanceId)
            => GetAsync(userId, instanceId, false);

        public Task<IEnumerable<AppInstance>> GetForUserLegacyAsync(string userId)
            => GetByPartitionIdAsync(userId);
    }
}
