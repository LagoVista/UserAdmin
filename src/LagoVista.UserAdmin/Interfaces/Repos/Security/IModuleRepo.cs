// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9120d5305e1e7b9d90c46c16678b52b52ab3bffb906afe844987b86601528243
// IndexVersion: 0
// --- END CODE INDEX META ---
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
        Task<ListResponse<ModuleSummary>> GetAllModulesForOrgAsync(string orgId, ListRequest listRequest);
        Task<List<ModuleSummary>> GetModulesForOrgAndPublicAsync(string orgId, bool isForProductLine);
        Task<ListResponse<ModuleSummary>> GetAllModulesAsync(ListRequest listRequest);
    }
}
