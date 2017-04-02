using LagoVista.UserAdmin.Models.Account;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IRoleRepo : IDisposable
    {
        Task AddRoleAsync(String organizationId, Role role);
        Task InsertAsync(Role role);

        Task RemoveAsync(string id, string etag = "*");

        Task<Role> GetAsync(string id);

        Task UpdateAsync(Role role);
    }
}
