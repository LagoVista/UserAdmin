using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Repos.OperationalData.LocationAssociations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Security
{
    /// <summary>
    /// Persistent Cassandra XRefs for location/user/role. SAI indexes support
    /// within-location user and role lookups; revocation uses versioned deletes.
    /// </summary>
    public sealed class LocationRoleRepo : ILocationRoleRepo
    {
        private readonly IConditionalOperationalDataStore<LocationRoleOperationalRecord> _store;

        public LocationRoleRepo(IConditionalOperationalDataStore<LocationRoleOperationalRecord> store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public static void ConfigureStorage(StorageDefinition<LocationRoleOperationalRecord> definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            definition.PartitionBy(record => record.OrganizationId)
                .KeyBy(record => record.Id)
                .Index(record => record.UserId)
                .Index(record => record.RoleId);
            // Intentionally no retention policy: a role remains until revoked.
        }

        public async Task AddRoleForUserAsync(LocationUserRole locationUserRole)
        {
            var record = LocationRoleOperationalRecord.FromModel(locationUserRole);
            var created = await _store.TryCreateAsync(record).ConfigureAwait(false);
            if (!created.Applied &&
                (created.Current == null ||
                 !String.Equals(created.Current.UserId, record.UserId, StringComparison.Ordinal) ||
                 !String.Equals(created.Current.RoleId, record.RoleId, StringComparison.Ordinal) ||
                 !String.Equals(created.Current.LocationId, record.LocationId, StringComparison.Ordinal)))
                throw new InvalidOperationException("Location role assignment already exists with conflicting identity.");
        }

        public async Task<bool> ConfirmUserInRoleAsync(string locationId, string userId, string roleId)
        {
            var record = await _store.GetAsync(
                LocationUserOperationalRecord.Require(locationId, nameof(locationId)),
                LocationRoleOperationalRecord.AssociationId(userId, roleId)).ConfigureAwait(false);
            return record != null && String.Equals(record.LocationId, locationId, StringComparison.Ordinal) &&
                String.Equals(record.UserId, userId, StringComparison.Ordinal) &&
                String.Equals(record.RoleId, roleId, StringComparison.Ordinal);
        }

        public async Task<IEnumerable<LocationUserRole>> GetUsersInRoleForLocationAsync(string roleId, string locationId)
        {
            var filter = ForLocation(locationId)
                .Where(r => r.RoleId, StorageFilterOperator.Equal,
                    LocationUserOperationalRecord.Require(roleId, nameof(roleId)));
            var records = await ReadAllAsync(filter).ConfigureAwait(false);
            return records.Where(record => String.Equals(record.LocationId, locationId, StringComparison.Ordinal) &&
                String.Equals(record.RoleId, roleId, StringComparison.Ordinal))
                .Select(record => record.ToModel()).ToList();
        }

        public async Task<IEnumerable<LocationUserRole>> GetRolesForUserInLocationAsync(string locationId, string userId)
        {
            var filter = ForLocation(locationId)
                .Where(r => r.UserId, StorageFilterOperator.Equal,
                    LocationUserOperationalRecord.Require(userId, nameof(userId)));
            var records = await ReadAllAsync(filter).ConfigureAwait(false);
            return records.Where(record => String.Equals(record.LocationId, locationId, StringComparison.Ordinal) &&
                String.Equals(record.UserId, userId, StringComparison.Ordinal))
                .Select(record => record.ToModel()).ToList();
        }

        public Task RevokeRoleForUserInLocationAsync(string locationId, string userId, string roleId)
            => DeleteAssignmentAsync(
                LocationUserOperationalRecord.Require(locationId, nameof(locationId)),
                LocationRoleOperationalRecord.AssociationId(userId, roleId));

        public async Task RevokeAllRolesForUserInLocationAsync(string locationId, string userId)
        {
            var filter = ForLocation(locationId)
                .Where(r => r.UserId, StorageFilterOperator.Equal,
                    LocationUserOperationalRecord.Require(userId, nameof(userId)));
            // Materialize before deleting; mutation invalidates Cassandra page state.
            var assignments = await ReadAllAsync(filter).ConfigureAwait(false);
            foreach (var record in assignments.Where(record =>
                String.Equals(record.LocationId, locationId, StringComparison.Ordinal) &&
                String.Equals(record.UserId, userId, StringComparison.Ordinal)))
                await DeleteAssignmentAsync(locationId, record.Id).ConfigureAwait(false);
        }

        private async Task DeleteAssignmentAsync(string locationId, string key)
        {
            for (var attempt = 0; attempt < 4; attempt++)
            {
                var current = await _store.GetAsync(locationId, key).ConfigureAwait(false);
                if (current == null) return;

                var result = await _store.TryDeleteAsync(locationId, key, current.Version)
                    .ConfigureAwait(false);
                if (result.Applied) return;
            }
            throw new InvalidOperationException("Concurrent role changes prevented revocation.");
        }

        private static StorageQuery<LocationRoleOperationalRecord> ForLocation(string locationId)
            => new StorageQuery<LocationRoleOperationalRecord>()
                .Where(r => r.OrganizationId, StorageFilterOperator.Equal,
                    LocationUserOperationalRecord.Require(locationId, nameof(locationId)));

        private async Task<List<LocationRoleOperationalRecord>> ReadAllAsync(
            StorageQuery<LocationRoleOperationalRecord> query)
        {
            var records = new List<LocationRoleOperationalRecord>();
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
    }
}
