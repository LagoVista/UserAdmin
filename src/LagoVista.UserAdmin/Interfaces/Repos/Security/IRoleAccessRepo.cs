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
