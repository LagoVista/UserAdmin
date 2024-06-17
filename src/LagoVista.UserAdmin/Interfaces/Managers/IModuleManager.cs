using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IModuleManager
    {
        Task<InvokeResult> AddModuleAsync(Module module, EntityHeader org, EntityHeader user);
        Task<Module> GetModuleAsync(string id, EntityHeader org, EntityHeader user);
        Task<Module> GetModuleByKeyAsync(string key, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteModuleAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateModuleAsync(Module module, EntityHeader org, EntityHeader user);
        Task<ListResponse<ModuleSummary>> GetAllModulesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<ModuleSummary>> GetAllModulesAsync(EntityHeader user, ListRequest listRequest);
        Task<ListResponse<ModuleSummary>> SysAdminGetModuleAsync(string orgId, EntityHeader user);
        List<UiCategory> GetTopLevelCategories();
    }
}
