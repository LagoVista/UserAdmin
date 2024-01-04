using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IModuleRepo
    {
        Task AddModuleAsync(Module module);
        Task<Module> GetModuleAsync(string id);
        Task<Module> GetModuleByKeyAsync(string key);
        Task<Module> GetModuleByKeyAsync(string key, string orgId);
        Task DeleteModuleAsync(string id);
        Task UpdateModuleAsync(Module module);
        Task<ListResponse<ModuleSummary>> GetAllModulesAsync(string orgId, ListRequest listRequest);
        Task<List<ModuleSummary>> GetModulesForOrgAndPublicAsyncAsync(string orgId);
    }
}
