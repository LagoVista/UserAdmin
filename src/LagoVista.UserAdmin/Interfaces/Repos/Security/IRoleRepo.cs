// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6e2d966795d1a8fa796cb6a2db55b03d45768b9b7a3aa14f49784e1db2e41e49
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IRoleRepo 
    {
        Task AddRoleAsync(Role role);
        Task InsertAsync(Role role);
        Task RemoveAsync(string id);
        Task<Role> GetRoleAsync(string id);
        Task<Role> GetRoleByKeyAsync(string key, string orgId);
        Task UpdateAsync(Role role);
        Task<List<RoleSummary>> GetRolesAsync(string orgId);
        Task<List<RoleSummary>> GetAssignableRolesAsync(string orgId);
    }
}
