// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 290b431308d26597711e15c76cf1c20562dbcb1fe245708c9a26161354d5dd6e
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
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
        Task<InvokeResult> DeleteSubscriptionsForOrgAsync(string id, EntityHeader org, EntityHeader user);
        Task<Subscription> GetTrialSubscriptionAsync(EntityHeader org, EntityHeader user);
        Task<Subscription> GetSubscriptionAsync(GuidString36 id, EntityHeader org, EntityHeader user);
        Task<ListResponse<SubscriptionResource>> GetResourcesForSubscriptionAsync(GuidString36 subscriptionId, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForOrgAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<SubscriptionSummary>> GetSubscriptionsForCustomerAsync(GuidString36 customerId, ListRequest listRequest, EntityHeader org, EntityHeader user);
        bool IsForInitialization { get; set; }
    }
}
