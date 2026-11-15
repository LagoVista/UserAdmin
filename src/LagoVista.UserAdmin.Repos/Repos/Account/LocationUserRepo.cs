using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.OperationalData.LocationAssociations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Users
{
    /// <summary>
    /// Durable Cassandra location/user XRefs, with a canonical location partition
    /// and a reverse user partition. A reverse hit must resolve its canonical
    /// record, so a stale projection cannot resurrect a revoked membership.
    /// </summary>
    public sealed class LocationUserRepo : ILocationUserRepo
    {
        private readonly IOperationalDataStore<LocationUserOperationalRecord> _store;

        public LocationUserRepo(IOperationalDataStore<LocationUserOperationalRecord> store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task AddUserToLocationAsync(LocationUser locationUser)
        {
            var primary = LocationUserOperationalRecord.ForLocation(locationUser);
            var reverse = LocationUserOperationalRecord.ForUser(locationUser);
            // Canonical first; a failed reverse projection fails closed on user lookup.
            await _store.UpsertAsync(primary).ConfigureAwait(false);
            await _store.UpsertAsync(reverse).ConfigureAwait(false);
        }

        public async Task<IEnumerable<LocationUser>> GetUsersForLocationAsync(string locationId)
        {
            var partition = LocationUserOperationalRecord.LocationPartition(locationId);
            var result = await ReadPartitionAsync(partition).ConfigureAwait(false);
            return result.Where(record =>
                    String.Equals(record.LocationId, locationId, StringComparison.Ordinal) &&
                    String.Equals(record.OrganizationId, partition, StringComparison.Ordinal))
                .Select(record => record.ToModel()).ToList();
        }

        public async Task<IEnumerable<LocationUser>> GetLocationsForUserAsync(string userId)
        {
            var partition = LocationUserOperationalRecord.UserPartition(userId);
            var reverse = await ReadPartitionAsync(partition).ConfigureAwait(false);
            var verified = new List<LocationUser>();
            foreach (var projection in reverse.Where(record =>
                String.Equals(record.UserId, userId, StringComparison.Ordinal)))
            {
                var canonical = await _store.GetAsync(
                    LocationUserOperationalRecord.LocationPartition(projection.LocationId),
                    LocationUserOperationalRecord.AssociationId(projection.LocationId, userId))
                    .ConfigureAwait(false);
                if (canonical != null &&
                    String.Equals(canonical.UserId, userId, StringComparison.Ordinal) &&
                    String.Equals(canonical.LocationId, projection.LocationId, StringComparison.Ordinal) &&
                    String.Equals(canonical.TenantOrganizationId, projection.TenantOrganizationId, StringComparison.Ordinal))
                    verified.Add(canonical.ToModel());
            }
            return verified;
        }

        public async Task RemoveUserFromLocationAsync(
            string locationId, string userId, EntityHeader removedBy)
        {
            var associationId = LocationUserOperationalRecord.AssociationId(locationId, userId);
            // Delete authoritative membership first. A failed second delete leaves
            // only a harmless reverse entry, filtered by GetLocationsForUserAsync.
            await _store.DeleteAsync(LocationUserOperationalRecord.LocationPartition(locationId),
                associationId).ConfigureAwait(false);
            await _store.DeleteAsync(LocationUserOperationalRecord.UserPartition(userId),
                associationId).ConfigureAwait(false);
        }

        private async Task<List<LocationUserOperationalRecord>> ReadPartitionAsync(string partition)
        {
            var result = new List<LocationUserOperationalRecord>();
            string continuation = null;
            do
            {
                var page = await _store.QueryAsync(new StorageQuery<LocationUserOperationalRecord>()
                    .Where(r => r.OrganizationId, StorageFilterOperator.Equal, partition)
                    .WithPage(new StoragePageRequest(500, continuation))).ConfigureAwait(false);
                result.AddRange(page.Items);
                continuation = page.ContinuationToken;
            } while (!String.IsNullOrWhiteSpace(continuation));
            return result;
        }
    }
}
