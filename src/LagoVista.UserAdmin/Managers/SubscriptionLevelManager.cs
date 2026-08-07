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
