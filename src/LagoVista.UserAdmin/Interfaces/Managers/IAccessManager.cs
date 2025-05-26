using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IIUserAccessManager
    {
        Task<List<ModuleSummary>> GetUserModulesAsync(string userId, string orgId);
        Task<Module> GetUserModuleAsync(string moduleId, string userId, string orgId);
        Task<List<Module>> GetFullAppTreeForUserAsync(string userId, string orgId);
    }
}
