using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IOrganizationRoleRepo
    {
        Task AddRoleForUserAsync(OrganizationUserRole userRole);
        Task<bool> ConfirmUserInRoleAsync(string orgId, string userId, string roleId);
        Task<IEnumerable<OrganizationUserRole>> GetRolesForUserAsync(string userId, string orgId);
        Task<IEnumerable<OrganizationUserRole>> GetUserForRoleAsync(string roleId, string orgId);
        Task RevokeRoleForUserInOrgAsync(string orgId, string userId, string roleId);
        Task RevokeAllRolesForUserInOrgAsync(string orgId, string userId);
    }
}
