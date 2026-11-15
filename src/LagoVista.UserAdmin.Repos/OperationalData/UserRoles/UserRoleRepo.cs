using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.Repos.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.OperationalData.UserRoles
{
    /// <summary>
    /// Cassandra-authoritative role assignments. Import old Azure roles with
    /// create-if-absent and preserve revocations as permanent CAS tombstones.
    /// </summary>
    public sealed class UserRoleRepo : IUserRoleRepo
    {
        private readonly IConditionalOperationalDataStore<UserRoleOperationalRecord> _store;
        private readonly ILegacyUserRoleReader _legacy;
        private readonly ICacheProvider _cache;

        public UserRoleRepo(IConditionalOperationalDataStore<UserRoleOperationalRecord> store,
            ILegacyUserRoleReader legacy, ICacheProvider cache)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public static void ConfigureStorage(StorageDefinition<UserRoleOperationalRecord> definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            definition.PartitionBy(r => r.OrganizationId)
                .KeyBy(r => r.Id)
                .Index(r => r.UserId);
            // No TTL: deleted role assignments are retained as security tombstones.
        }

        public async Task AddUserRole(UserRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            var wanted = UserRoleOperationalRecord.FromDTO(role.ToDTO());
            var existing = await _store.GetAsync(wanted.OrganizationId, wanted.Id)
                .ConfigureAwait(false);
            if (existing == null)
            {
                var old = await _legacy.GetLegacyAsync(wanted.OrganizationId, wanted.Id)
                    .ConfigureAwait(false);
                if (old != null)
                    existing = await ImportIfMissingAsync(old).ConfigureAwait(false);
            }

            for (var attempt = 0; attempt < 5; attempt++)
            {
                if (existing == null)
                {
                    var created = await _store.TryCreateAsync(wanted).ConfigureAwait(false);
                    if (created.Applied)
                    {
                        await ClearUserCacheAsync(wanted.UserId, wanted.OrganizationId).ConfigureAwait(false);
                        return;
                    }
                    existing = created.Current ?? await _store.GetAsync(wanted.OrganizationId, wanted.Id)
                        .ConfigureAwait(false);
                }
                if (existing == null) continue;
                if (!existing.IsDeleted)
                    throw new InvalidOperationException("User role assignment already exists.");

                var reactivated = await _store.TryReplaceAsync(wanted, existing.Version)
                    .ConfigureAwait(false);
                if (reactivated.Applied)
                {
                    await ClearUserCacheAsync(wanted.UserId, wanted.OrganizationId).ConfigureAwait(false);
                    return;
                }
                existing = reactivated.Current;
            }
            throw new InvalidOperationException("Concurrent changes prevented adding a role.");
        }

        public async Task<UserRole> GetRoleAssignmentAsync(string userRoleId, string organizationId)
        {
            var current = await _store.GetAsync(organizationId, userRoleId).ConfigureAwait(false);
            if (current != null) return current.IsDeleted ? null : current.ToDTO().ToUserRole();

            var old = await _legacy.GetLegacyAsync(organizationId, userRoleId).ConfigureAwait(false);
            if (old == null) return null;
            var imported = await ImportIfMissingAsync(old).ConfigureAwait(false);
            return imported.IsDeleted ? null : imported.ToDTO().ToUserRole();
        }

        public async Task<List<UserRole>> GetRolesForUserAsync(string userId, string organizationId)
        {
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required.", nameof(userId));
            if (String.IsNullOrWhiteSpace(organizationId))
                throw new ArgumentException("OrganizationId is required.", nameof(organizationId));

            // Revisit Azure on list until explicit backfill/verification completes.
            var historical = await _legacy.GetRolesForUserLegacyAsync(userId, organizationId)
                .ConfigureAwait(false);
            foreach (var old in historical)
                await ImportIfMissingAsync(old).ConfigureAwait(false);

            var records = new List<UserRoleOperationalRecord>();
            string next = null;
            do
            {
                var query = new StorageQuery<UserRoleOperationalRecord>()
                    .Where(r => r.OrganizationId, StorageFilterOperator.Equal, organizationId)
                    .Where(r => r.UserId, StorageFilterOperator.Equal, userId)
                    .WithPage(new StoragePageRequest(500, next));
                var page = await _store.QueryAsync(query).ConfigureAwait(false);
                records.AddRange(page.Items);
                next = page.ContinuationToken;
            } while (!String.IsNullOrWhiteSpace(next));

            return records.Where(r => !r.IsDeleted &&
                    String.Equals(r.UserId, userId, StringComparison.Ordinal))
                .Select(r => r.ToDTO().ToUserRole())
                .OrderBy(r => r.Role.Text).ToList();
        }

        public async Task RemoveUserRole(string userRoleId, string organizationId)
        {
            var record = await _store.GetAsync(organizationId, userRoleId).ConfigureAwait(false);
            if (record == null)
            {
                var old = await _legacy.GetLegacyAsync(organizationId, userRoleId)
                    .ConfigureAwait(false);
                if (old != null)
                    record = await ImportIfMissingAsync(old).ConfigureAwait(false);
            }
            for (var attempt = 0; attempt < 6; attempt++)
            {
                if (record == null)
                {
                    var marker = UserRoleOperationalRecord.Tombstone(organizationId, userRoleId);
                    var created = await _store.TryCreateAsync(marker).ConfigureAwait(false);
                    if (created.Applied) return;
                    record = created.Current;
                }
                if (record == null) continue;
                if (record.IsDeleted) return;

                var markerFromExisting = UserRoleOperationalRecord.Tombstone(organizationId, userRoleId);
                markerFromExisting.UserId = record.UserId;
                markerFromExisting.UserName = record.UserName;
                markerFromExisting.RoleId = record.RoleId;
                markerFromExisting.RoleName = record.RoleName;
                markerFromExisting.RoleKey = record.RoleKey;
                markerFromExisting.OrganizationName = record.OrganizationName;
                markerFromExisting.OriginalCreationDate = record.OriginalCreationDate;
                markerFromExisting.CreatedById = record.CreatedById;
                markerFromExisting.CreatedByName = record.CreatedByName;
                var revoked = await _store.TryReplaceAsync(markerFromExisting, record.Version)
                    .ConfigureAwait(false);
                if (revoked.Applied)
                {
                    await ClearUserCacheAsync(record.UserId, organizationId).ConfigureAwait(false);
                    return;
                }
                record = revoked.Current;
            }
            throw new InvalidOperationException("Concurrent role changes prevented revocation.");
        }

        private async Task<UserRoleOperationalRecord> ImportIfMissingAsync(
            LagoVista.UserAdmin.Models.DTOs.UserRoleDTO dto)
        {
            var candidate = UserRoleOperationalRecord.FromDTO(dto);
            var current = await _store.GetAsync(candidate.OrganizationId, candidate.Id)
                .ConfigureAwait(false);
            if (current != null) return current;
            var created = await _store.TryCreateAsync(candidate).ConfigureAwait(false);
            if (created.Current != null) return created.Current;
            var winner = await _store.GetAsync(candidate.OrganizationId, candidate.Id)
                .ConfigureAwait(false);
            if (winner != null) return winner;
            throw new InvalidOperationException("Could not safely import legacy role assignment.");
        }

        private Task ClearUserCacheAsync(string userId, string organizationId)
            => String.IsNullOrWhiteSpace(userId) ? Task.CompletedTask
                : _cache.RemoveAsync("user_role_cache_" + userId + "_" + organizationId + "_v2");
    }
}
