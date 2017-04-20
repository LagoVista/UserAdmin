using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ISubscriptionRepo
    {
        Task AddSubscriptionAsync(Subscription subscription);
        Task UpdateSubscriptionAsync(Subscription subscription);
        Task DeleteSubscriptionAsync(string id);

        Task<Subscription> GetSubscriptionAsync(String id);

        Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId);

        Task<bool> QueryKeyInUse(string key, string orgId);
    }
}
