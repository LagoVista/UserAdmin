using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Interfaces.Repos;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Users
{
    /// <summary>
    /// Cassandra-backed refresh tokens. Legacy Azure rows are imported lazily,
    /// without overwriting consumed tokens or writing new tokens back to Azure.
    /// </summary>
    public class RefreshTokenRepo : IRefreshTokenRepo
    {
        private readonly IConditionalOperationalDataStore<RefreshTokenOperationalRecord> _store;
        private readonly ILegacyRefreshTokenReader _legacy;

        public RefreshTokenRepo(
            IConditionalOperationalDataStore<RefreshTokenOperationalRecord> store,
            ILegacyRefreshTokenReader legacy)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
        }

        public static void ConfigureStorage(StorageDefinition<RefreshTokenOperationalRecord> definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            // Refresh tokens live seven days. Keep consumed markers a little longer
            // so a legacy row cannot be imported a second time during the cutover.
            definition
                .PartitionBy(token => token.OrganizationId)
                .KeyBy(token => token.Id)
                .RetainFor(TimeSpan.FromDays(9));
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string tokenId, string userId)
        {
            if (String.IsNullOrWhiteSpace(tokenId) || String.IsNullOrWhiteSpace(userId))
                return null;

            var current = await _store.GetAsync(userId, tokenId).ConfigureAwait(false);
            if (current != null)
                return current.Consumed ? null : current.ToToken();

            // During the migration window, preserve sessions issued by the
            // previous LIVE version. IF NOT EXISTS prevents a concurrent import
            // from replacing a consumed record or newly issued token.
            var old = await _legacy.GetAsync(userId, tokenId).ConfigureAwait(false);
            if (old == null)
                return null;

            var imported = await _store.TryCreateAsync(RefreshTokenOperationalRecord.FromToken(old))
                .ConfigureAwait(false);
            var record = imported.Current;
            return record == null || record.Consumed ? null : record.ToToken();
        }

        public async Task SaveRefreshTokenAsync(RefreshToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            var result = await _store.TryCreateAsync(RefreshTokenOperationalRecord.FromToken(token))
                .ConfigureAwait(false);
            if (!result.Applied)
                throw new InvalidOperationException("Refresh-token identity is already in use.");
        }

        /// <summary>
        /// Single-use consume. Exactly one concurrent request can change version 1
        /// to version 2. The tombstone is retained until Cassandra TTL expiry.
        /// </summary>
        public async Task<bool> TryConsumeRefreshTokenAsync(string tokenId, string userId)
        {
            if (String.IsNullOrWhiteSpace(tokenId) || String.IsNullOrWhiteSpace(userId))
                return false;

            var record = await _store.GetAsync(userId, tokenId).ConfigureAwait(false);
            if (record == null)
            {
                // Import a legacy token first, then claim the same Cassandra row.
                if (await GetRefreshTokenAsync(tokenId, userId).ConfigureAwait(false) == null)
                    return false;
                record = await _store.GetAsync(userId, tokenId).ConfigureAwait(false);
            }

            if (record == null || record.Consumed || record.Version <= 0 ||
                String.IsNullOrWhiteSpace(record.ExpiresUtc) ||
                record.ExpiresUtc.ToDateTime() <= DateTime.UtcNow)
                return false;

            var consumed = new RefreshTokenOperationalRecord
            {
                Id = record.Id,
                OrganizationId = record.OrganizationId,
                CreationDate = record.CreationDate,
                AppId = record.AppId,
                AppInstanceId = record.AppInstanceId,
                IssuedUtc = record.IssuedUtc,
                ExpiresUtc = record.ExpiresUtc,
                Consumed = true
            };

            var claim = await _store.TryReplaceAsync(consumed, record.Version).ConfigureAwait(false);
            return claim.Applied;
        }

        public async Task RemoveRefreshTokenAsync(string tokenId, string userId)
        {
            // Idempotent revocation; never throw a 404 for a previously consumed
            // or revoked token.
            await TryConsumeRefreshTokenAsync(tokenId, userId).ConfigureAwait(false);
        }

        public async Task RemoveAllForUserAsync(string userId)
        {
            if (String.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            // Enumerate the user partition, not a global scan.
            string continuation = null;
            do
            {
                var query = new StorageQuery<RefreshTokenOperationalRecord>()
                    .Where(record => record.OrganizationId, StorageFilterOperator.Equal, userId)
                    .WithPage(new StoragePageRequest(100, continuation));

                var page = await _store.QueryAsync(query).ConfigureAwait(false);
                foreach (var record in page.Items)
                    await TryConsumeRefreshTokenAsync(record.Id, userId).ConfigureAwait(false);

                continuation = page.ContinuationToken;
            } while (!String.IsNullOrEmpty(continuation));

            // Revoke historical tokens not yet imported. No Azure writes occur
            // during normal issuance/renewal.
            await _legacy.RemoveAllForUserAsync(userId).ConfigureAwait(false);
        }
    }
}
