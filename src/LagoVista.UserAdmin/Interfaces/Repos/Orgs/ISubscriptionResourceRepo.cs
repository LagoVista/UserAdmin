// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e0695929e181ca1f7c4a8d64e10aedf9e636155536fe1e0273830bb82e6abe5c
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ISubscriptionResourceRepo
    {
        Task<ListResponse<SubscriptionResource>> GetResourcesForSubscriptionAsync(Guid subscriptionId, ListRequest listRequest, string orgId);
    }
}
