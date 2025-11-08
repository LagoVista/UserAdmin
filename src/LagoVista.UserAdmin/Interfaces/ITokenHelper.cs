// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ba0ea98b32ba5474b93df07c530ec167e28f70a5c804aa8499113fe3a8966f6d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
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
