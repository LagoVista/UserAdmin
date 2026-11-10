using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IBusinessUnitRepo
    {
        Task AddBusinessUnitAsync(BusinessUnit businessUnit);
        Task UpdateBusinessUnitAsync(BusinessUnit businessUnit);
        Task<BusinessUnit> GetBusinessUnitAsync(string id);
        Task DeleteBusinessUnitAsync(string id);
        Task<ListResponse<BusinessUnitSummary>> GetBusinessUnitsForOrgAsync(string orgId, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, string orgId, string excludeId = null);
    }
}
