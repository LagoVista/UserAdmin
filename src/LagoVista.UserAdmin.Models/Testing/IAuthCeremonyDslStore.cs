using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models.UIMetaData;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// Persistence abstraction for storing/retrieving AuthCeremonyDsl specs.
    /// Id is the only key used by clients/APIs; ScenarioName is display-only.
    /// </summary>
    public interface IAppUserTestingDslRepo
    {
        Task<AppUserTestingDSL> GetByIdAsync(string id);
        Task<ListResponse<AppUserTestingDSLSummary>> ListAsync(string orgId, ListRequest request);
 
        Task AddDSLAsync(AppUserTestingDSL dsl);

        Task UpdateDSLAsync(AppUserTestingDSL dsl);

        Task DeleteByIdAsync(string id);
    }
}
