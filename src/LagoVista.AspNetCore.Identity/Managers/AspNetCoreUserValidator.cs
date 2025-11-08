// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2db77e6f67ddcd8ed16d1ffe7ddff499f644e12813b21b20b216856a1fe945b5
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Users;
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
