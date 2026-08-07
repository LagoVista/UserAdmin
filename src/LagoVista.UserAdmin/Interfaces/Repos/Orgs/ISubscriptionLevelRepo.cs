using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ISubscriptionLevelRepo
    {
        Task AddSubscriptionLevelAsync(SubscriptionLevel subscriptionLevel);
        Task UpdateSubscriptionLevelAsync(SubscriptionLevel subscriptionLevel);
        Task DeleteSubscriptionLevelAsync(Guid id);
        Task<SubscriptionLevel> GetSubscriptionLevelAsync(Guid id);
        Task<SubscriptionLevel> GetSubscriptionLevelByKeyAsync(string key);
        Task<List<SubscriptionLevel>> GetSubscriptionLevelsAsync(bool activeOnly = false);
    }
}
