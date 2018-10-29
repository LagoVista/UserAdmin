using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin;
using LagoVista.Core;
using LagoVista.Core.Interfaces;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AuthTokenManager : IAuthTokenManager
    {
        IRefreshTokenManager _refreshTokenManager;
        IAdminLogger _adminLogger;
        ISignInManager _signInManager;
        IUserManager _userManager;
        ITokenHelper _tokenHelper;
        IAppInstanceManager _appInstanceManager;
        IAuthRequestValidators _authRequestValidators;
        IOrgHelper _orgHelper;

        public const string GRANT_TYPE_PASSWORD = "password";
        public const string GRANT_TYPE_REFRESHTOKEN = "refreshtoken";

        public AuthTokenManager(IAppInstanceRepo appInstanceRepo,
                                IRefreshTokenManager refreshTokenManager, IAuthRequestValidators authRequestValidators, IOrgHelper orgHelper,
                                ITokenHelper tokenHelper, IAppInstanceManager appInstanceManager,
                                IAdminLogger adminLogger, ISignInManager signInManager, IUserManager userManager)
        {
            _refreshTokenManager = refreshTokenManager;
            _adminLogger = adminLogger;
            _orgHelper = orgHelper;
            _tokenHelper = tokenHelper;
            _signInManager = signInManager;
            _userManager = userManager;
            _authRequestValidators = authRequestValidators;
            _appInstanceManager = appInstanceManager;
        }


        /// <summary>
        /// Validate the core properties of the request.
        /// </summary>
        /// <param name="authRequest"></param>
        /// <returns></returns>
        public async Task<InvokeResult<AuthResponse>> AccessTokenGrantAsync(AuthRequest authRequest)
        {
            var requestValidationResult = _authRequestValidators.ValidateAuthRequest(authRequest);
            if (!requestValidationResult.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(requestValidationResult);

            var accessTokenRequestValidationResult = _authRequestValidators.ValidateAccessTokenGrant(authRequest);
            if (!accessTokenRequestValidationResult.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(accessTokenRequestValidationResult);

            var userName = authRequest.UserName;
            var password = authRequest.Password;

            

            switch (authRequest.AuthType)
            {
                case AuthTypes.DeviceUser:
                    userName = $"{authRequest.DeviceRepoId}-{authRequest.UserName}";
                    break;
                case AuthTypes.ClientApp:
                    userName = $"{authRequest.AppId}-{authRequest.UserName}";
                    break;
                case AuthTypes.Runtime:
                    userName = authRequest.InstanceId;
                    password = authRequest.InstanceAuthKey;
                    break;
            }

            var signInRequest = await _signInManager.PasswordSignInAsync(userName, password, true, false);
            if (!signInRequest.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(signInRequest);

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "AuthTokenManager_AccessTokenGrantAsync", "UserLoggedIn", new KeyValuePair<string, string>("email", userName));

            var appUser = await _userManager.FindByNameAsync(userName);
            if (appUser == null)
            {
                /* Should really never, ever happen, but well...let's track it */
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("email", userName));
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(authRequest.AppInstanceId))
            {
                /* This generally happens for the first time the app is logged in on a new device, if it is logged in again future times it will resend the app id */
                var appInstanceResult = await _appInstanceManager.CreateForUserAsync(appUser.Id, authRequest);
                authRequest.AppInstanceId = appInstanceResult.Result.RowKey;
            }
            else
            {
                var updateLastLoginResult = (await _appInstanceManager.UpdateLastLoginAsync(appUser.Id, authRequest));
                if(updateLastLoginResult.Successful)
                {
                    authRequest.AppInstanceId = updateLastLoginResult.Result.RowKey;
                }
                else
                {
                    return InvokeResult<AuthResponse>.FromInvokeResult(updateLastLoginResult.ToInvokeResult());
                }
            }

            var refreshTokenResponse = await _refreshTokenManager.GenerateRefreshTokenAsync(authRequest.AppId, authRequest.AppInstanceId, appUser.Id);
            var authResponse = _tokenHelper.GenerateAuthResponse(appUser, authRequest, refreshTokenResponse);
            return authResponse;
        }


        public async Task<InvokeResult<AuthResponse>> RefreshTokenGrantAsync(AuthRequest authRequest)
        {
            var requestValidationResult = _authRequestValidators.ValidateAuthRequest(authRequest);
            if (!requestValidationResult.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(requestValidationResult);

            var userName = authRequest.UserName;
            switch (authRequest.AuthType)
            {
                case AuthTypes.DeviceUser:
                    userName = $"{authRequest}-{authRequest.UserName}";
                    break;
                case AuthTypes.ClientApp:
                    userName = $"{authRequest}-{authRequest.UserName}";
                    break;
                case AuthTypes.Runtime:
                    userName = authRequest.InstanceId;
                    break;
            }

            var refreshTokenRequestValidationResult = _authRequestValidators.ValidateRefreshTokenGrant(authRequest);
            if (!refreshTokenRequestValidationResult.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(refreshTokenRequestValidationResult);

            var appUser = await _userManager.FindByNameAsync(authRequest.UserName);
            if (appUser == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_RefreshTokenGrantAsync", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("id", userName));
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            if (!String.IsNullOrEmpty(authRequest.OrgId) && (appUser.CurrentOrganization == null || authRequest.OrgId != appUser.CurrentOrganization.Id))
            {
                var changeOrgResult = await _orgHelper.SetUserOrgAsync(authRequest, appUser);
                if (!changeOrgResult.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(changeOrgResult);
            }

            var updateLastRefreshTokenResult = (await _appInstanceManager.UpdateLastAccessTokenRefreshAsync(appUser.Id, authRequest));
            if (updateLastRefreshTokenResult.Successful)
            {
                authRequest.AppInstanceId = updateLastRefreshTokenResult.Result.RowKey;
                var refreshTokenResponse = await _refreshTokenManager.RenewRefreshTokenAsync(authRequest.RefreshToken, appUser.Id);
                _adminLogger.LogInvokeResult("AuthTokenManager_RefreshTokenGrantAsync", refreshTokenResponse);
                return _tokenHelper.GenerateAuthResponse(appUser, authRequest, refreshTokenResponse);
            }
            else
            {
                return InvokeResult<AuthResponse>.FromInvokeResult(updateLastRefreshTokenResult.ToInvokeResult());
            }
        }
    }
}