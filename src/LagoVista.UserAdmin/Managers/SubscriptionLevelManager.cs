using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class SubscriptionLevelManager : ISubscriptionLevelManager
    {
        private readonly ISubscriptionLevelRepo _subscriptionLevelRepo;

        public SubscriptionLevelManager(ISubscriptionLevelRepo subscriptionLevelRepo)
        {
            _subscriptionLevelRepo = subscriptionLevelRepo ?? throw new ArgumentNullException(nameof(subscriptionLevelRepo));
        }

        public async Task<InvokeResult<SubscriptionLevel>> EnsureSystemSubscriptionLevelAsync(SubscriptionLevel defaultLevel)
        {
            if (defaultLevel == null) throw new ArgumentNullException(nameof(defaultLevel));
            if (defaultLevel.Id == Guid.Empty) return InvokeResult<SubscriptionLevel>.FromError("A canonical ID is required for a system subscription level.");
            if (String.IsNullOrWhiteSpace(defaultLevel.Key)) return InvokeResult<SubscriptionLevel>.FromError("Key is required for a system subscription level.");
            if (String.IsNullOrWhiteSpace(defaultLevel.Name)) return InvokeResult<SubscriptionLevel>.FromError("Name is required for a system subscription level.");

            var existingByKey = await _subscriptionLevelRepo.GetSubscriptionLevelByKeyAsync(defaultLevel.Key);
            if (existingByKey != null) return InvokeResult<SubscriptionLevel>.Create(existingByKey);

            var existingById = await _subscriptionLevelRepo.GetSubscriptionLevelAsync(defaultLevel.Id);
            if (existingById != null)
            {
                return InvokeResult<SubscriptionLevel>.FromError($"The canonical subscription level ID for '{defaultLevel.Key}' is already used by subscription level '{existingById.Key}'.");
            }

            try
            {
                await _subscriptionLevelRepo.AddSubscriptionLevelAsync(defaultLevel);
                return InvokeResult<SubscriptionLevel>.Create(defaultLevel);
            }
            catch
            {
                existingByKey = await _subscriptionLevelRepo.GetSubscriptionLevelByKeyAsync(defaultLevel.Key);
                if (existingByKey != null) return InvokeResult<SubscriptionLevel>.Create(existingByKey);

                existingById = await _subscriptionLevelRepo.GetSubscriptionLevelAsync(defaultLevel.Id);
                if (existingById != null && String.Equals(existingById.Key, defaultLevel.Key, StringComparison.Ordinal))
                    return InvokeResult<SubscriptionLevel>.Create(existingById);

                throw;
            }
        }

        public async Task<InvokeResult> AddSubscriptionLevelAsync(SubscriptionLevel subscriptionLevel)
        {
            if (subscriptionLevel == null)
                throw new ArgumentNullException(nameof(subscriptionLevel));

            if (subscriptionLevel.Id == Guid.Empty)
                subscriptionLevel.Id = Guid.NewGuid();

            await _subscriptionLevelRepo.AddSubscriptionLevelAsync(subscriptionLevel);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateSubscriptionLevelAsync(SubscriptionLevel subscriptionLevel)
        {
            if (subscriptionLevel == null)
                throw new ArgumentNullException(nameof(subscriptionLevel));

            await _subscriptionLevelRepo.UpdateSubscriptionLevelAsync(subscriptionLevel);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteSubscriptionLevelAsync(Guid id)
        {
            await _subscriptionLevelRepo.DeleteSubscriptionLevelAsync(id);
            return InvokeResult.Success;
        }

        public Task<SubscriptionLevel> GetSubscriptionLevelAsync(Guid id)
        {
            return _subscriptionLevelRepo.GetSubscriptionLevelAsync(id);
        }

        public Task<SubscriptionLevel> GetSubscriptionLevelByKeyAsync(string key)
        {
            return _subscriptionLevelRepo.GetSubscriptionLevelByKeyAsync(key);
        }

        public Task<List<SubscriptionLevel>> GetSubscriptionLevelsAsync(bool activeOnly = false)
        {
            return _subscriptionLevelRepo.GetSubscriptionLevelsAsync(activeOnly);
        }
    }
}
