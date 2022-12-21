using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface ITokenHelper
    {
        Task<InvokeResult<AuthResponse>> GenerateAuthResponseAsync(AppUser appUser, AuthRequest authRequest, InvokeResult<RefreshToken> refreshTokenResponse);
        Task<InvokeResult<AuthResponse>> GenerateAuthResponseAsync(AppUser appUser, string appInstanceId, InvokeResult<RefreshToken> refreshTokenResponse);
        string GetJWToken(AppUser user, DateTime accessExpires, string installationId);

        string NonceGenerator();
    }
}
