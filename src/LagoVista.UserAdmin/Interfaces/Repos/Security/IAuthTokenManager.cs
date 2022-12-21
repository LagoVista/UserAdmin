using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
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
