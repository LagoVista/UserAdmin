using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Account;
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface ILocationRoleRepo
    {
        Task<LocationAccountRoles> AddRoleForAccountAsync(EntityHeader location, EntityHeader account, EntityHeader role, EntityHeader addeBy);
        Task<IEnumerable<LocationAccountRoles>> GetRolesForAccountInLocationAsync(string locationId, string accountId);
        Task<IEnumerable<LocationAccountRoles>> GetAccountsForRoleInLocationAsync(string roleId, string locationId);
        Task RevokeRoleForAccountInLocationAsync(EntityHeader location, EntityHeader role, EntityHeader account, EntityHeader revokedBy);
        Task RevokeAllRolesForAccountInLocationAsync(EntityHeader location, EntityHeader acount, EntityHeader revokedBy);
    }
}
