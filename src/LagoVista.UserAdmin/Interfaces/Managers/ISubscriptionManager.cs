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
        Task<InvokeResult> DeleteSubscriptionAsync(Guid id, EntityHeader org, EntityHeader user);
        Task<DependentObjectCheckResult> CheckInUseAsync(Guid id, EntityHeader org, EntityHeader user);
        Task<Subscription> GetSubscriptionAsync(Guid id, EntityHeader org, EntityHeader user);
        Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId, EntityHeader user);
        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);
    }
}
