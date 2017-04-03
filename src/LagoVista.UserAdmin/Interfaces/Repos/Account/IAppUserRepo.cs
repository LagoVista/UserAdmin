using LagoVista.UserAdmin.Models.Account;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Account
{
    public interface IAppUserRepo : IDisposable
    {
        Task CreateAsync(AppUser user);
        Task<AppUser> FindByIdAsync(string userId);
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByEmailAsync(string email);
        Task<AppUser> FindByThirdPartyLogin(string providerId, string providerKey);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(AppUser user);
    }
}
