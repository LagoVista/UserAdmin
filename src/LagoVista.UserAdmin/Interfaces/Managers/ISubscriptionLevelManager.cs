using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISubscriptionLevelManager
    {
        Task<InvokeResult<SubscriptionLevel>> EnsureSystemSubscriptionLevelAsync(SubscriptionLevel defaultLevel);
        Task<InvokeResult> AddSubscriptionLevelAsync(SubscriptionLevel subscriptionLevel);
        Task<InvokeResult> UpdateSubscriptionLevelAsync(SubscriptionLevel subscriptionLevel);
        Task<InvokeResult> DeleteSubscriptionLevelAsync(Guid id);
        Task<SubscriptionLevel> GetSubscriptionLevelAsync(Guid id);
        Task<SubscriptionLevel> GetSubscriptionLevelByKeyAsync(string key);
        Task<List<SubscriptionLevel>> GetSubscriptionLevelsAsync(bool activeOnly = false);
    }
}
