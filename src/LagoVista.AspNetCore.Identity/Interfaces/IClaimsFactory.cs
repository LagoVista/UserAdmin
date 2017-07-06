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
    }
}
