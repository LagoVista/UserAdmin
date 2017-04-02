using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IOrganizationRoleRepo
    {
        Task<OrganizationAccountRoles> AddRoleForAccountAsync(EntityHeader organization, EntityHeader account, EntityHeader role, EntityHeader addedBy);
        Task<IEnumerable<OrganizationAccountRoles>> GetRolesForAccountAsync(string accountId, string organizationId);
        Task<IEnumerable<OrganizationAccountRoles>> GetAccountsForRoleAsync(string roleId, string organziationId);
        Task RevokeRoleForAccountInOrgAsync(EntityHeader org, EntityHeader role, EntityHeader account, EntityHeader revokedBy);
        Task RevokeAllRolesForAccountInOrgAsync(EntityHeader org, EntityHeader account, EntityHeader revokedBy);
    }
}
