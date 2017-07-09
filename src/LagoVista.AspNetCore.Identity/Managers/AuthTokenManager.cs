using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Models;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Interfaces.Managers;

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

        private const string AUTH_TOKEN_TYPE = "auth";
        private const string GRANT_TYPE_PASSWORD = "password";
        private const string GRANT_TYPE_REFRESHTOKEN = "refreshtoken";

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

            var result = await _signInManager.PasswordSignInAsync(authRequest.UserName, authRequest.Password, true, false);
            if (result.Succeeded)
            {
                var appUser = await _userManager.FindByNameAsync(authRequest.UserName);
                if (appUser == null)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("email", authRequest.UserName));
                    return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
                }

                if (String.IsNullOrEmpty(authRequest.AppInstanceId)) 
                {
                    var appInstanceResult = await _appInstanceManager.CreateForUserAsync(appUser, authRequest);
                    authRequest.AppInstanceId = appInstanceResult.Result.RowKey;
                }
                else
                {
                    await _appInstanceManager.UpdateLastLoginAsync(appUser.Id, authRequest.AppInstanceId);
                }

                var refreshTokenResponse = await _refreshTokenManager.GenerateRefreshTokenAsync(authRequest.AppId, authRequest.AppInstanceId, appUser.Id);
                _adminLogger.LogInvokeResult("AuthTokenManager_AuthAsync_RefreshToken", refreshTokenResponse);
                return _tokenHelper.GenerateAuthResponse(appUser, authRequest, refreshTokenResponse);
            }
            else
            {
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage());
            }
        }


        public async Task<InvokeResult<AuthResponse>> RefreshTokenGrantAsync(AuthRequest authRequest, EntityHeader org, EntityHeader user)
        {
            var requestValidationResult = _authRequestValidators.ValidateAuthRequest(authRequest);
            if (!requestValidationResult.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(requestValidationResult);

            var refreshTokenRequestValidationResult = _authRequestValidators.ValidateRefreshTokenGrant(authRequest);
            if (!refreshTokenRequestValidationResult.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(refreshTokenRequestValidationResult);

            var appUser = await _userManager.FindByNameAsync(authRequest.UserName);
            if (appUser == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AuthAsync", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("id", authRequest.UserName));
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            if (!String.IsNullOrEmpty(authRequest.OrgId) && (appUser.CurrentOrganization == null || authRequest.OrgId != appUser.CurrentOrganization.Id))
            {
                var changeOrgResult = await _orgHelper.SetUserOrgAsync(authRequest, appUser);
                if (!changeOrgResult.Successful) return InvokeResult<AuthResponse>.FromInvokeResult(changeOrgResult);
            }

            await _appInstanceManager.UpdateLastAccessTokenRefreshAsync(appUser.Id, authRequest.AppInstanceId);

            var refreshTokenResponse = await _refreshTokenManager.RenewRefreshTokenAsync(authRequest.RefreshToken, user.Id);
            _adminLogger.LogInvokeResult("AuthTokenManager_AuthAsync_RefreshToken", refreshTokenResponse);
            return _tokenHelper.GenerateAuthResponse(appUser, authRequest, refreshTokenResponse);
        }
    }
}
