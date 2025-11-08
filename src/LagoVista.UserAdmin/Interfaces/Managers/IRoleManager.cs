// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fda51181dcf58b7dce36cb7cb4c8c1cb058a5faa024a821923382ce50fc020d1
// IndexVersion: 2
// --- END CODE INDEX META ---
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

        Task<List<RoleAccess>> GetRoleAccessForModuleAsync(string moduleId, EntityHeader org, EntityHeader user);
        Task<List<RoleAccess>> GetRoleAccessForAreaAsync(string moduleId, string areaId, EntityHeader org, EntityHeader user);
        Task<List<RoleAccess>> GetRoleAccessForPageAsync(string moduleId, string areaId, string pageId, EntityHeader org, EntityHeader user);

        Task<List<RoleAccess>> GetRoleAccessForModuleFeatureAsync(string moduleId, string featureId, EntityHeader org, EntityHeader user);
        Task<List<RoleAccess>> GetRoleAccessForAreaFeatureAsync(string moduleId, string areaId, string featureId, EntityHeader org, EntityHeader user);
        Task<List<RoleAccess>> GetRoleAccessForPageFeatureAsync(string moduleId, string areaId, string pageId, string featureId, EntityHeader org, EntityHeader user);

    }
}
