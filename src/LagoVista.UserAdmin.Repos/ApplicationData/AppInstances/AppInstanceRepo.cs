using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Repos.Repos.Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.AppInstances
{
    /// <summary>
    /// Mongo-authoritative app instances with read-only Azure import during
    /// production cutover. Writes never go back to Table Storage.
    /// </summary>
    public sealed class AppInstanceRepo : IAppInstanceRepo
    {
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;
        private readonly LegacyAppInstanceReader _legacy;

        public AppInstanceRepo(IApplicationDataStore store, IAppConfig config,
            LegacyAppInstanceReader legacy)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
        }

        public Task AddAppInstanceAsync(AppInstance instance)
            => _store.InsertAsync(AppInstanceRecord.FromModel(instance, RequireOrg()));

        public async Task<AppInstance> GetAppInstanceAsync(string userId, string appInstanceId)
        {
            var id = AppInstanceRecord.CreateId(userId, appInstanceId);
            var stored = await _store.GetAsync<AppInstanceRecord>(
                new StorageKey(id.Value, RequireOrg().Id)).ConfigureAwait(false);
            if (stored != null)
                return Matches(stored, userId, appInstanceId) ? stored.ToModel() : null;

            var legacy = await _legacy.GetLegacyAsync(userId, appInstanceId).ConfigureAwait(false);
            if (legacy == null) return null;
            return (await ImportIfMissingAsync(legacy).ConfigureAwait(false)).ToModel();
        }

        public async Task<IEnumerable<AppInstance>> GetForUserAsync(string userId)
        {
            if (String.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.", nameof(userId));

            // Read and import every legacy entry on each listing, so records not yet
            // visited by key cannot disappear during the cutover. Mongo always wins
            // when an existing record has been updated since its Azure copy.
            var legacy = await _legacy.GetForUserLegacyAsync(userId).ConfigureAwait(false);
            foreach (var item in legacy)
                await ImportIfMissingAsync(item).ConfigureAwait(false);

            var records = new List<AppInstanceRecord>();
            string continuation = null;
            do
            {
                var query = new StorageQuery<AppInstanceRecord>()
                    .Where(r => r.Organization.Id, StorageFilterOperator.Equal, RequireOrg().Id)
                    .Where(r => r.UserPartition, StorageFilterOperator.Equal, userId)
                    .WithPage(new StoragePageRequest(500, continuation));
                var page = await _store.QueryAsync(query).ConfigureAwait(false);
                records.AddRange(page.Items.Where(r => r.UserPartition == userId));
                continuation = page.ContinuationToken;
            } while (!String.IsNullOrWhiteSpace(continuation));

            return records.Select(r => r.ToModel()).ToList();
        }

        public async Task UpdateAppInstanceAsync(AppInstance instance)
        {
            var updated = AppInstanceRecord.FromModel(instance, RequireOrg());
            var key = new StorageKey(updated.Id.Value, RequireOrg().Id);
            var exists = await _store.GetAsync<AppInstanceRecord>(key).ConfigureAwait(false);
            if (exists == null)
            {
                var old = await _legacy.GetLegacyAsync(updated.UserPartition, updated.InstanceKey)
                    .ConfigureAwait(false);
                if (old != null)
                    await ImportIfMissingAsync(old).ConfigureAwait(false);
            }
            await _store.UpdateAsync(updated).ConfigureAwait(false);
        }

        private async Task<AppInstanceRecord> ImportIfMissingAsync(AppInstance legacy)
        {
            var record = AppInstanceRecord.FromModel(legacy, RequireOrg());
            var key = new StorageKey(record.Id.Value, RequireOrg().Id);
            var existing = await _store.GetAsync<AppInstanceRecord>(key).ConfigureAwait(false);
            if (existing != null) return existing;

            try
            {
                await _store.InsertAsync(record).ConfigureAwait(false);
                return record;
            }
            catch
            {
                // Concurrent first-read import may have won; never replace it.
                var winner = await _store.GetAsync<AppInstanceRecord>(key).ConfigureAwait(false);
                if (winner != null) return winner;
                throw;
            }
        }

        private bool Matches(AppInstanceRecord record, string userId, string instanceId)
            => String.Equals(record.UserPartition, userId, StringComparison.Ordinal) &&
               String.Equals(record.InstanceKey, instanceId, StringComparison.Ordinal);

        private EntityHeader RequireOrg()
        {
            var org = _config.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for AppInstance storage.");
            return org;
        }
    }
}
