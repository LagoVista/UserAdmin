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
        Task<DistroList> GetDistroListAsync(String id);
        Task<ListResponse<DistroListSummary>> GetDistroListsForOrgAsync(string orgId, ListRequest listRequest);
        Task<ListResponse<DistroListSummary>> GetDistroListsForCustomerAsync(string customerId, string orgId, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, string orgId);
    }
}
