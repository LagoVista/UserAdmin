// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a954f2e8388b0dacfc837f273c209f0408a7b6e4e5d93201d2e28f721e3c8c8b
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ISubscriptionRepo
    {
        Task AddSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user);
        Task UpdateSubscriptionAsync(Subscription subscription, EntityHeader org, EntityHeader user);
        Task DeleteSubscriptionAsync(GuidString36 id, EntityHeader org, EntityHeader user);
        Task DeleteSubscriptionsForOrgAsync(EntityHeader org, EntityHeader user);
        Task<Subscription> GetTrialSubscriptionAsync(string orgId, EntityHeader org, EntityHeader user);

        Task<Subscription> GetSubscriptionAsync(GuidString36 id, EntityHeader org, EntityHeader user);
        Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForOrgAsync(EntityHeader org, EntityHeader user, ListRequest request);
        Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForCustomerAsync(GuidString36 customerId, EntityHeader org, EntityHeader user, ListRequest request);
    }
}
