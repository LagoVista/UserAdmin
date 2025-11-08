// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: afa14dac623236bc8bda467f99120d07df7f0592b953e081924c709bbcb1f232
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IDistributionListRepo
    {
        Task AddDistroListAsync(DistroList distroList);
        Task UpdateDistroListAsync(DistroList distroList);
        Task DeleteDistroListAsync(string id);
        Task<DistroList> GetDistroListAsync(String id, bool loadParents = false);
        Task<ListResponse<DistroListSummary>> GetDistroListsForOrgAsync(string orgId, ListRequest listRequest);
        Task<ListResponse<DistroListSummary>> GetDistroListsForCustomerAsync(string customerId, string orgId, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, string orgId);
    }
}
