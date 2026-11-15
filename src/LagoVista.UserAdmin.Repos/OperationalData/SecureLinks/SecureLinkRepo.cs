using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Repos.Security;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.OperationalData.SecureLinks
{
    /// <summary>
    /// Cassandra conditional updates prevent a stale access-count update from
    /// reverting a concurrent revocation. Existing valid Table links are imported
    /// lazily; new writes never go to Table Storage.
    /// </summary>
    public sealed class SecureLinkRepo : ISecureLinkRepo
    {
        // Bounded physical retention. The expires field is enforced on EVERY read.
        public static readonly TimeSpan Retention = TimeSpan.FromDays(365);
        private readonly IConditionalOperationalDataStore<SecureLinkOperationalRecord> _store;
        private readonly LegacySecureLinkReader _legacy;

        public SecureLinkRepo(IConditionalOperationalDataStore<SecureLinkOperationalRecord> store,
            LegacySecureLinkReader legacy)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
        }

        public static void ConfigureStorage(StorageDefinition<SecureLinkOperationalRecord> definition)
        {
            definition.PartitionBy(r => r.OrganizationId).KeyBy(r => r.Id)
                .RetainFor(Retention);
        }

        public async Task AddSecureLinkAsync(SecureLink link)
        {
            if (!IsEligible(link, DateTime.UtcNow) ||
                link.Expires.ToDateTime() - DateTime.UtcNow > Retention)
                throw new InvalidOperationException("Secure link must have a valid expiration within 365 days.");

            var create = await _store.TryCreateAsync(SecureLinkOperationalRecord.FromModel(link))
                .ConfigureAwait(false);
            if (!create.Applied)
                throw new InvalidOperationException("Secure link identity is already in use.");
        }

        public async Task<SecureLink> GetSecureLinkAsync(string orgId, string linkId)
        {
            if (String.IsNullOrWhiteSpace(orgId) || String.IsNullOrWhiteSpace(linkId))
                return null;

            var record = await _store.GetAsync(orgId, linkId).ConfigureAwait(false);
            if (record == null)
            {
                // Temporary compatibility path for links generated before cutover.
                var previous = await _legacy.GetLegacyAsync(orgId, linkId).ConfigureAwait(false);
                if (!IsEligible(previous, DateTime.UtcNow))
                    return null;
                var imported = await _store.TryCreateAsync(
                    SecureLinkOperationalRecord.FromModel(previous)).ConfigureAwait(false);
                record = imported.Current;
            }
            return record != null && String.Equals(record.OrganizationId, orgId, StringComparison.Ordinal) &&
                IsEligible(record.ToModel(), DateTime.UtcNow) ? record.ToModel() : null;
        }

        public async Task UpdateSecureLinkAsync(SecureLink link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));
            var current = await _store.GetAsync(link.PartitionKey, link.RowKey).ConfigureAwait(false);
            if (current == null || !IsEligible(current.ToModel(), DateTime.UtcNow) || !IsEligible(link, DateTime.UtcNow))
                throw new InvalidOperationException("Secure link is missing, expired or revoked.");

            var replacement = SecureLinkOperationalRecord.FromModel(link);
            if (replacement.AccessCount < current.AccessCount)
                throw new InvalidOperationException("Secure-link access count cannot decrease.");
            if (!String.Equals(replacement.Expires, current.Expires, StringComparison.Ordinal) ||
                !String.Equals(replacement.DestinationLink, current.DestinationLink, StringComparison.Ordinal) ||
                !String.Equals(replacement.ForUserId, current.ForUserId, StringComparison.Ordinal))
                throw new InvalidOperationException("Secure-link security attributes cannot be changed by an access update.");

            var updated = await _store.TryReplaceAsync(replacement, current.Version).ConfigureAwait(false);
            if (!updated.Applied)
                throw new InvalidOperationException("Secure link was changed concurrently; retry the access.");
        }

        public async Task RevokeLinkAsync(string orgId, string linkId)
        {
            for (var attempt = 0; attempt < 4; attempt++)
            {
                var current = await _store.GetAsync(orgId, linkId).ConfigureAwait(false);
                if (current == null)
                {
                    var original = await _legacy.GetLegacyAsync(orgId, linkId).ConfigureAwait(false);
                    if (original == null) return;
                    var imported = await _store.TryCreateAsync(
                        SecureLinkOperationalRecord.FromModel(original)).ConfigureAwait(false);
                    current = imported.Current;
                }
                if (current == null || current.Expired) return;

                var replacement = current.ToModel();
                replacement.Expired = true;
                var change = await _store.TryReplaceAsync(
                    SecureLinkOperationalRecord.FromModel(replacement), current.Version).ConfigureAwait(false);
                if (change.Applied) return;
            }
            throw new InvalidOperationException("Concurrent secure-link updates prevented revocation.");
        }

        public static bool IsEligible(SecureLink link, DateTime nowUtc)
        {
            if (link == null || link.Expired || String.IsNullOrWhiteSpace(link.Expires) ||
                String.IsNullOrWhiteSpace(link.OrgId) || String.IsNullOrWhiteSpace(link.PartitionKey) ||
                !String.Equals(link.OrgId, link.PartitionKey, StringComparison.Ordinal))
                return false;
            try { return link.Expires.ToDateTime().ToUniversalTime() > nowUtc.ToUniversalTime(); }
            catch { return false; }
        }
    }
}
