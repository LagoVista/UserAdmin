using LagoVista.AspNetCore.Identity.Models;
using LagoVista.Core.Authentication.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using System;
using System.Collections.Generic;
using LagoVista.Core;
using System.Threading.Tasks;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Resources;
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.UserAdmin;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class RefreshTokenManager : IRefreshTokenManager
    {
        IRefreshTokenRepo _refreshTokenRepo;
        IAdminLogger _adminLogger;
        TokenAuthOptions _tokenOptions;
        IAuthRequestValidators _authRequestValidators;

        public RefreshTokenManager(TokenAuthOptions tokenOptions, IAuthRequestValidators authRequestValidators, IRefreshTokenRepo refreshTokenRepo, IAdminLogger adminLogger)
        {
            _refreshTokenRepo = refreshTokenRepo;
            _adminLogger = adminLogger;
            _tokenOptions = tokenOptions;
            _authRequestValidators = authRequestValidators;
        }

        public async Task<InvokeResult<RefreshToken>> GenerateRefreshTokenAsync(string appId, string appInstanceId, string userId)
        {
            var refreshToken = new RefreshToken(userId);
            refreshToken.RowKey = DateTime.UtcNow.ToInverseTicksRowKey();
            refreshToken.AppId = appId;
            refreshToken.AppInstanceId = appInstanceId;
            refreshToken.IssuedUtc = DateTime.UtcNow.ToJSONString();
            refreshToken.ExpiresUtc = (DateTime.UtcNow + _tokenOptions.RefreshExpiration).ToJSONString();
            await _refreshTokenRepo.SaveRefreshTokenAsync(refreshToken);

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "RefreshTokenManager_GenerateRefreshTokenAsync", "RefreshTokenGenerated",
                new KeyValuePair<string, string>("authAppId", appId),
                new KeyValuePair<string, string>("authAppInstanceId", appInstanceId),
                new KeyValuePair<string, string>("authUserId", userId));

            return InvokeResult<RefreshToken>.Create(refreshToken);
        }

        public Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenId, string userId)
        {
            return _refreshTokenRepo.GetRefreshTokenAsync(refreshTokenId, userId);
        }

        public async Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(RefreshToken oldRefreshToken)
        {
            if (oldRefreshToken == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_RenewRefreshTokenAsync", UserAdminErrorCodes.AuthMissingRefreshToken.Message);
                return InvokeResult<RefreshToken>.FromErrors(UserAdminErrorCodes.AuthMissingRefreshToken.ToErrorMessage());
            }

            var validateRefreshTokenResult = await _authRequestValidators.ValidateRefreshTokenAsync(oldRefreshToken.RowKey, oldRefreshToken.PartitionKey);
            if (!validateRefreshTokenResult.Successful)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_RenewRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenExpired.Message,
                    new KeyValuePair<string, string>("authAppId", oldRefreshToken.AppId),
                    new KeyValuePair<string, string>("authAppInstanceId", oldRefreshToken.AppInstanceId),
                    new KeyValuePair<string, string>("authUserId", oldRefreshToken.PartitionKey));

                await _refreshTokenRepo.RemoveRefreshTokenAsync(oldRefreshToken.RowKey, oldRefreshToken.PartitionKey);
                return InvokeResult<RefreshToken>.FromErrors(UserAdminErrorCodes.AuthRefreshTokenExpired.ToErrorMessage());
            }

            var newRefreshToken = await GenerateRefreshTokenAsync(oldRefreshToken.AppId, oldRefreshToken.AppInstanceId, oldRefreshToken.PartitionKey);
            if (newRefreshToken.Successful)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "RefreshTokenManager_RenewRefreshTokenAsync", "RefrehTokenRenewed",
                    new KeyValuePair<string, string>("authAppId", oldRefreshToken.AppId),
                    new KeyValuePair<string, string>("authAppInstanceId", oldRefreshToken.AppInstanceId),
                    new KeyValuePair<string, string>("authUserId", oldRefreshToken.PartitionKey));

                await _refreshTokenRepo.RemoveRefreshTokenAsync(oldRefreshToken.RowKey, oldRefreshToken.PartitionKey);
            }

            return newRefreshToken;
        }

        public async Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(string refreshTokenId, string userId)
        {
            var refreshToken = await GetRefreshTokenAsync(refreshTokenId, userId);
            if (refreshToken == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_RenewRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenNotInStorage.Message, 
                    new KeyValuePair<string, string>("refreshtokenid", refreshTokenId),
                    new KeyValuePair<string, string>("authUserId", userId));
                return InvokeResult<RefreshToken>.FromErrors(UserAdminErrorCodes.AuthRefreshTokenNotInStorage.ToErrorMessage());
            }

            return await RenewRefreshTokenAsync(refreshToken);
        }

        public Task RevokeAllForUserAsync(string userId)
        {
            return _refreshTokenRepo.RemoveAllForUserAsync(userId);
        }

        public Task RevokeRefreshTokenAsync(string refreshTokenId, string userId)
        {
            return _refreshTokenRepo.RemoveRefreshTokenAsync(refreshTokenId, userId);
        }
    }
}