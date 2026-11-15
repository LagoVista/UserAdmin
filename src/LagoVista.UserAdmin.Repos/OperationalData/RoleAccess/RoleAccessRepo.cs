using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Repos.Repos.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.OperationalData.RoleAccess
{
    /// <summary>
    /// Cassandra-authoritative role permissions with scoped and global public
    /// partitions. Lazy Azure imports use CAS; revocations are retained as
    /// permanent markers so old public or private permissions cannot resurface.
    /// </summary>
    public sealed class RoleAccessRepo : IRoleAccessRepo
    {
        public const string IS_PUBLIC = RoleAccessOperationalRecord.PublicPartition;

        private static readonly ConcurrentDictionary<string, byte> ImportedOrganizations =
            new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);
        private static volatile bool _publicImported;

        private readonly IConditionalOperationalDataStore<RoleAccessOperationalRecord> _store;
        private readonly ILegacyRoleAccessReader _legacy;
        private readonly ICacheProvider _cache;
        private readonly IAppConfig _config;

        public RoleAccessRepo(IConditionalOperationalDataStore<RoleAccessOperationalRecord> store,
            ILegacyRoleAccessReader legacy, ICacheProvider cache, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public static void ConfigureStorage(StorageDefinition<RoleAccessOperationalRecord> definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            definition.PartitionBy(r => r.OrganizationId)
                .KeyBy(r => r.Id)
                .Index(r => r.RoleId)
                .Index(r => r.ModuleId)
                .Index(r => r.AreaId)
                .Index(r => r.PageId)
                .Index(r => r.FeatureId);
            // No TTL: revocation markers are retained to prevent legacy import.
        }

        public string CacheKey(string roleId, string organizationId)
            => nameof(RoleAccess) + "__" + roleId + "__" + organizationId;

        public async Task AddRoleAccess(RoleAccess roleAccess)
        {
            if (roleAccess == null) throw new ArgumentNullException(nameof(roleAccess));
            var wanted = RoleAccessOperationalRecord.FromDTO(roleAccess.ToDTO());
            var current = await _store.GetAsync(wanted.OrganizationId, wanted.Id).ConfigureAwait(false);
            if (current == null)
            {
                var old = await _legacy.GetLegacyAsync(wanted.SourceOrganizationId, wanted.Id)
                    .ConfigureAwait(false);
                if (old != null) current = await ImportIfMissingAsync(old).ConfigureAwait(false);
            }
            for (var attempt = 0; attempt < 5; attempt++)
            {
                if (current == null)
                {
                    var created = await _store.TryCreateAsync(wanted).ConfigureAwait(false);
                    if (created.Applied)
                    {
                        await InvalidateAsync(wanted).ConfigureAwait(false);
                        return;
                    }
                    current = created.Current ?? await _store.GetAsync(wanted.OrganizationId, wanted.Id)
                        .ConfigureAwait(false);
                }
                if (current == null) continue;
                if (!current.IsDeleted)
                    throw new InvalidOperationException("Role access assignment already exists.");
                var replaced = await _store.TryReplaceAsync(wanted, current.Version)
                    .ConfigureAwait(false);
                if (replaced.Applied)
                {
                    await InvalidateAsync(wanted).ConfigureAwait(false);
                    return;
                }
                current = replaced.Current;
            }
            throw new InvalidOperationException("Concurrent changes prevented role permission assignment.");
        }

        public async Task RemoveRoleAccess(string roleAccessId, string organizationId)
        {
            if (String.IsNullOrWhiteSpace(roleAccessId)) throw new ArgumentException("Role access Id required.", nameof(roleAccessId));
            if (String.IsNullOrWhiteSpace(organizationId)) throw new ArgumentException("OrganizationId required.", nameof(organizationId));
            var current = await _store.GetAsync(organizationId, roleAccessId).ConfigureAwait(false);
            if (current == null)
            {
                var publicCandidate = await _store.GetAsync(IS_PUBLIC, roleAccessId).ConfigureAwait(false);
                if (publicCandidate != null &&
                    (String.Equals(publicCandidate.SourceOrganizationId, organizationId, StringComparison.Ordinal) ||
                     organizationId == IS_PUBLIC))
                    current = publicCandidate;
            }
            if (current == null)
            {
                var old = await _legacy.GetLegacyAsync(organizationId, roleAccessId)
                    .ConfigureAwait(false);
                if (old != null) current = await ImportIfMissingAsync(old).ConfigureAwait(false);
            }
            var partition = current?.OrganizationId ?? organizationId;
            for (var attempt = 0; attempt < 6; attempt++)
            {
                if (current == null)
                {
                    var marker = RoleAccessOperationalRecord.Tombstone(partition, roleAccessId);
                    var created = await _store.TryCreateAsync(marker).ConfigureAwait(false);
                    if (created.Applied) return;
                    current = created.Current;
                }
                if (current == null) continue;
                if (current.IsDeleted) return;

                var tombstone = RoleAccessOperationalRecord.Tombstone(partition, roleAccessId);
                tombstone.SourceOrganizationId = current.SourceOrganizationId;
                tombstone.SourceOrganizationName = current.SourceOrganizationName;
                tombstone.RoleId = current.RoleId;
                tombstone.RoleKey = current.RoleKey;
                tombstone.RoleName = current.RoleName;
                tombstone.IsPublic = current.IsPublic;
                var replaced = await _store.TryReplaceAsync(tombstone, current.Version)
                    .ConfigureAwait(false);
                if (replaced.Applied)
                {
                    await InvalidateAsync(current).ConfigureAwait(false);
                    return;
                }
                current = replaced.Current;
            }
            throw new InvalidOperationException("Concurrent role permission changes prevented revocation.");
        }

        public Task<List<RoleAccess>> GetRoleAccessForModuleAsync(string moduleId, string organizationId)
            => GetForScopeAsync(moduleId, null, null, null, organizationId);

        public Task<List<RoleAccess>> GetRoleAccessForAreaAsync(string moduleId,
            string areaId, string organizationId)
            => GetForScopeAsync(moduleId, areaId, null, null, organizationId);

        public Task<List<RoleAccess>> GetRoleAccessForPageAsync(string moduleId,
            string areaId, string pageId, string organizationId)
            => GetForScopeAsync(moduleId, areaId, pageId, null, organizationId);

        public Task<List<RoleAccess>> GetRoleAccessForModuleFeatureAsync(string moduleId,
            string featureId, string organizationId)
            => GetForScopeAsync(moduleId, null, null, featureId, organizationId);

        public Task<List<RoleAccess>> GetRoleAccessForAreaFeatureAsync(string moduleId,
            string areaId, string featureId, string organizationId)
            => GetForScopeAsync(moduleId, areaId, null, featureId, organizationId);

        public Task<List<RoleAccess>> GetRoleAccessForPageFeatureAsync(string moduleId,
            string areaId, string pageId, string featureId, string organizationId)
            => GetForScopeAsync(moduleId, areaId, pageId, featureId, organizationId);

        public async Task<List<RoleAccess>> GetRoleAccessForRoleAsync(string roleId, string organizationId)
        {
            if (String.IsNullOrWhiteSpace(roleId)) throw new ArgumentException("RoleId required.", nameof(roleId));
            await EnsureOrgImportedAsync(organizationId).ConfigureAwait(false);
            var systemOrgId = _config.SystemOwnerOrg?.Id;
            if (String.IsNullOrWhiteSpace(systemOrgId))
                throw new InvalidOperationException("SystemOwnerOrg is required for role access.");
            if (!String.Equals(organizationId, systemOrgId, StringComparison.Ordinal))
                await EnsureOrgImportedAsync(systemOrgId).ConfigureAwait(false);
            await EnsurePublicImportedAsync().ConfigureAwait(false);

            var result = await ReadMatchingAsync(organizationId, roleId, null, null, null, null)
                .ConfigureAwait(false);
            if (!String.Equals(organizationId, systemOrgId, StringComparison.Ordinal))
                result.AddRange(await ReadMatchingAsync(systemOrgId, roleId, null, null, null, null)
                    .ConfigureAwait(false));
            result.AddRange(await ReadMatchingAsync(IS_PUBLIC, roleId, null, null, null, null)
                .ConfigureAwait(false));

            return result.Where(r => !r.IsDeleted &&
                    (r.OrganizationId == IS_PUBLIC ? r.IsPublic : !r.IsPublic))
                .GroupBy(r => r.OrganizationId + ":" + r.Id, StringComparer.Ordinal)
                .Select(g => g.First().ToDTO().ToRoleAccess())
                .OrderBy(r => r.Role.Text).ToList();
        }

        private async Task<List<RoleAccess>> GetForScopeAsync(string moduleId, string areaId,
            string pageId, string featureId, string orgId)
        {
            if (String.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentException("ModuleId required.", nameof(moduleId));
            await EnsureOrgImportedAsync(orgId).ConfigureAwait(false);
            await EnsurePublicImportedAsync().ConfigureAwait(false);

            var orgRows = await ReadMatchingAsync(orgId, null, moduleId, areaId, pageId, featureId)
                .ConfigureAwait(false);
            var publicRows = await ReadMatchingAsync(IS_PUBLIC, null, moduleId, areaId, pageId, featureId)
                .ConfigureAwait(false);
            return orgRows.Where(r => !r.IsDeleted && !r.IsPublic)
                .Concat(publicRows.Where(r => !r.IsDeleted && r.IsPublic))
                .GroupBy(r => r.OrganizationId + ":" + r.Id, StringComparer.Ordinal)
                .Select(group => group.First().ToDTO().ToRoleAccess())
                .OrderBy(r => r.Role.Text).ToList();
        }

        private async Task<List<RoleAccessOperationalRecord>> ReadMatchingAsync(
            string orgId, string roleId, string moduleId, string areaId, string pageId, string featureId)
        {
            var query = new StorageQuery<RoleAccessOperationalRecord>()
                .Where(r => r.OrganizationId, StorageFilterOperator.Equal, orgId);
            if (roleId != null) query = query.Where(r => r.RoleId, StorageFilterOperator.Equal, roleId);
            if (moduleId != null) query = query.Where(r => r.ModuleId, StorageFilterOperator.Equal, moduleId);
            if (areaId != null) query = query.Where(r => r.AreaId, StorageFilterOperator.Equal, areaId);
            if (pageId != null) query = query.Where(r => r.PageId, StorageFilterOperator.Equal, pageId);
            if (featureId != null) query = query.Where(r => r.FeatureId, StorageFilterOperator.Equal, featureId);

            var records = new List<RoleAccessOperationalRecord>();
            string continuation = null;
            do
            {
                var page = await _store.QueryAsync(
                    query.WithPage(new StoragePageRequest(500, continuation))).ConfigureAwait(false);
                records.AddRange(page.Items);
                continuation = page.ContinuationToken;
            } while (!String.IsNullOrWhiteSpace(continuation));
            return records;
        }

        private async Task<RoleAccessOperationalRecord> ImportIfMissingAsync(
            LagoVista.UserAdmin.Models.DTOs.RoleAccessDTO dto)
        {
            var candidate = RoleAccessOperationalRecord.FromDTO(dto);
            var current = await _store.GetAsync(candidate.OrganizationId, candidate.Id)
                .ConfigureAwait(false);
            if (current != null) return current;
            var created = await _store.TryCreateAsync(candidate).ConfigureAwait(false);
            if (created.Current != null) return created.Current;
            var winner = await _store.GetAsync(candidate.OrganizationId, candidate.Id)
                .ConfigureAwait(false);
            if (winner != null) return winner;
            throw new InvalidOperationException("Could not safely import role access from legacy storage.");
        }

        private async Task EnsureOrgImportedAsync(string orgId)
        {
            if (String.IsNullOrWhiteSpace(orgId))
                throw new ArgumentException("OrganizationId required.", nameof(orgId));
            if (ImportedOrganizations.ContainsKey(orgId)) return;
            var request = new ListRequest { PageIndex = 1, PageSize = 500 };
            for (var index = 0; index < 1000; index++)
            {
                var page = await _legacy.GetOrgPageAsync(orgId, request).ConfigureAwait(false);
                foreach (var dto in page.Model)
                    await ImportIfMissingAsync(dto).ConfigureAwait(false);
                if (!page.HasMoreRecords)
                {
                    ImportedOrganizations.TryAdd(orgId, 1);
                    return;
                }
                if (String.IsNullOrWhiteSpace(page.NextPartitionKey) ||
                    String.IsNullOrWhiteSpace(page.NextRowKey))
                    throw new InvalidOperationException("Legacy role access continuation token incomplete.");
                request = new ListRequest
                {
                    PageIndex = request.PageIndex + 1,
                    PageSize = request.PageSize,
                    NextPartitionKey = page.NextPartitionKey,
                    NextRowKey = page.NextRowKey
                };
            }
            throw new InvalidOperationException("Legacy role access org import exceeded 500000 rows.");
        }

        private async Task EnsurePublicImportedAsync()
        {
            if (_publicImported) return;
            // Public permissions may originate from any legacy organization
            // partition. Import them into one shared Cassandra public partition.
            var old = await _legacy.GetPublicLegacyAsync().ConfigureAwait(false);
            foreach (var dto in old)
                await ImportIfMissingAsync(dto).ConfigureAwait(false);
            _publicImported = true;
        }

        private Task InvalidateAsync(RoleAccessOperationalRecord record)
        {
            if (String.IsNullOrWhiteSpace(record.RoleId)) return Task.CompletedTask;
            return _cache.RemoveAsync(CacheKey(record.RoleId,
                record.IsPublic ? IS_PUBLIC : record.SourceOrganizationId));
        }
    }
}
