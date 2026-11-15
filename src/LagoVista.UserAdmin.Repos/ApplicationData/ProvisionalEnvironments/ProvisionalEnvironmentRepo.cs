using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.ProvisionalEnvironments
{
    /// <summary>
    /// One ApplicationData document per provisional environment. This record is durable
    /// until the manager completes billing archival, accounting, and organization purge.
    /// </summary>
    public sealed class ProvisionalEnvironmentRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public ProvisionalEnvironment Environment { get; set; }
        public string StateKey { get; set; }
        public DateTime LifecycleDueUtc { get; set; }

        // The Mongo _id uniqueness constraint replaces the old creation-request table.
        public static string IdFromCreationRequest(string creationRequestId)
        {
            if (String.IsNullOrWhiteSpace(creationRequestId))
                throw new ArgumentException("CreationRequestId is required.", nameof(creationRequestId));
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(
                    "app-trex-provisional-environment:v1:" + creationRequestId));
                return BitConverter.ToString(bytes, 0, 16).Replace("-", String.Empty);
            }
        }

        public static ProvisionalEnvironmentRecord FromEnvironment(
            ProvisionalEnvironment environment, EntityHeader organization)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (organization == null || String.IsNullOrWhiteSpace(organization.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for provisional environment storage.");
            if (!NormalizedId32.TryCreate(environment.Id, out var id))
                throw new ArgumentException("Provisional environment Id must be a normalized 32-character identifier.", nameof(environment));

            return new ProvisionalEnvironmentRecord
            {
                Id = id,
                Organization = organization,
                CreationDate = UtcTimestamp.Now,
                LastUpdatedDate = UtcTimestamp.Now,
                Environment = environment,
                StateKey = environment.State.ToString(),
                LifecycleDueUtc = GetLifecycleDueUtc(environment)
            };
        }

        public static DateTime GetLifecycleDueUtc(ProvisionalEnvironment environment)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (environment.State == ProvisionalEnvironmentState.Expired ||
                environment.State == ProvisionalEnvironmentState.PurgePending)
                return environment.PurgeAfterUtc?.ToUniversalTime() ?? DateTime.MaxValue;
            return environment.ExpiresUtc.ToUniversalTime();
        }
    }

    /// <summary>
    /// Single-document replacement for the former Azure Table entity and four
    /// companion index repositories, retaining the IProvisionalEnvironmentRepo API.
    /// </summary>
    public sealed class ProvisionalEnvironmentRepo : IProvisionalEnvironmentRepo
    {
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;
        private readonly ConditionalWeakTable<ProvisionalEnvironment, ApplicationDataConcurrencyToken> _versions =
            new ConditionalWeakTable<ProvisionalEnvironment, ApplicationDataConcurrencyToken>();

        public ProvisionalEnvironmentRepo(IApplicationDataStore store, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task CreateAsync(ProvisionalEnvironment environment)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            // The application-generated random Id is replaced before persistence.
            // Concurrent requests with the same idempotency key now collide atomically.
            environment.Id = ProvisionalEnvironmentRecord.IdFromCreationRequest(environment.CreationRequestId);
            await CheckCredentialConflictsAsync(environment).ConfigureAwait(false);
            var record = ProvisionalEnvironmentRecord.FromEnvironment(environment, RequireOrg());
            await _store.InsertAsync(record).ConfigureAwait(false);

            var read = await _store.GetVersionedAsync<ProvisionalEnvironmentRecord>(
                new StorageKey(record.Id.Value, record.Organization.Id)).ConfigureAwait(false);
            if (read?.ConcurrencyToken == null)
                throw new InvalidOperationException("Provisional environment was inserted without a version.");
            Track(environment, read.ConcurrencyToken);
        }

        public async Task<ProvisionalEnvironment> GetByIdAsync(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            return (await LoadAsync(id).ConfigureAwait(false))?.Environment;
        }

        public async Task<ProvisionalEnvironment> FindByCreationRequestIdAsync(string creationRequestId)
        {
            var id = ProvisionalEnvironmentRecord.IdFromCreationRequest(creationRequestId);
            var environment = await GetByIdAsync(id).ConfigureAwait(false);
            return String.Equals(environment?.CreationRequestId, creationRequestId, StringComparison.Ordinal)
                ? environment : null;
        }

        public Task<ProvisionalEnvironment> FindByRecoveryTokenHashAsync(string recoveryTokenHash)
            => FindByCredentialAsync(recoveryTokenHash, true);

        public Task<ProvisionalEnvironment> FindByInstallationIdHashAsync(string installationIdHash)
            => FindByCredentialAsync(installationIdHash, false);

        private async Task<ProvisionalEnvironment> FindByCredentialAsync(string hash, bool recovery)
        {
            if (String.IsNullOrWhiteSpace(hash)) throw new ArgumentNullException(nameof(hash));
            var query = new StorageQuery<ProvisionalEnvironmentRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, RequireOrg().Id)
                .WithPage(new StoragePageRequest(3));
            query = recovery
                ? query.Where(r => r.Environment.RecoveryTokenHash, StorageFilterOperator.Equal, hash)
                : query.Where(r => r.Environment.InstallationIdHash, StorageFilterOperator.Equal, hash);

            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            var ids = page.Items.Where(r => r?.Environment != null)
                .Select(r => r.Environment.Id).Distinct(StringComparer.Ordinal).ToList();
            if (ids.Count > 1)
                throw new InvalidOperationException("Provisional environment credential lookup is ambiguous.");
            if (ids.Count == 0) return null;
            var environment = await GetByIdAsync(ids[0]).ConfigureAwait(false);
            return String.Equals(recovery ? environment?.RecoveryTokenHash : environment?.InstallationIdHash,
                hash, StringComparison.Ordinal) ? environment : null;
        }

        public async Task<IEnumerable<ProvisionalEnvironment>> GetByStateAsync(
            ProvisionalEnvironmentState state, DateTime? expiresBeforeUtc = null, int take = 100)
        {
            if (take <= 0) return Enumerable.Empty<ProvisionalEnvironment>();
            var query = new StorageQuery<ProvisionalEnvironmentRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, RequireOrg().Id)
                .Where(r => r.StateKey, StorageFilterOperator.Equal, state.ToString())
                .OrderBy(r => r.LifecycleDueUtc, StorageSortDirection.Ascending)
                .WithPage(new StoragePageRequest(take));
            if (expiresBeforeUtc.HasValue)
                query = query.Where(r => r.LifecycleDueUtc, StorageFilterOperator.LessThanOrEqual,
                    expiresBeforeUtc.Value.ToUniversalTime());

            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            var current = await Task.WhenAll(page.Items.Where(r => r?.Environment != null)
                .Select(r => GetByIdAsync(r.Environment.Id))).ConfigureAwait(false);
            return current.Where(e => e != null && e.State == state &&
                (!expiresBeforeUtc.HasValue ||
                 ProvisionalEnvironmentRecord.GetLifecycleDueUtc(e) <= expiresBeforeUtc.Value.ToUniversalTime()))
                .Take(take).ToList();
        }

        public async Task UpdateAsync(ProvisionalEnvironment environment)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (!_versions.TryGetValue(environment, out var version))
                throw new InvalidOperationException(
                    "Provisional environment must be versioned by a repository read before update.");
            if (!String.Equals(environment.Id,
                    ProvisionalEnvironmentRecord.IdFromCreationRequest(environment.CreationRequestId),
                    StringComparison.Ordinal))
                throw new InvalidOperationException("CreationRequestId and provisional environment identity are immutable.");

            await CheckCredentialConflictsAsync(environment).ConfigureAwait(false);
            var record = ProvisionalEnvironmentRecord.FromEnvironment(environment, RequireOrg());
            var nextVersion = await _store.UpdateIfVersionAsync(record, version).ConfigureAwait(false);
            Track(environment, nextVersion);
        }

        public Task DeleteAsync(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            // The manager invokes this after the archive/accounting/purge succeeds.
            return _store.DeleteAsync<ProvisionalEnvironmentRecord>(
                new StorageKey(id, RequireOrg().Id));
        }

        private async Task CheckCredentialConflictsAsync(ProvisionalEnvironment environment)
        {
            if (!String.IsNullOrWhiteSpace(environment.RecoveryTokenHash))
            {
                var match = await FindByRecoveryTokenHashAsync(environment.RecoveryTokenHash).ConfigureAwait(false);
                if (match != null && !String.Equals(match.Id, environment.Id, StringComparison.Ordinal))
                    throw new InvalidOperationException("Recovery token belongs to a different provisional environment.");
            }
            if (!String.IsNullOrWhiteSpace(environment.InstallationIdHash))
            {
                var match = await FindByInstallationIdHashAsync(environment.InstallationIdHash).ConfigureAwait(false);
                if (match != null && !String.Equals(match.Id, environment.Id, StringComparison.Ordinal))
                    throw new InvalidOperationException("Installation identifier belongs to a different provisional environment.");
            }
        }

        private async Task<ProvisionalEnvironmentRecord> LoadAsync(string id)
        {
            var read = await _store.GetVersionedAsync<ProvisionalEnvironmentRecord>(
                new StorageKey(id, RequireOrg().Id)).ConfigureAwait(false);
            if (read?.Record?.Environment == null) return null;
            if (!String.Equals(read.Record.Environment.Id, id, StringComparison.Ordinal))
                throw new InvalidOperationException("Provisional environment storage identity mismatch.");
            Track(read.Record.Environment, read.ConcurrencyToken);
            return read.Record;
        }

        private void Track(ProvisionalEnvironment environment, ApplicationDataConcurrencyToken token)
        {
            if (token == null) throw new InvalidOperationException("ApplicationData did not return a version.");
            _versions.Remove(environment);
            _versions.Add(environment, token);
        }

        private EntityHeader RequireOrg()
        {
            var org = _config.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for provisional environment storage.");
            return org;
        }
    }
}
