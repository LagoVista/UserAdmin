using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISubscriptionManager
    {
        Task<InvokeResult> AddSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteSubscriptionAsync(String id, EntityHeader org, EntityHeader user);
        Task<bool> CanDeleteSubscriptionAsync(String id, EntityHeader org, EntityHeader user);
        Task<Subscription> GetSubscriptionAsync(String id, EntityHeader org, EntityHeader user);
        Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId, EntityHeader user);
        Task<bool> QueryKeyInUse(string key, EntityHeader org);
    }
}
