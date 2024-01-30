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
