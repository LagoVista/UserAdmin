// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8cc5d60e4aa9a4375ea1c05dbd216245d7a661f5bcf016bea70102e077949e82
// IndexVersion: 0
// --- END CODE INDEX META ---
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
