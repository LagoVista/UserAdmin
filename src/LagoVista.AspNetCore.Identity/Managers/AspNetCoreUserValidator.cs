using LagoVista.UserAdmin.Models.Account;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AspNetCoreUserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            return Task.FromResult(IdentityResult.Success);
        }
    }
}
