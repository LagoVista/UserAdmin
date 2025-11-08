// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 09086ddd29e2c71322c604d2c96084c57530dcb06c20159972f3147e60eb4692
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IUserSecurityServices
    {
        Task<List<string>> GetRolesNamesForUserAsync(string userId, string orgId);
        Task<List<string>> GetRolesKeysForUserAsync(string userId, string orgId);
        Task<List<Role>> GetRolesForUserAsync(string userId, string orgId);
        Task<List<RoleAccess>> GetRoleAccessForUserAsync(string userId, string orgId);
        Task<List<RoleAccess>> GetModuleRoleAccessForUserAsync(string moduleId, string userId, string orgId);
        Task AssertFinanceAdminAsync(string userId, string orgId);
    }
}
