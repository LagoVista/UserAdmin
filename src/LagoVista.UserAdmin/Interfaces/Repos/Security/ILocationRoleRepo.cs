using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface ILocationRoleRepo
    {
        Task AddRoleForUserAsync(LocationUserRole locationUserRole);
        Task<IEnumerable<LocationUserRole>> GetRolesForUserInLocationAsync(string locationId, string userId);
        Task<IEnumerable<LocationUserRole>> GetUsersInRoleForLocationAsync(string roleId, string locationId);
        Task RevokeRoleForUserInLocationAsync(string locationId, string userId,  string roleId);
        Task RevokeAllRolesForUserInLocationAsync(string locationId, string userId);
        Task<bool> ConfirmUserInRoleAsync(string locationId, string userId, string roleId);
    }
}
