using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IRoleRepo : IDisposable
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
