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
using System.Text.RegularExpressions;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class RefreshTokenManager : IRefreshTokenManager
    {
        IRefreshTokenRepo _refreshTokenRepo;
        IAdminLogger _adminLogger;
        TokenAuthOptions _tokenOptions;

        private const string REFRESH_TOKEN_FORMAT = @"[0-9]{19,19}\.[0-9A-F]{32,32}";

        public RefreshTokenManager(TokenAuthOptions tokenOptions, IRefreshTokenRepo refreshTokenRepo, IAdminLogger adminLogger)
        {
            _refreshTokenRepo = refreshTokenRepo;
            _adminLogger = adminLogger;
            _tokenOptions = tokenOptions;
        }

        public async Task<InvokeResult<RefreshToken>> GenerateRefreshTokenAsync(string appId, string clientId, string userId)
        {
            var refreshToken = new RefreshToken(userId);
            refreshToken.RowKey = DateTime.UtcNow.ToInverseTicksRowKey();
            refreshToken.AppId = appId;
            refreshToken.ClientId = clientId;
            refreshToken.IssuedUtc = DateTime.UtcNow.ToJSONString();
            refreshToken.ExpiresUtc = (DateTime.UtcNow + _tokenOptions.RefreshExpiration).ToJSONString();
            await _refreshTokenRepo.SaveRefreshTokenAsync(refreshToken);
            return InvokeResult<RefreshToken>.Create(refreshToken);
        }

        public Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenId, string userId)
        {
            return _refreshTokenRepo.GetRefreshTokenAsync(refreshTokenId, userId);
        }

        public async Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(RefreshToken oldRefreshToken)
        {
            if(oldRefreshToken == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_RenewRefreshTokenAsync", UserAdminErrorCodes.AuthMissingRefreshToken.Message);
                return InvokeResult<RefreshToken>.FromErrors(UserAdminErrorCodes.AuthMissingRefreshToken.ToErrorMessage());
            }

            var refreshToken = new RefreshToken(oldRefreshToken.PartitionKey);
            if (refreshToken.ExpiresUtc.ToDateTime() < DateTime.UtcNow)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_RenewRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenExpired.Message);
                await _refreshTokenRepo.RemoveRefreshTokenAsync(oldRefreshToken.RowKey, oldRefreshToken.PartitionKey);
                return InvokeResult<RefreshToken>.FromErrors(UserAdminErrorCodes.AuthRefreshTokenExpired.ToErrorMessage());
            }

            await _refreshTokenRepo.RemoveRefreshTokenAsync(oldRefreshToken.RowKey, oldRefreshToken.PartitionKey);
            return await GenerateRefreshTokenAsync(oldRefreshToken.AppId, oldRefreshToken.PartitionKey, oldRefreshToken.ClientId);
        }

        public async Task<InvokeResult<RefreshToken>> RenewRefreshTokenAsync(String refreshTokenId, string userId)
        {
            var refreshToken = await GetRefreshTokenAsync(refreshTokenId, userId);
            if (refreshToken == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_RenewRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenNotInStorage.Message, new KeyValuePair<string, string>("refreshtokenid",refreshTokenId));
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

        public async Task<InvokeResult> ValidateRefreshTokenAsync(string refreshTokenId, string userId)
        {
            var validateFormatResult = ValidateTokenFormat(refreshTokenId);
            if (!validateFormatResult.Successful) return validateFormatResult;

            var token = await _refreshTokenRepo.GetRefreshTokenAsync(refreshTokenId, userId);
            if (token == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_ValidateRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenNotInStorage.Message, new KeyValuePair<string, string>("refreshtokenid", refreshTokenId));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthRefreshTokenNotInStorage.ToErrorMessage());
            }

            if (token.ExpiresUtc.ToDateTime() < DateTime.UtcNow)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_ValidateRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenExpired.Message);
                await _refreshTokenRepo.RemoveRefreshTokenAsync(refreshTokenId, userId);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthRefreshTokenExpired.ToErrorMessage());
            }
            return InvokeResult.Success;
        }

        public InvokeResult ValidateTokenFormat(string refreshToken)
        {
            var regEx = new Regex(REFRESH_TOKEN_FORMAT);
            if (!regEx.Match(refreshToken).Success)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "RefreshTokenManager_ValidateTokenFormat", UserAdminErrorCodes.AuthRefrshTokenInvalidFormat.Message, new KeyValuePair<string, string>("token",refreshToken));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthRefrshTokenInvalidFormat.ToErrorMessage());
            }

            return InvokeResult.Success;
        }
    }
}
