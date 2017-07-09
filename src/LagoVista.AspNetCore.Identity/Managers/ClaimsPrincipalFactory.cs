using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class ClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, Role>
    {

        IClaimsFactory _claimsFactory;

        public ClaimsPrincipalFactory(UserManager<AppUser> userManager, RoleManager<Role> roleManager, IClaimsFactory claimsFactory, IOptions<IdentityOptions> optionsAccessor) : base(userManager, roleManager, optionsAccessor)
        {
            _claimsFactory = claimsFactory;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(AppUser user)
        {
            var principal = await base.CreateAsync(user);

            ((ClaimsIdentity)principal.Identity).AddClaims(_claimsFactory.GetClaims(user).ToArray());

            return principal;
        }
    }
}
