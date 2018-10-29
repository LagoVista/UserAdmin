using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Resources;
using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.Core.Interfaces;

namespace LagoVista.AspNetCore.Identity.Utils
{
    public class AuthRequestValidators : IAuthRequestValidators
    {
        IAdminLogger _adminLogger;
        IUserManager _userManager;
        IRefreshTokenRepo _refreshTokenRepo;

        private const string EMAIL_REGEX_FORMAT = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        private const string REFRESH_TOKEN_FORMAT = @"[0-9]{19,19}\.[0-9A-F]{32,32}";

        public AuthRequestValidators(IAdminLogger adminLogger, IRefreshTokenRepo refreshTokenRepo, IUserManager userManager)
        {
            _adminLogger = adminLogger;
            _userManager = userManager;
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

            if (String.IsNullOrEmpty(authRequest.Email))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAccessTokenGrant", UserAdminErrorCodes.AuthMissingEmail.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingEmail.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.UserName))
            {
                authRequest.UserName = authRequest.Email;
            }

            switch (authRequest.AuthType)
            {
                case AuthTypes.ClientApp:
                    break;
                case AuthTypes.DeviceUser:
                    if (String.IsNullOrEmpty(authRequest.DeviceRepoId))
                    {
                        _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateAccessTokenGrant", UserAdminErrorCodes.AuthMissingRepoIdForDeviceUser.Message);
                        return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingRepoIdForDeviceUser.ToErrorMessage());
                    }
                    break;
                case AuthTypes.Runtime:
                    break;
                case AuthTypes.User:
                    break;
            }

            return InvokeResult.Success;
        }

        public InvokeResult ValidateAccessTokenGrant(AuthRequest authRequest)
        {
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

        public InvokeResult ValidatePasswordChangeRequest(ChangePassword changePassword, string userId)
        {
            if (String.IsNullOrEmpty(changePassword.UserId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidatePasswordChangeRequest", UserAdminResources.Err_PwdChange_MissingUserId);
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_MissingUserId));
            }

            if (changePassword.UserId != userId)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidatePasswordChangeRequest", UserAdminResources.Err_PwdChange_UserIdMismatch,
                    new KeyValuePair<string, string>("requestUseId", changePassword.UserId),
                     new KeyValuePair<string, string>("currentUserId", userId));
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_UserIdMismatch));
            }

            if (String.IsNullOrEmpty(changePassword.OldPassword))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidatePasswordChangeRequest", UserAdminResources.Err_PwdChange_OldPassword_Missing);
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_OldPassword_Missing));
            }

            if (String.IsNullOrEmpty(changePassword.NewPassword))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidatePasswordChangeRequest", UserAdminResources.Err_PwdChange_NewPassword_Missing);
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_NewPassword_Missing));
            }

            return InvokeResult.Success;
        }

        public InvokeResult ValidateSendPasswordLinkRequest(SendResetPasswordLink sendRestPasswordLink)
        {
            if (String.IsNullOrEmpty(sendRestPasswordLink.Email))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateSendPasswordLinkRequest", UserAdminErrorCodes.RegMissingEmail.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.RegMissingEmail.ToErrorMessage());
            }

            var emailRegEx = new Regex(EMAIL_REGEX_FORMAT);
            if (!emailRegEx.Match(sendRestPasswordLink.Email).Success)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateSendPasswordLinkRequest", UserAdminErrorCodes.RegInvalidEmailAddress.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.RegInvalidEmailAddress.ToErrorMessage());
            }

            return InvokeResult.Success;
        }

        public InvokeResult ValidateResetPasswordRequest(ResetPassword resetPassword)
        {
            if (String.IsNullOrEmpty(resetPassword.Email))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateResetPasswordRequest", UserAdminErrorCodes.RegMissingEmail.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.RegMissingEmail.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(resetPassword.Token))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateResetPasswordRequest", UserAdminResources.Err_PwdChange_Token_Missing);
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_Token_Missing));
            }

            if (String.IsNullOrEmpty(resetPassword.NewPassword))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateResetPasswordRequest", UserAdminResources.Err_PwdChange_NewPassword_Missing);
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_NewPassword_Missing));
            }

            var emailRegEx = new Regex(EMAIL_REGEX_FORMAT);
            if (!emailRegEx.Match(resetPassword.Email).Success)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthRequestValidators_ValidateResetPasswordRequest", UserAdminErrorCodes.RegInvalidEmailAddress.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.RegInvalidEmailAddress.ToErrorMessage());
            }

            return InvokeResult.Success;

        }
    }
}