using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface ITokenHelper
    {
        InvokeResult<AuthResponse> GenerateAuthResponse(AppUser appUser, AuthRequest authRequest, InvokeResult<RefreshToken> refreshTokenResponse);

        string GetJWToken(AppUser user, DateTime accessExpires, string installationId);

        string NonceGenerator();
    }
}
