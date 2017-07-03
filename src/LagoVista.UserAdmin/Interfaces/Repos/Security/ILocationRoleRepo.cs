using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Account;
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface ILocationRoleRepo
    {
        Task AddRoleForAccountAsync(LocationUserRole locationUserRole);
        Task<IEnumerable<LocationUserRole>> GetRolesForAccountInLocationAsync(string locationId, string accountId);
        Task<IEnumerable<LocationUserRole>> GetAccountsForRoleInLocationAsync(string roleId, string locationId);
        Task RevokeRoleForAccountInLocationAsync(string locationId, string accountId,  string roleId);
        Task RevokeAllRolesForAccountInLocationAsync(string locationId, string accountId);
        Task<bool> ConfirmUserInRoleAsync(string locationId, string accountId, string roleId);
    }
}
