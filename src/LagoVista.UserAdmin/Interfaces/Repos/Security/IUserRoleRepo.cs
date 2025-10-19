// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 434f5f7d5557e72d4e05debf7731d6b649389b9ea2e20b4dfdc3dbcc449154dd
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IUserRoleRepo
    {
        Task AddUserRole(UserRole role);
        Task RemoveUserRole(string userRoleId, string organizationId);
        Task<UserRole> GetRoleAssignmentAsync(string userRoleId, string organizationId);
        Task<List<UserRole>> GetRolesForUserAsync(string userId, string organizationId);
    }
}
