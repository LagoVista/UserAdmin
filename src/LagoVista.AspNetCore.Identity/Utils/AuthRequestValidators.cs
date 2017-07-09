using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Resources;
using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Utils
{
    public class AuthRequestValidators : IAuthRequestValidators
    {
        IAdminLogger _adminLogger;
        IRefreshTokenRepo _refreshTokenRepo;

        private const string EMAIL_REGEX_FORMAT = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        private const string REFRESH_TOKEN_FORMAT = @"[0-9]{19,19}\.[0-9A-F]{32,32}";

        public AuthRequestValidators(IAdminLogger adminLogger, IRefreshTokenRepo refreshTokenRepo)
        {
            _adminLogger = adminLogger;
            _refreshTokenRepo = refreshTokenRepo;
        }

        public InvokeResult ValidateAuthRequest(AuthRequest authRequest)
        {
            if (authRequest == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAuthRequest", UserAdminErrorCodes.AuthRequestNull.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthRequestNull.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.AppId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAuthRequest", UserAdminErrorCodes.AuthMissingAppId.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingAppId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.DeviceId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAuthRequest", UserAdminErrorCodes.AuthMissingDeviceId.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingDeviceId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.ClientType))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAuthRequest", UserAdminErrorCodes.AuthMissingClientType.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingClientType.ToErrorMessage());
            }

            return InvokeResult.Success;
        }

        public InvokeResult ValidateAccessTokenGrant(AuthRequest authRequest)
        {
            if (String.IsNullOrEmpty(authRequest.Email))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAccessTokenGrant", UserAdminErrorCodes.AuthMissingEmail.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingEmail.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.Password))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAccessTokenGrant", UserAdminErrorCodes.AuthMissingPassword.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingPassword.ToErrorMessage());
            }

            var emailRegEx = new Regex(EMAIL_REGEX_FORMAT);
            if (!emailRegEx.Match(authRequest.Email).Success)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAccessTokenGrant", UserAdminErrorCodes.AuthEmailInvalidFormat.Message, new KeyValuePair<string, string>("email", authRequest.Email));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthEmailInvalidFormat.ToErrorMessage());
            }

            return InvokeResult.Success;
        }

        public InvokeResult ValidateRefreshTokenGrant(AuthRequest authRequest)
        {
            if (String.IsNullOrEmpty(authRequest.RefreshToken))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateRefreshTokenGrant", UserAdminErrorCodes.AuthMissingRefreshToken.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingRefreshToken.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.AppInstanceId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateRefreshTokenGrant", UserAdminErrorCodes.AuthMissingAppInstanceId.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingAppInstanceId.ToErrorMessage());
            }

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> ValidateRefreshTokenAsync(string refreshTokenId, string userId)
        {
            var validateFormatResult = ValidateRefreshTokenFormat(refreshTokenId);
            if (!validateFormatResult.Successful) return validateFormatResult;

            var token = await _refreshTokenRepo.GetRefreshTokenAsync(refreshTokenId, userId);
            if (token == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenNotInStorage.Message, new KeyValuePair<string, string>("refreshtokenid", refreshTokenId));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthRefreshTokenNotInStorage.ToErrorMessage());
            }

            if (token.ExpiresUtc.ToDateTime() < DateTime.UtcNow)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateRefreshTokenAsync", UserAdminErrorCodes.AuthRefreshTokenExpired.Message);
                await _refreshTokenRepo.RemoveRefreshTokenAsync(refreshTokenId, userId);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthRefreshTokenExpired.ToErrorMessage());
            }
            return InvokeResult.Success;
        }

        public InvokeResult ValidateRefreshTokenFormat(string refreshToken)
        {
            var regEx = new Regex(REFRESH_TOKEN_FORMAT);
            if (!regEx.Match(refreshToken).Success)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateRefreshTokenFormat", UserAdminErrorCodes.AuthRefrshTokenInvalidFormat.Message, new KeyValuePair<string, string>("token", refreshToken));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthRefrshTokenInvalidFormat.ToErrorMessage());
            }

            return InvokeResult.Success;
        }
    }
}