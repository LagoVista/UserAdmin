// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6ee508c9424fbd71f843ac31c322cf2f87bcd78afa4ad5e0cf28172c5dd6f5e1
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

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
