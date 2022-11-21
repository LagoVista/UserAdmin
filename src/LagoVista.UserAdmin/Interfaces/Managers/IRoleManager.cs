using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IRoleManager
    {
        Task<InvokeResult> AddRoleAsync(Role role, EntityHeader org, EntityHeader user);
        Task<Role> GetRoleAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateRoleAsync(Role role, EntityHeader org, EntityHeader user);
        Task<List<RoleSummary>> GetRolesAsync(EntityHeader org, EntityHeader user);

        Task<List<RoleAccess>> GetRoleAccessAsync(string roleId, EntityHeader org, EntityHeader user);

        Task<InvokeResult> AddRoleAccessAsync(RoleAccess access, EntityHeader org, EntityHeader user);

        Task<InvokeResult> RevokeRoleAccessAsync(string accessId, EntityHeader org, EntityHeader user);

        Task<List<RoleSummary>> GetAssignableRolesAsync(EntityHeader org, EntityHeader user);
    }
}
