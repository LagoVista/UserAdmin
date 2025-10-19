// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a954f2e8388b0dacfc837f273c209f0408a7b6e4e5d93201d2e28f721e3c8c8b
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ISubscriptionRepo
    {
        Task AddSubscriptionAsync(SubscriptionDTO subscription);
        Task UpdateSubscriptionAsync(SubscriptionDTO subscription);
        Task DeleteSubscriptionAsync(string orgId, Guid id);
        Task DeleteSubscriptionsForOrgAsync(string orgId);
        Task<SubscriptionDTO> GetTrialSubscriptionAsync(string orgId);

        Task<SubscriptionDTO> GetSubscriptionAsync(string orgId, Guid id, bool disableTracking = false);

        Task<IEnumerable<SubscriptionSummary>> GetSubscriptionsForOrgAsync(string orgId);

        Task<bool> QueryKeyInUse(string key, string orgId);
    }
}
