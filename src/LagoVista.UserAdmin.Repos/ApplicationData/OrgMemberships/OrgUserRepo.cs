using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Repos.Orgs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.OrgMemberships
{
    /// <summary>
    /// Mongo-authoritative organization memberships with bounded, read-only Azure
    /// backfill. Removed associations are tombstoned so legacy reads fail closed.
    /// </summary>
    public sealed class OrgUserRepo : IOrgUserRepo
    {
        private static readonly ConcurrentDictionary<string, byte> ImportedOrganizations =
            new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;
        private readonly ILegacyOrgUserReader _legacy;
        private readonly ICacheProvider _cache;

        public OrgUserRepo(IApplicationDataStore store, IAppConfig config,
            ILegacyOrgUserReader legacy, ICacheProvider cache)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<bool> QueryOrgHasUserAsync(string orgId, string userId)
        {
            if (ProvisionalOrganizationBootstrapContext.TryConsumeMembershipProbe(orgId, userId))
                return false;
            return await GetOrgUserAsync(orgId, userId).ConfigureAwait(false) != null;
        }

        public async Task AddOrgUserAsync(OrgUser user)
        {
            var record = OrgMembershipRecord.FromModel(user, RequireOrg());
            var existing = await ReadAsync(record.OrgId, record.UserId).ConfigureAwait(false);
            if (existing != null && existing.IsDeleted)
                await _store.UpdateAsync(record).ConfigureAwait(false);
            else
                await _store.InsertAsync(record).ConfigureAwait(false);
            await ClearKeysAsync(record.OrgId, record.UserId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<OrgUser>> GetOrgsForUserAsync(string userId)
        {
            if (String.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.", nameof(userId));
            var old = await _legacy.GetLegacyForUserAsync(userId).ConfigureAwait(false);
            foreach (var member in old)
                await ImportIfMissingAsync(member).ConfigureAwait(false);
            var records = await ReadAllAsync(ForOwner()
                .Where(r => r.UserId, StorageFilterOperator.Equal, userId)).ConfigureAwait(false);
            return records.Where(r => !r.IsDeleted && r.UserId == userId)
                .Select(r => r.ToModel()).ToList();
        }

        public async Task<IEnumerable<OrgUser>> GetUsersForOrgAsync(string orgId)
        {
            await EnsureOrgImportedAsync(orgId).ConfigureAwait(false);
            var records = await ReadAllAsync(ForOrganization(orgId)).ConfigureAwait(false);
            return records.Where(r => !r.IsDeleted && r.OrgId == orgId)
                .Select(r => r.ToModel()).ToList();
        }

        public async Task RemoveUserFromOrgAsync(string orgId, string userId, EntityHeader removedBy)
        {
            var existing = await ReadAsync(orgId, userId).ConfigureAwait(false);
            var marker = existing ?? OrgMembershipRecord.Deleted(orgId, userId, RequireOrg(), removedBy);
            marker.IsDeleted = true;
            marker.LastUpdatedBy = removedBy?.Text;
            marker.LastUpdatedById = removedBy?.Id;
            if (existing != null)
                await _store.UpdateAsync(marker).ConfigureAwait(false);
            else
            {
                try { await _store.InsertAsync(marker).ConfigureAwait(false); }
                catch
                {
                    // A concurrent legacy import may have created the record.
                    var winner = await ReadAsync(orgId, userId).ConfigureAwait(false);
                    if (winner == null) throw;
                    winner.IsDeleted = true;
                    winner.LastUpdatedBy = removedBy?.Text;
                    winner.LastUpdatedById = removedBy?.Id;
                    await _store.UpdateAsync(winner).ConfigureAwait(false);
                }
            }
            await ClearKeysAsync(orgId, userId).ConfigureAwait(false);
        }

        public async Task<bool> QueryOrgHasUserByEmailAsync(string orgId, string email)
        {
            await EnsureOrgImportedAsync(orgId).ConfigureAwait(false);
            var key = OrgMembershipRecord.NormalizeEmail(email);
            if (key == null) return false;
            var records = await ReadAllAsync(ForOrganization(orgId)
                .Where(r => r.EmailKey, StorageFilterOperator.Equal, key)).ConfigureAwait(false);
            return records.Any(r => !r.IsDeleted && r.OrgId == orgId);
        }

        public async Task<bool> IsUserOrgAdminAsync(string orgId, string userId)
        {
            var member = await GetOrgUserAsync(orgId, userId).ConfigureAwait(false);
            if (member == null)
                throw new ArgumentNullException(nameof(member),
                    "OrgUser not found for OrgId: " + orgId + ", UserId: " + userId);
            return member.IsOrgAdmin;
        }

        public async Task<OrgUser> GetOrgUserAsync(string orgId, string userId)
        {
            var record = await ReadAsync(orgId, userId).ConfigureAwait(false);
            if (record != null)
                return record.IsDeleted ? null : record.ToModel();

            var old = await _legacy.GetLegacyAsync(orgId, userId).ConfigureAwait(false);
            if (old == null) return null;
            var imported = await ImportIfMissingAsync(old).ConfigureAwait(false);
            return imported.IsDeleted ? null : imported.ToModel();
        }

        public async Task UpdateOrgUserAsync(OrgUser member)
        {
            var record = OrgMembershipRecord.FromModel(member, RequireOrg());
            var existing = await ReadAsync(record.OrgId, record.UserId).ConfigureAwait(false);
            if (existing == null)
            {
                var old = await _legacy.GetLegacyAsync(record.OrgId, record.UserId).ConfigureAwait(false);
                if (old != null)
                    existing = await ImportIfMissingAsync(old).ConfigureAwait(false);
            }
            if (existing != null && existing.IsDeleted)
                throw new InvalidOperationException("Cannot update a removed organization membership.");
            await _store.UpdateAsync(record).ConfigureAwait(false);
            await ClearKeysAsync(record.OrgId, record.UserId).ConfigureAwait(false);
        }

        public async Task<bool> IsAppBuilderAsync(string orgId, string userId)
        {
            var member = await GetOrgUserAsync(orgId, userId).ConfigureAwait(false);
            return member?.IsAppBuilder == true;
        }

        public Task ClearOrgCacheAsync(string orgId)
            => _cache.RemoveAsync("ORG_USERS_" + orgId);

        private async Task ClearKeysAsync(string orgId, string userId)
        {
            await _cache.RemoveAsync("ORG_USERS_" + orgId).ConfigureAwait(false);
            await _cache.RemoveAsync("org_user_" + orgId + "_" + userId).ConfigureAwait(false);
        }

        private async Task<OrgMembershipRecord> ReadAsync(string orgId, string userId)
            => await _store.GetAsync<OrgMembershipRecord>(
                new StorageKey(OrgMembershipRecord.CreateId(orgId, userId).Value, RequireOrg().Id))
                .ConfigureAwait(false);

        private StorageQuery<OrgMembershipRecord> ForOwner()
            => new StorageQuery<OrgMembershipRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, RequireOrg().Id);

        private StorageQuery<OrgMembershipRecord> ForOrganization(string orgId)
        {
            if (String.IsNullOrWhiteSpace(orgId))
                throw new ArgumentException("OrgId is required.", nameof(orgId));
            return ForOwner().Where(r => r.OrgId, StorageFilterOperator.Equal, orgId);
        }

        private async Task<List<OrgMembershipRecord>> ReadAllAsync(
            StorageQuery<OrgMembershipRecord> query)
        {
            var result = new List<OrgMembershipRecord>();
            string continuation = null;
            do
            {
                var page = await _store.QueryAsync(query
                    .WithPage(new StoragePageRequest(500, continuation))).ConfigureAwait(false);
                result.AddRange(page.Items);
                continuation = page.ContinuationToken;
            } while (!String.IsNullOrWhiteSpace(continuation));
            return result;
        }

        private async Task EnsureOrgImportedAsync(string orgId)
        {
            if (ImportedOrganizations.ContainsKey(orgId)) return;
            var request = new ListRequest { PageIndex = 1, PageSize = 500 };
            for (var pageIndex = 0; pageIndex < 1000; pageIndex++)
            {
                var page = await _legacy.GetOrgPageAsync(orgId, request).ConfigureAwait(false);
                foreach (var member in page.Model)
                    await ImportIfMissingAsync(member).ConfigureAwait(false);

                if (!page.HasMoreRecords)
                {
                    ImportedOrganizations.TryAdd(orgId, 1);
                    return;
                }
                if (String.IsNullOrWhiteSpace(page.NextPartitionKey) ||
                    String.IsNullOrWhiteSpace(page.NextRowKey))
                    throw new InvalidOperationException("Legacy org membership continuation token was incomplete.");
                request = new ListRequest
                {
                    PageIndex = request.PageIndex + 1,
                    PageSize = request.PageSize,
                    NextPartitionKey = page.NextPartitionKey,
                    NextRowKey = page.NextRowKey
                };
            }
            throw new InvalidOperationException("Legacy org membership import exceeded 500000 records.");
        }

        private EntityHeader RequireOrg()
        {
            var org = _config.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for org membership storage.");
            return org;
        }

        private async Task<OrgMembershipRecord> ImportIfMissingAsync(OrgUser old)
        {
            var record = OrgMembershipRecord.FromModel(old, RequireOrg());
            var existing = await ReadAsync(record.OrgId, record.UserId).ConfigureAwait(false);
            if (existing != null) return existing;
            try { await _store.InsertAsync(record).ConfigureAwait(false); return record; }
            catch
            {
                var winner = await ReadAsync(record.OrgId, record.UserId).ConfigureAwait(false);
                if (winner != null) return winner;
                throw;
            }
        }
    }
}
