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
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.AnonymousVisitors
{
    /// <summary>
    /// One Mongo ApplicationData document per anonymous visitor. Lookup and lifecycle
    /// fields are indexed inside the document, not mirrored into Azure tables.
    /// </summary>
    public sealed class AnonymousVisitorRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public AnonymousVisitor Visitor { get; set; }
        public string StateKey { get; set; }
        public DateTime LifecycleDueUtc { get; set; }

        public static AnonymousVisitorRecord Create(AnonymousVisitor visitor, EntityHeader organization)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            if (organization == null || String.IsNullOrWhiteSpace(organization.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for anonymous visitor storage.");
            if (!NormalizedId32.TryCreate(visitor.ActorId, out var id))
                throw new ArgumentException("A normalized 32-character ActorId is required.", nameof(visitor));

            return new AnonymousVisitorRecord
            {
                Id = id,
                Organization = organization,
                CreationDate = UtcTimestamp.Now,
                LastUpdatedDate = UtcTimestamp.Now,
                Visitor = visitor,
                StateKey = visitor.State.ToString(),
                LifecycleDueUtc = GetLifecycleDueUtc(visitor)
            };
        }

        public static DateTime GetLifecycleDueUtc(AnonymousVisitor visitor)
        {
            if (visitor.State == AnonymousVisitorState.Promoted)
                return (visitor.PromotedUtc ?? visitor.StateChangedUtc).ToUniversalTime();
            if (visitor.State == AnonymousVisitorState.Expired)
                return (visitor.ExpiredUtc ?? visitor.StateChangedUtc).ToUniversalTime();
            return visitor.ExpiresUtc.ToUniversalTime();
        }
    }

    /// <summary>
    /// Keeps IAnonymousVisitorRepo stable for bootstrap, restore, promotion and reset.
    /// Updates use the same provider-owned compare-and-swap contract as VisitorConversation.
    /// </summary>
    public sealed class AnonymousVisitorRepo : IAnonymousVisitorRepo
    {
        // VisitorConversation has a rolling 180-day ExpiresUtc horizon.
        // Physical Mongo TTL here is also 180 days, measured from the last write.
        public static readonly TimeSpan Retention = TimeSpan.FromDays(180);

        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;
        private readonly ConditionalWeakTable<AnonymousVisitor, ApplicationDataConcurrencyToken> _versions =
            new ConditionalWeakTable<AnonymousVisitor, ApplicationDataConcurrencyToken>();

        public AnonymousVisitorRepo(IApplicationDataStore store, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task CreateAsync(AnonymousVisitor visitor)
        {
            Validate(visitor);
            await RejectTokenCollisionAsync(visitor).ConfigureAwait(false);
            var record = AnonymousVisitorRecord.Create(visitor, RequireOrg());
            await _store.InsertAsync(record).ConfigureAwait(false);

            var read = await _store.GetVersionedAsync<AnonymousVisitorRecord>(
                new StorageKey(record.Id.Value, record.Organization.Id)).ConfigureAwait(false);
            if (read?.ConcurrencyToken == null)
                throw new InvalidOperationException("Anonymous visitor created without a retrievable ApplicationData version.");
            Track(visitor, read.ConcurrencyToken);
        }

        public async Task<AnonymousVisitor> GetByActorIdAsync(string actorId)
        {
            if (String.IsNullOrWhiteSpace(actorId)) throw new ArgumentNullException(nameof(actorId));
            var record = await LoadAsync(actorId).ConfigureAwait(false);
            return record?.Visitor;
        }

        public Task<AnonymousVisitor> FindByContinuityTokenHashAsync(string continuityTokenHash)
            => FindAsync(continuityTokenHash, true);

        public Task<AnonymousVisitor> FindByInstallationIdHashAsync(string installationIdHash)
            => FindAsync(installationIdHash, false);

        private async Task<AnonymousVisitor> FindAsync(string hash, bool continuity)
        {
            if (String.IsNullOrWhiteSpace(hash)) throw new ArgumentNullException(nameof(hash));
            var query = new StorageQuery<AnonymousVisitorRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, RequireOrg().Id)
                .WithPage(new StoragePageRequest(3));
            query = continuity
                ? query.Where(r => r.Visitor.ContinuityTokenHash, StorageFilterOperator.Equal, hash)
                : query.Where(r => r.Visitor.InstallationIdHash, StorageFilterOperator.Equal, hash);

            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            var actors = page.Items.Where(r => r?.Visitor != null)
                .Select(r => r.Visitor.ActorId).Distinct(StringComparer.Ordinal).ToList();
            // Never pick an arbitrary identity if two rows share a credential hash.
            if (actors.Count > 1)
                throw new InvalidOperationException("Anonymous visitor credential lookup is ambiguous.");
            if (actors.Count == 0) return null;

            var visitor = await GetByActorIdAsync(actors[0]).ConfigureAwait(false);
            var actualHash = continuity ? visitor?.ContinuityTokenHash : visitor?.InstallationIdHash;
            return String.Equals(actualHash, hash, StringComparison.Ordinal) ? visitor : null;
        }

        public async Task<IEnumerable<AnonymousVisitor>> GetByStateAsync(
            AnonymousVisitorState state, DateTime? dueBeforeUtc = null, int take = 100)
        {
            if (take <= 0) return Enumerable.Empty<AnonymousVisitor>();
            var query = new StorageQuery<AnonymousVisitorRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, RequireOrg().Id)
                .Where(r => r.StateKey, StorageFilterOperator.Equal, state.ToString())
                .OrderBy(r => r.LifecycleDueUtc, StorageSortDirection.Ascending)
                .WithPage(new StoragePageRequest(take));
            if (dueBeforeUtc.HasValue)
                query = query.Where(r => r.LifecycleDueUtc, StorageFilterOperator.LessThanOrEqual,
                    dueBeforeUtc.Value.ToUniversalTime());

            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            var current = await Task.WhenAll(page.Items.Where(r => r?.Visitor != null)
                .Select(r => GetByActorIdAsync(r.Visitor.ActorId))).ConfigureAwait(false);
            return current.Where(v => v != null && v.State == state &&
                (!dueBeforeUtc.HasValue ||
                 AnonymousVisitorRecord.GetLifecycleDueUtc(v) <= dueBeforeUtc.Value.ToUniversalTime()))
                .Take(take).ToList();
        }

        public async Task UpdateAsync(AnonymousVisitor visitor)
        {
            Validate(visitor);
            if (!_versions.TryGetValue(visitor, out var version))
                throw new InvalidOperationException(
                    "Anonymous visitor must be loaded through the versioned repository before updating.");

            await RejectTokenCollisionAsync(visitor).ConfigureAwait(false);
            var record = AnonymousVisitorRecord.Create(visitor, RequireOrg());
            var next = await _store.UpdateIfVersionAsync(record, version).ConfigureAwait(false);
            Track(visitor, next);
        }

        public Task DeleteAsync(string actorId)
        {
            if (String.IsNullOrWhiteSpace(actorId)) throw new ArgumentNullException(nameof(actorId));
            return _store.DeleteAsync<AnonymousVisitorRecord>(
                new StorageKey(actorId, RequireOrg().Id));
        }

        private async Task RejectTokenCollisionAsync(AnonymousVisitor visitor)
        {
            if (!String.IsNullOrWhiteSpace(visitor.ContinuityTokenHash))
            {
                var existing = await FindByContinuityTokenHashAsync(visitor.ContinuityTokenHash).ConfigureAwait(false);
                if (existing != null && !String.Equals(existing.ActorId, visitor.ActorId, StringComparison.Ordinal))
                    throw new InvalidOperationException("Continuity token is already assigned to another visitor.");
            }

            if (!String.IsNullOrWhiteSpace(visitor.InstallationIdHash))
            {
                var existing = await FindByInstallationIdHashAsync(visitor.InstallationIdHash).ConfigureAwait(false);
                if (existing != null && !String.Equals(existing.ActorId, visitor.ActorId, StringComparison.Ordinal))
                    throw new InvalidOperationException("Installation identifier is already assigned to another visitor.");
            }
        }

        private async Task<AnonymousVisitorRecord> LoadAsync(string actorId)
        {
            var read = await _store.GetVersionedAsync<AnonymousVisitorRecord>(
                new StorageKey(actorId, RequireOrg().Id)).ConfigureAwait(false);
            if (read?.Record?.Visitor == null) return null;
            if (!String.Equals(read.Record.Visitor.ActorId, actorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Anonymous visitor document identity mismatch.");
            Track(read.Record.Visitor, read.ConcurrencyToken);
            return read.Record;
        }

        private void Track(AnonymousVisitor visitor, ApplicationDataConcurrencyToken version)
        {
            if (version == null) throw new InvalidOperationException("ApplicationData did not return a visitor version.");
            _versions.Remove(visitor);
            _versions.Add(visitor, version);
        }

        private EntityHeader RequireOrg()
        {
            var org = _config.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for anonymous visitor storage.");
            return org;
        }

        private static void Validate(AnonymousVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            if (String.IsNullOrWhiteSpace(visitor.ActorId))
                throw new ArgumentException("ActorId is required.", nameof(visitor));
            if (visitor.State == AnonymousVisitorState.Active &&
                String.IsNullOrWhiteSpace(visitor.ContinuityTokenHash) &&
                String.IsNullOrWhiteSpace(visitor.InstallationIdHash))
                throw new ArgumentException("Active visitors require a continuity token or installation identifier.", nameof(visitor));
            if ((visitor.BootstrapContext?.Length ?? 0) > AnonymousVisitor.MaximumBootstrapContextLength)
                throw new ArgumentException("BootstrapContext is too long.", nameof(visitor));
            if (visitor.CreatedUtc == default(DateTime) || visitor.LastActivityUtc == default(DateTime) ||
                visitor.ExpiresUtc == default(DateTime) || visitor.StateChangedUtc == default(DateTime))
                throw new ArgumentException("Visitor lifecycle dates are required.", nameof(visitor));
            if (visitor.ExpiresUtc.ToUniversalTime() <= visitor.CreatedUtc.ToUniversalTime())
                throw new ArgumentException("ExpiresUtc must be later than CreatedUtc.", nameof(visitor));
            if (visitor.State == AnonymousVisitorState.Promoted &&
                (String.IsNullOrWhiteSpace(visitor.ProvisionalEnvironmentId) || !visitor.PromotedUtc.HasValue))
                throw new ArgumentException("Promoted visitors require a provisional environment and timestamp.", nameof(visitor));
            if (visitor.State == AnonymousVisitorState.Expired && !visitor.ExpiredUtc.HasValue)
                throw new ArgumentException("Expired visitors require ExpiredUtc.", nameof(visitor));
        }
    }
}
