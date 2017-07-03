using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IOrganizationRoleRepo
    {
        Task AddRoleForAccountAsync(OrganizationUserRole userRole);
        Task<bool> ConfirmUserInRoleAsync(string orgId, string accountId, string roleId);
        Task<IEnumerable<OrganizationUserRole>> GetRolesForAccountAsync(string accountId, string organizationId);
        Task<IEnumerable<OrganizationUserRole>> GetAccountsForRoleAsync(string roleId, string organziationId);
        Task RevokeRoleForAccountInOrgAsync(string orgId, string userId, string roleId);
        Task RevokeAllRolesForAccountInOrgAsync(string orgId, string account);
    }
}
