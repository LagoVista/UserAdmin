// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c260ef46015760cc5e1a553770bcd0a0d9bbb801f95c1820acc809bb1d907d38
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IUserRoleManager
    {
        Task<InvokeResult<UserRole>> GrantUserRoleAsync(string userId, string roleId, EntityHeader org, EntityHeader user);
        Task<List<InvokeResult<UserRole>>> GrantUserRolesAsync(string userId, List<string> roles, EntityHeader org, EntityHeader user);

        Task<InvokeResult> RevokeUserRoleAsync(string userRoleId, EntityHeader org, EntityHeader user);

        Task<List<UserRole>> GetRolesForUserAsync(string userId, EntityHeader org, EntityHeader user);

        Task<bool> UserHasRoleAsync(string roleId, string userId, string orgId);
    }
}
