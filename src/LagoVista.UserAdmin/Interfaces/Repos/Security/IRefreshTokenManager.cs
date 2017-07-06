using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IRefreshTokenManager
    {
        Task<InvokeResult<RefreshToken>> GenerateRefreshTokenAsync(string appId, string clientId, string userId);

        Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(RefreshToken oldRefreshToken);

        Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(String refreshTokenId, string userId);

        Task<InvokeResult> ValidateRefreshTokenAsync(string refreshTokenId, string userId);

        Task RevokeRefreshTokenAsync(string refreshTokenId, string userId);

        Task RevokeAllForUserAsync(string userId);

        InvokeResult ValidateTokenFormat(string refreshToken);
    }
}
