// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7363cade23c0f7bee5bfd3352bce334d53a3887ea9583067c7789f84512d08cf
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Apps;
using RingCentral;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class RefreshTokenManager : IRefreshTokenManager
    {
        IRefreshTokenRepo _refreshTokenRepo;
        IAdminLogger _adminLogger;
        TokenAuthOptions _tokenOptions;
        IAuthRequestValidators _authRequestValidators;
        IAuthenticationLogManager _authLogManager;

        public RefreshTokenManager(TokenAuthOptions tokenOptions, IAuthenticationLogManager authLogManager, IAuthRequestValidators authRequestValidators, IRefreshTokenRepo refreshTokenRepo, IAdminLogger adminLogger)
        {
            _refreshTokenRepo = refreshTokenRepo;
            _adminLogger = adminLogger;
            _tokenOptions = tokenOptions;
            _authRequestValidators = authRequestValidators;
            _authLogManager = authLogManager;
        }

        public async Task<InvokeResult<RefreshToken>> GenerateRefreshTokenAsync(string appId, string appInstanceId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException(nameof(appId));
                if (string.IsNullOrEmpty(appInstanceId)) throw new ArgumentNullException(nameof(appInstanceId));
                if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.GenerateRefreshToken, userId: userId);

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


                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.GenerateRefreshTokenSuccess, userId: userId);

                return InvokeResult<RefreshToken>.Create(refreshToken);
            }
            catch(Exception ex)
            {
                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.GenerateRefreshTokenFailed, userId: userId);
                _adminLogger.AddException("[RefreshTokenManager__GenerateRefreshTokenAsync]", ex, new KeyValuePair<string, string>("authAppId", appId),
                    new KeyValuePair<string, string>("authAppInstanceId", appInstanceId),
                    new KeyValuePair<string, string>("authUserId", userId));
                return InvokeResult<RefreshToken>.FromException("[RefreshTokenManager__GenerateRefreshTokenAsync]", ex);
            }
        }

        public Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenId, string userId)
        {
            return _refreshTokenRepo.GetRefreshTokenAsync(refreshTokenId, userId);
        }

        public async Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(RefreshToken oldRefreshToken)
        {
            try
            {
                if (oldRefreshToken == null)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_RenewRefreshTokenAsync", UserAdminErrorCodes.AuthMissingRefreshToken.Message);

                    await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RenewRefreshTokenFailed, userId: "?", errors:"Old Refresh Token Null.");

                    return InvokeResult<RefreshToken>.FromErrors(UserAdminErrorCodes.AuthMissingRefreshToken.ToErrorMessage());
                }

                var validateRefreshTokenResult = await _authRequestValidators.ValidateRefreshTokenAsync(oldRefreshToken.RowKey, oldRefreshToken.PartitionKey);
                if (!validateRefreshTokenResult.Successful)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_RenewRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenExpired.Message,
                        new KeyValuePair<string, string>("authAppId", oldRefreshToken.AppId),
                        new KeyValuePair<string, string>("authAppInstanceId", oldRefreshToken.AppInstanceId),
                        new KeyValuePair<string, string>("authUserId", oldRefreshToken.PartitionKey));

                    await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RenewRefreshTokenFailed, userId: oldRefreshToken.PartitionKey, errors: $"Could not validate existing refresh token: {validateRefreshTokenResult.ErrorMessage}");

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

                    await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RenewRefreshTokenSuccess, userId: oldRefreshToken.PartitionKey, extras: $"AppId: {oldRefreshToken.AppId}");

                    await _refreshTokenRepo.RemoveRefreshTokenAsync(oldRefreshToken.RowKey, oldRefreshToken.PartitionKey);
                }
                else
                {
                    await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RenewRefreshTokenFailed, userId: oldRefreshToken.PartitionKey, errors: $"Could not validate existing refresh token: {validateRefreshTokenResult.ErrorMessage}; AppId: {oldRefreshToken.AppId}");
                }

                return newRefreshToken;
            }
            catch(Exception ex)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "[RefreshTokenManager_RenewRefreshTokenAsync]", "RefrehTokenRenewed Failed",
                      new KeyValuePair<string, string>("authAppId", oldRefreshToken.AppId),
                      new KeyValuePair<string, string>("authAppInstanceId", oldRefreshToken.AppInstanceId),
                      new KeyValuePair<string, string>("authUserId", oldRefreshToken.PartitionKey));

                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RenewRefreshTokenFailed, userId: oldRefreshToken.PartitionKey, errors: ex.Message, extras: $"RowKey: {oldRefreshToken.RowKey} AppId: {oldRefreshToken.AppId}");

                return InvokeResult<RefreshToken>.FromException("[RefreshTokenManager_RenewRefreshTokenAsync]",ex);
            }
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