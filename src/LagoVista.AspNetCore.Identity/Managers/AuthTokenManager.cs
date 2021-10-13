using LagoVista.Core.Authentication.Models;
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
using LagoVista.Core.Interfaces;
using System.Linq;
using LagoVista.Core.Models;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AuthTokenManager : IAuthTokenManager
    {
        private readonly IRefreshTokenManager _refreshTokenManager;
        private readonly IAdminLogger _adminLogger;
        private readonly ISignInManager _signInManager;
        private readonly IUserManager _userManager;
        private readonly ITokenHelper _tokenHelper;
        private readonly IAppInstanceManager _appInstanceManager;
        private readonly IAuthRequestValidators _authRequestValidators;
        private readonly IOrganizationManager _orgManager;

        public const string GRANT_TYPE_PASSWORD = "password";
        public const string GRANT_TYPE_REFRESHTOKEN = "refreshtoken";

        public AuthTokenManager(IAppInstanceRepo appInstanceRepo, IOrganizationManager orgManager,
                                IRefreshTokenManager refreshTokenManager, IAuthRequestValidators authRequestValidators,
                                ITokenHelper tokenHelper, IAppInstanceManager appInstanceManager,
                                IAdminLogger adminLogger, ISignInManager signInManager, IUserManager userManager)
        {
            _refreshTokenManager = refreshTokenManager;
            _adminLogger = adminLogger;
            _tokenHelper = tokenHelper;
            _signInManager = signInManager;
            _userManager = userManager;
            _orgManager = orgManager;
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

            if (appUser.IsAccountDisabled)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", "UserLogin Failed - Account Disabled", new KeyValuePair<string, string>("email", userName));
                return InvokeResult<AuthResponse>.FromError("Account Disabled.");
            }

            if (!string.IsNullOrEmpty(authRequest.OrgId))
            {
                if (EntityHeader.IsNullOrEmpty(appUser.CurrentOrganization))
                {
                    return InvokeResult<AuthResponse>.FromError($"Sorry, you do not have access to the {authRequest.OrgName} organization.", "NOORGACESS");
                }
                else
                {
                    var userOrgs = await _orgManager.GetOrganizationsForUserAsync(appUser.Id, appUser.ToEntityHeader(), appUser.CurrentOrganization);
                    if (!userOrgs.Where(org => org.OrgId == authRequest.OrgId).Any())
                        return InvokeResult<AuthResponse>.FromError($"User does not have access to {authRequest.OrgName}");
                }
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
                if (updateLastLoginResult.Successful)
                {
                    authRequest.AppInstanceId = updateLastLoginResult.Result.RowKey;
                }
                else
                {
                    return InvokeResult<AuthResponse>.FromInvokeResult(updateLastLoginResult.ToInvokeResult());
                }
            }

            var refreshTokenResponse = await _refreshTokenManager.GenerateRefreshTokenAsync(authRequest.AppId, authRequest.AppInstanceId, appUser.Id);
            var authResponse = await _tokenHelper.GenerateAuthResponseAsync(appUser, authRequest, refreshTokenResponse);
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

            if (appUser.IsAccountDisabled)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_RefreshTokenGrantAsync", "UserLogin Failed - Account Disabled", new KeyValuePair<string, string>("email", userName));
                return InvokeResult<AuthResponse>.FromError("Account Disabled.");
            }

            if (!String.IsNullOrEmpty(authRequest.OrgId))
            {
                if (EntityHeader.IsNullOrEmpty(appUser.CurrentOrganization))
                {
                    return InvokeResult<AuthResponse>.FromError($"User does not have access to {authRequest.OrgName}");
                }
                else
                {
                    var userOrgs = await _orgManager.GetOrganizationsForUserAsync(appUser.Id, appUser.ToEntityHeader(), appUser.CurrentOrganization);
                    if (!userOrgs.Where(org => org.OrgId == authRequest.OrgId).Any())
                        return InvokeResult<AuthResponse>.FromError($"Sorry, you do not have access to the {authRequest.OrgName} organization.", "NOORGACESS");
                }
            }

            var updateLastRefreshTokenResult = (await _appInstanceManager.UpdateLastAccessTokenRefreshAsync(appUser.Id, authRequest));
            if (updateLastRefreshTokenResult.Successful)
            {
                authRequest.AppInstanceId = updateLastRefreshTokenResult.Result.RowKey;
                var refreshTokenResponse = await _refreshTokenManager.RenewRefreshTokenAsync(authRequest.RefreshToken, appUser.Id);
                _adminLogger.LogInvokeResult("AuthTokenManager_RefreshTokenGrantAsync", refreshTokenResponse);
                return await _tokenHelper.GenerateAuthResponseAsync(appUser, authRequest, refreshTokenResponse);
            }
            else
            {
                return InvokeResult<AuthResponse>.FromInvokeResult(updateLastRefreshTokenResult.ToInvokeResult());
            }
        }
    }
}