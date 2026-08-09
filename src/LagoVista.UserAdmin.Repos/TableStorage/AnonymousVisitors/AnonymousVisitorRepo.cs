using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal class AnonymousVisitorRepo : IAnonymousVisitorRepo
    {
        private readonly IAnonymousVisitorEntityRepo _visitorRepo;
        private readonly IAnonymousVisitorContinuityIndexRepo _continuityIndexRepo;
        private readonly IAnonymousVisitorInstallationIndexRepo _installationIndexRepo;
        private readonly IAnonymousVisitorStateIndexRepo _stateIndexRepo;

        public AnonymousVisitorRepo(IAnonymousVisitorEntityRepo visitorRepo, IAnonymousVisitorContinuityIndexRepo continuityIndexRepo, IAnonymousVisitorInstallationIndexRepo installationIndexRepo, IAnonymousVisitorStateIndexRepo stateIndexRepo)
        {
            _visitorRepo = visitorRepo ?? throw new ArgumentNullException(nameof(visitorRepo));
            _continuityIndexRepo = continuityIndexRepo ?? throw new ArgumentNullException(nameof(continuityIndexRepo));
            _installationIndexRepo = installationIndexRepo ?? throw new ArgumentNullException(nameof(installationIndexRepo));
            _stateIndexRepo = stateIndexRepo ?? throw new ArgumentNullException(nameof(stateIndexRepo));
        }

        public async Task CreateAsync(AnonymousVisitor visitor)
        {
            Validate(visitor);

            var continuityIndexInserted = false;
            var installationIndexInserted = false;
            var stateIndexInserted = false;

            await _visitorRepo.InsertAsync(visitor);

            try
            {
                if (!String.IsNullOrEmpty(visitor.ContinuityTokenHash)) continuityIndexInserted = await EnsureLookupAsync(_continuityIndexRepo, visitor.ContinuityTokenHash, visitor);
                if (!String.IsNullOrEmpty(visitor.InstallationIdHash)) installationIndexInserted = await EnsureLookupAsync(_installationIndexRepo, visitor.InstallationIdHash, visitor);
                stateIndexInserted = await EnsureStateIndexAsync(visitor);

                var persisted = await _visitorRepo.GetByActorIdAsync(visitor.ActorId);
                if (persisted != null) visitor.ETag = persisted.ETag;
            }
            catch
            {
                if (stateIndexInserted) await TryAsync(() => _stateIndexRepo.DeleteAsync(visitor.State, GetLifecycleDueUtc(visitor), visitor.ActorId));
                if (installationIndexInserted) await TryAsync(() => _installationIndexRepo.DeleteAsync(visitor.InstallationIdHash));
                if (continuityIndexInserted) await TryAsync(() => _continuityIndexRepo.DeleteAsync(visitor.ContinuityTokenHash));
                await TryAsync(() => _visitorRepo.DeleteAsync(visitor.ActorId));
                throw;
            }
        }

        public Task<AnonymousVisitor> GetByActorIdAsync(string actorId)
        {
            if (String.IsNullOrEmpty(actorId)) throw new ArgumentNullException(nameof(actorId));
            return _visitorRepo.GetByActorIdAsync(actorId);
        }

        public Task<AnonymousVisitor> FindByContinuityTokenHashAsync(string continuityTokenHash)
        {
            return FindByLookupAsync(_continuityIndexRepo, continuityTokenHash, visitor => String.Equals(visitor.ContinuityTokenHash, continuityTokenHash, StringComparison.Ordinal));
        }

        public Task<AnonymousVisitor> FindByInstallationIdHashAsync(string installationIdHash)
        {
            return FindByLookupAsync(_installationIndexRepo, installationIdHash, visitor => String.Equals(visitor.InstallationIdHash, installationIdHash, StringComparison.Ordinal));
        }

        public async Task<IEnumerable<AnonymousVisitor>> GetByStateAsync(AnonymousVisitorState state, DateTime? dueBeforeUtc = null, int take = 100)
        {
            if (take <= 0) return Enumerable.Empty<AnonymousVisitor>();

            var actorIds = await _stateIndexRepo.FindActorIdsAsync(state, dueBeforeUtc, take);
            var visitors = await Task.WhenAll(actorIds.Distinct().Select(GetByActorIdAsync));
            var cutoffUtc = dueBeforeUtc?.ToUniversalTime();

            return visitors.Where(visitor => visitor != null && visitor.State == state && (!cutoffUtc.HasValue || GetLifecycleDueUtc(visitor) <= cutoffUtc.Value)).Take(take).ToList();
        }

        public async Task UpdateAsync(AnonymousVisitor visitor)
        {
            Validate(visitor);

            var existing = await _visitorRepo.GetByActorIdAsync(visitor.ActorId);
            if (existing == null) throw new InvalidOperationException($"Anonymous visitor '{visitor.ActorId}' was not found.");

            if (!String.IsNullOrEmpty(visitor.ContinuityTokenHash)) await EnsureLookupAsync(_continuityIndexRepo, visitor.ContinuityTokenHash, visitor);
            if (!String.IsNullOrEmpty(visitor.InstallationIdHash)) await EnsureLookupAsync(_installationIndexRepo, visitor.InstallationIdHash, visitor);
            await EnsureStateIndexAsync(visitor);
            await _visitorRepo.UpdateAsync(visitor);

            if (!String.Equals(existing.ContinuityTokenHash, visitor.ContinuityTokenHash, StringComparison.Ordinal) && !String.IsNullOrEmpty(existing.ContinuityTokenHash)) await _continuityIndexRepo.DeleteAsync(existing.ContinuityTokenHash);
            if (!String.Equals(existing.InstallationIdHash, visitor.InstallationIdHash, StringComparison.Ordinal) && !String.IsNullOrEmpty(existing.InstallationIdHash)) await _installationIndexRepo.DeleteAsync(existing.InstallationIdHash);
            if (existing.State != visitor.State || GetLifecycleDueUtc(existing) != GetLifecycleDueUtc(visitor)) await _stateIndexRepo.DeleteAsync(existing.State, GetLifecycleDueUtc(existing), existing.ActorId);

            var persisted = await _visitorRepo.GetByActorIdAsync(visitor.ActorId);
            if (persisted != null) visitor.ETag = persisted.ETag;
        }

        public async Task DeleteAsync(string actorId)
        {
            if (String.IsNullOrEmpty(actorId)) throw new ArgumentNullException(nameof(actorId));

            var visitor = await _visitorRepo.GetByActorIdAsync(actorId);
            await _visitorRepo.DeleteAsync(actorId);

            if (visitor == null) return;
            if (!String.IsNullOrEmpty(visitor.ContinuityTokenHash)) await TryAsync(() => _continuityIndexRepo.DeleteAsync(visitor.ContinuityTokenHash));
            if (!String.IsNullOrEmpty(visitor.InstallationIdHash)) await TryAsync(() => _installationIndexRepo.DeleteAsync(visitor.InstallationIdHash));
            await TryAsync(() => _stateIndexRepo.DeleteAsync(visitor.State, GetLifecycleDueUtc(visitor), visitor.ActorId));
        }

        private async Task<AnonymousVisitor> FindByLookupAsync(IAnonymousVisitorLookupRepo lookupRepo, string lookupHash, Func<AnonymousVisitor, bool> matches)
        {
            if (String.IsNullOrEmpty(lookupHash)) throw new ArgumentNullException(nameof(lookupHash));

            var actorId = await lookupRepo.FindActorIdAsync(lookupHash);
            if (String.IsNullOrEmpty(actorId)) return null;

            var visitor = await _visitorRepo.GetByActorIdAsync(actorId);
            if (visitor != null && matches(visitor)) return visitor;

            await TryAsync(() => lookupRepo.DeleteAsync(lookupHash));
            return null;
        }

        private static async Task<bool> EnsureLookupAsync(IAnonymousVisitorLookupRepo lookupRepo, string lookupHash, AnonymousVisitor visitor)
        {
            var existingActorId = await lookupRepo.FindActorIdAsync(lookupHash);
            if (String.Equals(existingActorId, visitor.ActorId, StringComparison.Ordinal)) return false;
            if (!String.IsNullOrEmpty(existingActorId)) throw new InvalidOperationException("Anonymous visitor lookup is already assigned to another actor.");

            await lookupRepo.InsertAsync(lookupHash, visitor.ActorId, visitor.CreatedUtc);
            return true;
        }

        private async Task<bool> EnsureStateIndexAsync(AnonymousVisitor visitor)
        {
            var dueUtc = GetLifecycleDueUtc(visitor);
            if (await _stateIndexRepo.ExistsAsync(visitor.State, dueUtc, visitor.ActorId)) return false;
            await _stateIndexRepo.InsertAsync(visitor.State, dueUtc, visitor.ActorId);
            return true;
        }

        private static DateTime GetLifecycleDueUtc(AnonymousVisitor visitor)
        {
            if (visitor.State == AnonymousVisitorState.Promoted) return (visitor.PromotedUtc ?? visitor.StateChangedUtc).ToUniversalTime();
            if (visitor.State == AnonymousVisitorState.Expired) return (visitor.ExpiredUtc ?? visitor.StateChangedUtc).ToUniversalTime();
            return visitor.ExpiresUtc.ToUniversalTime();
        }

        private static void Validate(AnonymousVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            if (String.IsNullOrEmpty(visitor.ActorId)) throw new ArgumentException("ActorId is required.", nameof(visitor));
            if (visitor.State == AnonymousVisitorState.Active && String.IsNullOrEmpty(visitor.ContinuityTokenHash) && String.IsNullOrEmpty(visitor.InstallationIdHash)) throw new ArgumentException("Active visitors require a continuity token or installation identifier.", nameof(visitor));
            if ((visitor.BootstrapContext?.Length ?? 0) > AnonymousVisitor.MaximumBootstrapContextLength) throw new ArgumentException($"BootstrapContext cannot exceed {AnonymousVisitor.MaximumBootstrapContextLength} characters.", nameof(visitor));
            if (visitor.CreatedUtc == default(DateTime)) throw new ArgumentException("CreatedUtc is required.", nameof(visitor));
            if (visitor.LastActivityUtc == default(DateTime)) throw new ArgumentException("LastActivityUtc is required.", nameof(visitor));
            if (visitor.ExpiresUtc == default(DateTime)) throw new ArgumentException("ExpiresUtc is required.", nameof(visitor));
            if (visitor.StateChangedUtc == default(DateTime)) throw new ArgumentException("StateChangedUtc is required.", nameof(visitor));
            if (visitor.ExpiresUtc.ToUniversalTime() <= visitor.CreatedUtc.ToUniversalTime()) throw new ArgumentException("ExpiresUtc must be later than CreatedUtc.", nameof(visitor));
            if (visitor.State == AnonymousVisitorState.Promoted && (String.IsNullOrEmpty(visitor.ProvisionalEnvironmentId) || !visitor.PromotedUtc.HasValue)) throw new ArgumentException("Promoted visitors require ProvisionalEnvironmentId and PromotedUtc.", nameof(visitor));
            if (visitor.State == AnonymousVisitorState.Expired && !visitor.ExpiredUtc.HasValue) throw new ArgumentException("Expired visitors require ExpiredUtc.", nameof(visitor));
        }

        private static async Task TryAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch
            {
                // Canonical visitor records are authoritative; stale indexes are ignored on reads.
            }
        }
    }
}
