using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IUserSecurityServices
    {
        Task<List<string>> GetRolesNamesForUserAsync(string userId, string orgId);
        Task<List<Role>> GetRolesForUserAsync(string userId, string orgId);
        Task<List<RoleAccess>> GetRoleAccessForUserAsync(string userId, string orgId);
        Task<List<RoleAccess>> GetModuleRoleAccessForUserAsync(string moduleId, string userId, string orgId);
    }
}
