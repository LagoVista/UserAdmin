using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IBusinessUnitManager
    {
        Task<InvokeResult> AddBusinessUnitAsync(BusinessUnit businessUnit, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateBusinessUnitAsync(BusinessUnit businessUnit, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteBusinessUnitAsync(string id, EntityHeader org, EntityHeader user);
        Task<BusinessUnit> GetBusinessUnitAsync(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<BusinessUnitSummary>> GetBusinessUnitsForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org, EntityHeader user);
        Task<DependentObjectCheckResult> CheckBusinessUnitInUseAsync(string id, EntityHeader org, EntityHeader user);
    }
}
