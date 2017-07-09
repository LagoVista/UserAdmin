using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using LagoVista.Core;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class UserManager : IUserManager
    {
        private readonly UserManager<AppUser> _userManager;

        public UserManager(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<AppUser> FindByIdAsync(string id)
        {
            return _userManager.FindByIdAsync(id);
        }

        public Task<AppUser> FindByNameAsync(string userName)
        {
            return _userManager.FindByNameAsync(userName);
        }

        public async Task<InvokeResult> CreateAsync(AppUser user, string password)
        {
            return (await _userManager.CreateAsync(user, password)).ToInvokeResult();
        }

        public async Task<InvokeResult> UpdateAsync(AppUser appUser)
        {
            return (await _userManager.UpdateAsync(appUser)).ToInvokeResult();
        }       
    }
}
