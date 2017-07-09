using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IRefreshTokenManager
    {
        Task<InvokeResult<RefreshToken>> GenerateRefreshTokenAsync(string appId, string appInstanceId, string userId);

        Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(RefreshToken oldRefreshToken);

        Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(String refreshTokenId, string userId);

        Task RevokeRefreshTokenAsync(string refreshTokenId, string userId);

        Task RevokeAllForUserAsync(string userId);
    }
}
