using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Security.Claims;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IClaimsFactory
    {
        List<Claim> GetClaims(AppUser user);
        List<Claim> GetClaims(AppUser user, EntityHeader org, bool isOrgAdmin, bool isAppBuilder);
        List<Claim> GetClaimsForDeviceOwner(AppUser pinAuthUser);
        List<Claim> GetClaimsForCustomer(AppUser pinAuthUser);
    }
}
