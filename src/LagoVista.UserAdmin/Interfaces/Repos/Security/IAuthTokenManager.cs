// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b8ded12637c139980886d992097bf8250d135a495868c911940f615fd8c5f75f
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IAuthTokenManager
    {
        Task<InvokeResult<AuthResponse>> AccessTokenGrantAsync(AuthRequest authRequest);

        Task<InvokeResult<AuthResponse>> RefreshTokenGrantAsync(AuthRequest authRequest);
        Task<InvokeResult<SingleUseToken>> GenerateOneTimeUseTokenAsync(string userId, TimeSpan? expires = null);
        Task<InvokeResult<AuthResponse>> SingleUseTokenGrantAsync(AuthRequest authRequest);
    }
}
