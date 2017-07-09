using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LagoVista.AspNetCore.Identity.Utils
{
    public class AuthRequestValidators : IAuthRequestValidators
    {
        IAdminLogger _adminLogger;
        private const string EMAIL_REGEX_FORMAT = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";

        public AuthRequestValidators(IAdminLogger adminLogger)
        {
            _adminLogger = adminLogger;
        }

        public InvokeResult ValidateAuthRequest(AuthRequest authRequest)
        {
            if (authRequest == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthRequestNull.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthRequestNull.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.AppId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingAppId.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingAppId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.DeviceId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingDeviceId.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingDeviceId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.ClientType))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingClientType.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingClientType.ToErrorMessage());
            }

            return InvokeResult.Success;
        }

        public InvokeResult ValidateAccessTokenGrant(AuthRequest authRequest)
        {
            if (String.IsNullOrEmpty(authRequest.Email))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingEmail.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingEmail.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.Password))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingPassword.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingPassword.ToErrorMessage());
            }

            var emailRegEx = new Regex(EMAIL_REGEX_FORMAT);
            if (!emailRegEx.Match(authRequest.Email).Success)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthEmailInvalidFormat.Message, new KeyValuePair<string, string>("email", authRequest.Email));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthEmailInvalidFormat.ToErrorMessage());
            }

            return InvokeResult.Success;
        }

        public InvokeResult ValidateRefreshTokenGrant(AuthRequest authRequest)
        {
            if (String.IsNullOrEmpty(authRequest.RefreshToken))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingRefreshToken.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingRefreshToken.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.AppInstanceId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthMissingAppInstanceId.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingAppInstanceId.ToErrorMessage());
            }

            return InvokeResult.Success;
        }
    }
}
