using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentRepo : IProvisionalEnvironmentRepo
    {
        private const string EnvironmentCachePrefix = "provisional-environment:id:";
        private const string CreationCachePrefix = "provisional-environment:creation:";
        private const string RecoveryCachePrefix = "provisional-environment:recovery:";
        private const string InstallationCachePrefix = "provisional-environment:installation:";

        private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(3);

        private readonly IProvisionalEnvironmentEntityRepo _environmentRepo;
        private readonly IProvisionalEnvironmentCreationIndexRepo _creationIndexRepo;
        private readonly IProvisionalEnvironmentRecoveryIndexRepo _recoveryIndexRepo;
        private readonly IProvisionalEnvironmentInstallationIndexRepo _installationIndexRepo;
        private readonly IProvisionalEnvironmentStateIndexRepo _stateIndexRepo;
        private readonly ICacheProvider _cacheProvider;

        public ProvisionalEnvironmentRepo(IProvisionalEnvironmentEntityRepo environmentRepo, IProvisionalEnvironmentCreationIndexRepo creationIndexRepo, IProvisionalEnvironmentRecoveryIndexRepo recoveryIndexRepo, IProvisionalEnvironmentInstallationIndexRepo installationIndexRepo, IProvisionalEnvironmentStateIndexRepo stateIndexRepo, ICacheProvider cacheProvider)
        {
            _environmentRepo = environmentRepo ?? throw new ArgumentNullException(nameof(environmentRepo));
            _creationIndexRepo = creationIndexRepo ?? throw new ArgumentNullException(nameof(creationIndexRepo));
            _recoveryIndexRepo = recoveryIndexRepo ?? throw new ArgumentNullException(nameof(recoveryIndexRepo));
            _installationIndexRepo = installationIndexRepo ?? throw new ArgumentNullException(nameof(installationIndexRepo));
            _stateIndexRepo = stateIndexRepo ?? throw new ArgumentNullException(nameof(stateIndexRepo));
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        }

        public async Task CreateAsync(ProvisionalEnvironment environment)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));

            var creationIndexInserted = false;
            var recoveryIndexInserted = false;
            var installationIndexInserted = false;
            var stateIndexInserted = false;

            await _environmentRepo.InsertAsync(environment);

            try
            {
                if (!String.IsNullOrEmpty(environment.CreationRequestId))
                {
                    creationIndexInserted = await EnsureCreationIndexAsync(environment);
                }

                if (!String.IsNullOrEmpty(environment.RecoveryTokenHash))
                {
                    recoveryIndexInserted = await EnsureRecoveryIndexAsync(environment);
                }

                if (!String.IsNullOrEmpty(environment.InstallationIdHash))
                {
                    installationIndexInserted = await EnsureInstallationIndexAsync(environment);
                }

                stateIndexInserted = await EnsureStateIndexAsync(environment);
                await CacheAsync(environment);
            }
            catch
            {
                if (stateIndexInserted) await TryAsync(() => _stateIndexRepo.DeleteAsync(environment.State, environment.ExpiresUtc, environment.Id));
                if (installationIndexInserted) await TryAsync(() => _installationIndexRepo.DeleteAsync(environment.InstallationIdHash));
                if (recoveryIndexInserted) await TryAsync(() => _recoveryIndexRepo.DeleteAsync(environment.RecoveryTokenHash));
                if (creationIndexInserted) await TryAsync(() => _creationIndexRepo.DeleteAsync(environment.CreationRequestId));
                await TryAsync(() => _environmentRepo.DeleteAsync(environment.Id));
                await InvalidateCacheAsync(environment);
                throw;
            }
        }

        public async Task<ProvisionalEnvironment> GetByIdAsync(string id)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var environment = await TryGetCacheAsync<ProvisionalEnvironment>(GetEnvironmentCacheKey(id));
            if (environment == null) environment = await _environmentRepo.GetByIdAsync(id);
            if (environment != null) await CacheAsync(environment);
            return environment;
        }

        public Task<ProvisionalEnvironment> FindByCreationRequestIdAsync(string creationRequestId)
        {
            return FindByLookupAsync(creationRequestId, CreationCachePrefix, _creationIndexRepo.FindEnvironmentIdAsync, environment => String.Equals(environment.CreationRequestId, creationRequestId, StringComparison.Ordinal), () => _creationIndexRepo.DeleteAsync(creationRequestId));
        }

        public Task<ProvisionalEnvironment> FindByRecoveryTokenHashAsync(string recoveryTokenHash)
        {
            return FindByLookupAsync(recoveryTokenHash, RecoveryCachePrefix, _recoveryIndexRepo.FindEnvironmentIdAsync, environment => String.Equals(environment.RecoveryTokenHash, recoveryTokenHash, StringComparison.Ordinal), () => _recoveryIndexRepo.DeleteAsync(recoveryTokenHash));
        }

        public Task<ProvisionalEnvironment> FindByInstallationIdHashAsync(string installationIdHash)
        {
            return FindByLookupAsync(installationIdHash, InstallationCachePrefix, _installationIndexRepo.FindEnvironmentIdAsync, environment => String.Equals(environment.InstallationIdHash, installationIdHash, StringComparison.Ordinal), () => _installationIndexRepo.DeleteAsync(installationIdHash));
        }

        public async Task<IEnumerable<ProvisionalEnvironment>> GetByStateAsync(ProvisionalEnvironmentState state, DateTime? expiresBeforeUtc = null, int take = 100)
        {
            if (take <= 0) return Enumerable.Empty<ProvisionalEnvironment>();

            var environmentIds = await _stateIndexRepo.FindEnvironmentIdsAsync(state, expiresBeforeUtc, take);
            var environments = await Task.WhenAll(environmentIds.Distinct().Select(GetByIdAsync));
            var cutoffUtc = expiresBeforeUtc?.ToUniversalTime();

            return environments.Where(environment => environment != null && environment.State == state && (!cutoffUtc.HasValue || environment.ExpiresUtc.ToUniversalTime() <= cutoffUtc.Value)).Take(take).ToList();
        }

        public async Task UpdateAsync(ProvisionalEnvironment environment)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));

            var existing = await _environmentRepo.GetByIdAsync(environment.Id);
            if (existing == null) throw new InvalidOperationException($"Provisional environment '{environment.Id}' was not found.");

            await EnsureCreationIndexAsync(environment);
            await EnsureRecoveryIndexAsync(environment);
            await EnsureInstallationIndexAsync(environment);
            await EnsureStateIndexAsync(environment);
            await _environmentRepo.UpdateAsync(environment);
            await InvalidateCacheAsync(existing);
            await InvalidateCacheAsync(environment);

            if (!String.Equals(existing.CreationRequestId, environment.CreationRequestId, StringComparison.Ordinal) && !String.IsNullOrEmpty(existing.CreationRequestId)) await _creationIndexRepo.DeleteAsync(existing.CreationRequestId);
            if (!String.Equals(existing.RecoveryTokenHash, environment.RecoveryTokenHash, StringComparison.Ordinal) && !String.IsNullOrEmpty(existing.RecoveryTokenHash)) await _recoveryIndexRepo.DeleteAsync(existing.RecoveryTokenHash);
            if (!String.Equals(existing.InstallationIdHash, environment.InstallationIdHash, StringComparison.Ordinal) && !String.IsNullOrEmpty(existing.InstallationIdHash)) await _installationIndexRepo.DeleteAsync(existing.InstallationIdHash);
            if (existing.State != environment.State || existing.ExpiresUtc != environment.ExpiresUtc) await _stateIndexRepo.DeleteAsync(existing.State, existing.ExpiresUtc, existing.Id);
            await CacheAsync(environment);
        }

        public async Task DeleteAsync(string id)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var environment = await _environmentRepo.GetByIdAsync(id);
            await _environmentRepo.DeleteAsync(id);

            if (environment != null)
            {
                if (!String.IsNullOrEmpty(environment.CreationRequestId)) await TryAsync(() => _creationIndexRepo.DeleteAsync(environment.CreationRequestId));
                if (!String.IsNullOrEmpty(environment.RecoveryTokenHash)) await TryAsync(() => _recoveryIndexRepo.DeleteAsync(environment.RecoveryTokenHash));
                if (!String.IsNullOrEmpty(environment.InstallationIdHash)) await TryAsync(() => _installationIndexRepo.DeleteAsync(environment.InstallationIdHash));
                await TryAsync(() => _stateIndexRepo.DeleteAsync(environment.State, environment.ExpiresUtc, environment.Id));
                await InvalidateCacheAsync(environment);
            }

            await TryAsync(() => _cacheProvider.RemoveAsync(GetEnvironmentCacheKey(id)));
        }

        private async Task<ProvisionalEnvironment> FindByLookupAsync(string lookupValue, string cachePrefix, Func<string, Task<string>> findEnvironmentIdAsync, Func<ProvisionalEnvironment, bool> matches, Func<Task> deleteStaleIndexAsync)
        {
            if (String.IsNullOrEmpty(lookupValue)) throw new ArgumentNullException(nameof(lookupValue));

            var cacheKey = GetLookupCacheKey(cachePrefix, lookupValue);
            var environmentId = await TryGetCacheAsync<string>(cacheKey);
            if (String.IsNullOrEmpty(environmentId)) environmentId = await findEnvironmentIdAsync(lookupValue);
            if (String.IsNullOrEmpty(environmentId)) return null;

            var environment = await GetByIdAsync(environmentId);
            if (environment == null || !matches(environment))
            {
                await TryAsync(() => _cacheProvider.RemoveAsync(cacheKey));
                await TryAsync(deleteStaleIndexAsync);
                return null;
            }

            await TryAsync(() => _cacheProvider.AddAsync(cacheKey, environment.Id, CacheDuration));
            return environment;
        }

        private async Task<bool> EnsureCreationIndexAsync(ProvisionalEnvironment environment)
        {
            if (String.IsNullOrEmpty(environment.CreationRequestId)) return false;

            var existingEnvironmentId = await _creationIndexRepo.FindEnvironmentIdAsync(environment.CreationRequestId);
            if (String.Equals(existingEnvironmentId, environment.Id, StringComparison.Ordinal)) return false;
            if (!String.IsNullOrEmpty(existingEnvironmentId)) throw new InvalidOperationException($"Creation request '{environment.CreationRequestId}' is already assigned to another provisional environment.");

            await _creationIndexRepo.InsertAsync(environment.CreationRequestId, environment.Id, environment.CreatedUtc);
            return true;
        }

        private async Task<bool> EnsureRecoveryIndexAsync(ProvisionalEnvironment environment)
        {
            if (String.IsNullOrEmpty(environment.RecoveryTokenHash)) return false;

            var existingEnvironmentId = await _recoveryIndexRepo.FindEnvironmentIdAsync(environment.RecoveryTokenHash);
            if (String.Equals(existingEnvironmentId, environment.Id, StringComparison.Ordinal)) return false;
            if (!String.IsNullOrEmpty(existingEnvironmentId)) throw new InvalidOperationException("Recovery token is already assigned to another provisional environment.");

            await _recoveryIndexRepo.InsertAsync(environment.RecoveryTokenHash, environment.Id, environment.CreatedUtc);
            return true;
        }

        private async Task<bool> EnsureInstallationIndexAsync(ProvisionalEnvironment environment)
        {
            if (String.IsNullOrEmpty(environment.InstallationIdHash)) return false;

            var existingEnvironmentId = await _installationIndexRepo.FindEnvironmentIdAsync(environment.InstallationIdHash);
            if (String.Equals(existingEnvironmentId, environment.Id, StringComparison.Ordinal)) return false;
            if (!String.IsNullOrEmpty(existingEnvironmentId)) throw new InvalidOperationException("Installation is already assigned to another provisional environment.");

            await _installationIndexRepo.InsertAsync(environment.InstallationIdHash, environment.Id, environment.CreatedUtc);
            return true;
        }

        private async Task<bool> EnsureStateIndexAsync(ProvisionalEnvironment environment)
        {
            if (await _stateIndexRepo.ExistsAsync(environment.State, environment.ExpiresUtc, environment.Id)) return false;
            await _stateIndexRepo.InsertAsync(environment.State, environment.ExpiresUtc, environment.Id);
            return true;
        }

        private async Task CacheAsync(ProvisionalEnvironment environment)
        {
            await TryAsync(() => _cacheProvider.AddAsync(GetEnvironmentCacheKey(environment.Id), environment, CacheDuration));
            if (!String.IsNullOrEmpty(environment.CreationRequestId)) await TryAsync(() => _cacheProvider.AddAsync(GetLookupCacheKey(CreationCachePrefix, environment.CreationRequestId), environment.Id, CacheDuration));
            if (!String.IsNullOrEmpty(environment.RecoveryTokenHash)) await TryAsync(() => _cacheProvider.AddAsync(GetLookupCacheKey(RecoveryCachePrefix, environment.RecoveryTokenHash), environment.Id, CacheDuration));
            if (!String.IsNullOrEmpty(environment.InstallationIdHash)) await TryAsync(() => _cacheProvider.AddAsync(GetLookupCacheKey(InstallationCachePrefix, environment.InstallationIdHash), environment.Id, CacheDuration));
        }

        private async Task InvalidateCacheAsync(ProvisionalEnvironment environment)
        {
            await TryAsync(() => _cacheProvider.RemoveAsync(GetEnvironmentCacheKey(environment.Id)));
            if (!String.IsNullOrEmpty(environment.CreationRequestId)) await TryAsync(() => _cacheProvider.RemoveAsync(GetLookupCacheKey(CreationCachePrefix, environment.CreationRequestId)));
            if (!String.IsNullOrEmpty(environment.RecoveryTokenHash)) await TryAsync(() => _cacheProvider.RemoveAsync(GetLookupCacheKey(RecoveryCachePrefix, environment.RecoveryTokenHash)));
            if (!String.IsNullOrEmpty(environment.InstallationIdHash)) await TryAsync(() => _cacheProvider.RemoveAsync(GetLookupCacheKey(InstallationCachePrefix, environment.InstallationIdHash)));
        }

        private static string GetEnvironmentCacheKey(string environmentId)
        {
            return $"{EnvironmentCachePrefix}{environmentId}";
        }

        private static string GetLookupCacheKey(string prefix, string lookupValue)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(lookupValue));
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var value in hash) builder.Append(value.ToString("x2"));
                return $"{prefix}{builder}";
            }
        }

        private async Task<T> TryGetCacheAsync<T>(string key)
        {
            try
            {
                return await _cacheProvider.GetAsync<T>(key);
            }
            catch
            {
                return default(T);
            }
        }

        private static async Task TryAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch
            {
                // Canonical records are authoritative; stale projections are ignored on reads.
            }
        }
    }
}
