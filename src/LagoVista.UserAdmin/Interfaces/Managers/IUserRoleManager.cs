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
    }
}
