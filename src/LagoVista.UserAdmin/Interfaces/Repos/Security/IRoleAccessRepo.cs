// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8a02760aa42b0bb1dd0df433211afd1570a95b7fda4551999c575457feb8238a
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IRoleAccessRepo
    {
        Task<List<RoleAccess>> GetRoleAccessForRoleAsync(string roleId, string organizationId);

        Task AddRoleAccess(RoleAccess roleAccess);

        Task RemoveRoleAccess(string roleAccessId, string organizationId);

        Task<List<RoleAccess>> GetRoleAccessForModuleAsync(string moduleId, string organizationId);
        Task<List<RoleAccess>> GetRoleAccessForAreaAsync(string moduleId, string areaId, string organizationId);
        Task<List<RoleAccess>> GetRoleAccessForPageAsync(string moduleId, string areaId, string pageId, string organizationId);

        Task<List<RoleAccess>> GetRoleAccessForModuleFeatureAsync(string moduleId, string featureId, string organizationId);
        Task<List<RoleAccess>> GetRoleAccessForAreaFeatureAsync(string moduleId, string areaId, string featureId, string organizationId);
        Task<List<RoleAccess>> GetRoleAccessForPageFeatureAsync(string moduleId, string areaId, string pageId, string featureId, string organizationId);
     }
}
