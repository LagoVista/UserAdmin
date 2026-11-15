using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.AuthCodes
{
    /// <summary>
    /// One Mongo collection for password recovery and email verification, with
    /// an explicit Kind discriminator. Only hashes, never numeric codes, persist.
    /// </summary>
    public sealed class VerificationCodeRecord : IApplicationDataRecord
    {
        // Both code types have a 10-minute logical lifetime. The 24-hour TTL
        // is a physical cleanup window; a resend's cooldown still uses IssuedUtc.
        public static readonly TimeSpan Retention = TimeSpan.FromDays(1);
        public const string PasswordResetKind = "password-reset";
        public const string EmailVerificationKind = "email-verification";

        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public string Kind { get; set; }
        public string UserId { get; set; }
        public string CodeHash { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? ConsumedUtc { get; set; }
    }

    internal sealed class VerificationCodeStore
    {
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;

        public VerificationCodeStore(IApplicationDataStore store, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task StoreAsync(string kind, string codeId, string userId,
            string codeHash, DateTime issuedUtc, DateTime expiresUtc,
            int attemptCount, DateTime? consumedUtc)
        {
            if (String.IsNullOrWhiteSpace(kind)) throw new ArgumentException("Code kind is required.", nameof(kind));
            if (!NormalizedId32.TryCreate(codeId, out var id))
                throw new ArgumentException("Code Id must be a normalized 32-character identifier.", nameof(codeId));
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required.", nameof(userId));
            if (String.IsNullOrWhiteSpace(codeHash)) throw new ArgumentException("CodeHash is required.", nameof(codeHash));

            var record = new VerificationCodeRecord
            {
                Id = id,
                Organization = RequireOrg(),
                Kind = kind,
                UserId = userId,
                CodeHash = codeHash,
                IssuedUtc = issuedUtc.ToUniversalTime(),
                ExpiresUtc = expiresUtc.ToUniversalTime(),
                AttemptCount = attemptCount,
                ConsumedUtc = consumedUtc?.ToUniversalTime()
            };
            await _store.InsertAsync(record).ConfigureAwait(false);
        }

        public async Task<VersionedApplicationDataRecord<VerificationCodeRecord>> GetLatestAsync(
            string kind, string userId)
        {
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            var org = RequireOrg();
            var query = new StorageQuery<VerificationCodeRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, org.Id)
                .Where(r => r.UserId, StorageFilterOperator.Equal, userId)
                .Where(r => r.Kind, StorageFilterOperator.Equal, kind)
                .OrderBy(r => r.IssuedUtc, StorageSortDirection.Descending)
                .WithPage(new StoragePageRequest(10));
            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            foreach (var candidate in page.Items)
            {
                var read = await _store.GetVersionedAsync<VerificationCodeRecord>(
                    new StorageKey(candidate.Id.Value, org.Id)).ConfigureAwait(false);
                if (read?.Record != null &&
                    String.Equals(read.Record.Kind, kind, StringComparison.Ordinal) &&
                    String.Equals(read.Record.UserId, userId, StringComparison.Ordinal))
                    return read;
            }
            return null;
        }

        public async Task UpdateAsync(VersionedApplicationDataRecord<VerificationCodeRecord> current,
            int attemptCount, DateTime? consumedUtc)
        {
            if (current?.Record == null) throw new InvalidOperationException("Verification code must be versioned before updating.");
            if (attemptCount < current.Record.AttemptCount)
                throw new InvalidOperationException("Verification code attempts cannot decrease.");
            if (current.Record.ConsumedUtc.HasValue && !consumedUtc.HasValue)
                throw new InvalidOperationException("A consumed verification code cannot become unconsumed.");

            current.Record.AttemptCount = attemptCount;
            current.Record.ConsumedUtc = consumedUtc?.ToUniversalTime();
            // A stale updater fails atomically with RecordConflictException,
            // preventing two successful consumptions or lost attempt increments.
            await _store.UpdateIfVersionAsync(current.Record, current.ConcurrencyToken).ConfigureAwait(false);
        }

        public async Task ClearAsync(string kind, string userId)
        {
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            var org = RequireOrg();
            while (true)
            {
                var query = new StorageQuery<VerificationCodeRecord>()
                    .Where(r => r.Organization.Id, StorageFilterOperator.Equal, org.Id)
                    .Where(r => r.UserId, StorageFilterOperator.Equal, userId)
                    .Where(r => r.Kind, StorageFilterOperator.Equal, kind)
                    .WithPage(new StoragePageRequest(100));
                var page = await _store.QueryAsync(query).ConfigureAwait(false);
                if (page.Items.Count == 0) return;
                foreach (var record in page.Items)
                    await _store.DeleteAsync<VerificationCodeRecord>(
                        new StorageKey(record.Id.Value, org.Id)).ConfigureAwait(false);
            }
        }

        private EntityHeader RequireOrg()
        {
            var org = _config.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for verification-code storage.");
            return org;
        }
    }
}
