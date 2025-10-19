// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cea654d330df8f8f7b046f15e7fcf7f43dc6a550dc0aa143a1c96885f8816186
// IndexVersion: 0
// --- END CODE INDEX META ---
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
