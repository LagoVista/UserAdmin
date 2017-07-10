using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;

namespace LagoVista.UserAdmin
{
    public interface ITokenHelper
    {
        InvokeResult<AuthResponse> GenerateAuthResponse(AppUser appUser, AuthRequest authRequest, InvokeResult<RefreshToken> refreshTokenResponse);

        string GetJWToken(AppUser user, DateTime accessExpires, string installationId);

        string NonceGenerator();
    }
}
