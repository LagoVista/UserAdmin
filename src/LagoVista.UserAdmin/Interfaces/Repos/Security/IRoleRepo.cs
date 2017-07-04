using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IRoleRepo : IDisposable
    {
        Task AddRoleAsync(String orgId, Role role);
        Task InsertAsync(Role role);

        Task RemoveAsync(string id, string etag = "*");

        Task<Role> GetAsync(string id);

        Task UpdateAsync(Role role);
    }
}
