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
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;

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
        private readonly ISingleUseTokenManager _singleUseTokenManager;
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly IOrganizationRepo _orgRepo;

        public const string GRANT_TYPE_PASSWORD = "password";
        public const string GRANT_TYPE_REFRESHTOKEN = "refreshtoken";
        public const string GRANT_TYPE_SINGLEUSETOKEN = "single-use-token";

        public AuthTokenManager(IAppInstanceRepo appInstanceRepo, IAuthenticationLogManager authLogMgr, ISingleUseTokenManager singleUseTokenManager, IOrganizationManager orgManager,
                                IRefreshTokenManager refreshTokenManager, IAuthRequestValidators authRequestValidators, IOrganizationRepo organizationRepo,
                                ITokenHelper tokenHelper, IAppInstanceManager appInstanceManager,
                                IAdminLogger adminLogger, ISignInManager signInManager, IUserManager userManager)
        {
            _refreshTokenManager = refreshTokenManager;
            _adminLogger = adminLogger;
            _tokenHelper = tokenHelper;
            _signInManager = signInManager;
            _userManager = userManager;
            _orgManager = orgManager;
            _orgRepo = organizationRepo;
            _authRequestValidators = authRequestValidators;
            _appInstanceManager = appInstanceManager;
            _singleUseTokenManager = singleUseTokenManager;
            _authLogMgr = authLogMgr;
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

            var signInRequest = new AuthLoginRequest()
            {
                Email = userName,
                Password = password,
                RememberMe = true,
                LockoutOnFailure = false,
                InviteId = authRequest.InviteId
            };

            var signInResponse = await _signInManager.PasswordSignInAsync(signInRequest);
            if (!signInResponse.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrantFailure, userName: userName, errors: signInResponse.ErrorMessage);

                return InvokeResult<AuthResponse>.FromInvokeResult(signInResponse.ToInvokeResult());
            }

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AuthTokenManager_AccessTokenGrantAsync[", "UserLoggedIn", new KeyValuePair<string, string>("email", userName));

            var appUser = await _userManager.FindByNameAsync(userName);
            if (appUser == null)
            {
                /* Should really never, ever happen, but well...let's track it */
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrantFailure, userName: userName, errors:" Could not find user");                
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "pAuthTokenManager_AccessTokenGrantAsync[", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("email", userName));
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrant, appUser);

            if (appUser.IsAccountDisabled)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrantFailure, appUser, errors: "Account Disabled");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[AuthTokenManager_AccessTokenGrantAsync]", "UserLogin Failed - Account Disabled", new KeyValuePair<string, string>("email", userName));
                return InvokeResult<AuthResponse>.FromError("Account Disabled.");
            }

            if (!string.IsNullOrEmpty(authRequest.OrgId))
            {
                if (null == appUser.CurrentOrganization)
                {
                    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrantFailure, appUser, errors: "No Current Org");
                    return InvokeResult<AuthResponse>.FromError($"Sorry, you do not have access to the {authRequest.OrgName} organization.", "NOORGACESS");
                }
                else
                {
                    var userOrgs = await _orgManager.GetOrganizationsForUserAsync(appUser.Id, appUser.ToEntityHeader(), appUser.CurrentOrganization.ToEntityHeader());
                    if (!userOrgs.Where(org => org.OrgId == authRequest.OrgId).Any())
                    {
                        await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrantFailure, appUser, errors: "User does not have access to {authRequest.OrgName}");
                        return InvokeResult<AuthResponse>.FromError($"User does not have access to {authRequest.OrgName}");
                    }
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
                    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrantFailure, appUser, updateLastLoginResult.ErrorMessage);
                    return InvokeResult<AuthResponse>.FromInvokeResult(updateLastLoginResult.ToInvokeResult());
                }
            }

            var refreshTokenResponse = await _refreshTokenManager.GenerateRefreshTokenAsync(authRequest.AppId, authRequest.AppInstanceId, appUser.Id);
            var authResponse = await _tokenHelper.GenerateAuthResponseAsync(appUser, authRequest, refreshTokenResponse);

            if(authResponse.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrantSuccess, appUser, redirectUri: authResponse.RedirectURL);

            }                
            else
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AccessTokenGrantFailure, appUser, authResponse.ErrorMessage);

            return authResponse;
        }

        public async Task<InvokeResult<AuthResponse>> SingleUseTokenGrantAsync(AuthRequest authRequest)
        {
            _adminLogger.Trace($"[AuthTokenManager__SingleUseTokenGrantAsync] App Instance {authRequest.AppInstanceId}");

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrant, userName: authRequest.UserName, userId: authRequest.UserId);

            var refreshTokenRequestValidationResult = _authRequestValidators.ValidateSingleUseTokenGrant(authRequest);
            if (!refreshTokenRequestValidationResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrantFailure, userName: authRequest.UserName, userId: authRequest.UserId);
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrantFailure, userName: authRequest.UserName, userId: authRequest.UserId, errors: refreshTokenRequestValidationResult.ErrorMessage);
                return InvokeResult<AuthResponse>.FromInvokeResult(refreshTokenRequestValidationResult);
            }

            AppUser appUser = await _userManager.FindByIdAsync(authRequest.UserId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrantFailure, userName: authRequest.UserName, userId: authRequest.UserId);
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrantFailure, userId: authRequest.UserId, errors: "Could not find user");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_SingleUseTokenGrantAsync", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("userId", authRequest.UserId));
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrant, appUser);

            if (appUser.IsAccountDisabled)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrantFailure, appUser, errors: "Account disabled.");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_SingleUseTokenGrantAsync", "UserLogin Failed - Account Disabled", new KeyValuePair<string, string>("userId", authRequest.UserId));
                return InvokeResult<AuthResponse>.FromError("Account Disabled.");
            }

            var result = await _singleUseTokenManager.ValidationAsync(authRequest.UserId, authRequest.SingleUseToken);
            if(!result.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrantFailure, appUser, errors:result.ErrorMessage);
                return InvokeResult<AuthResponse>.FromInvokeResult(result);
            }
            _adminLogger.Trace($"[AuthTOkenManager__SingleUseTokenGrantAsync] App Instance {authRequest.AppInstanceId}");

            var updateLastRefreshTokenResult = (await _appInstanceManager.UpdateLastLoginAsync(appUser.Id, authRequest));
            if (updateLastRefreshTokenResult.Successful)
            {
                authRequest.AppInstanceId = updateLastRefreshTokenResult.Result.RowKey;
                var refreshTokenResponse = await _refreshTokenManager.GenerateRefreshTokenAsync(authRequest.AppId, authRequest.AppInstanceId, appUser.Id);
                _adminLogger.LogInvokeResult("[AuthTokenManager_SingleUseTokenGrantAsync]", refreshTokenResponse);

                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrantSuccess, appUser);

                return await _tokenHelper.GenerateAuthResponseAsync(appUser, authRequest.AppInstanceId, refreshTokenResponse);
            }
            else
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.SingleUseTokenGrantFailure, appUser, errors: updateLastRefreshTokenResult.ErrorMessage);
                return InvokeResult<AuthResponse>.FromInvokeResult(updateLastRefreshTokenResult.ToInvokeResult());
            }
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
            if (!refreshTokenRequestValidationResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrant, userName);

                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrantFailed, userName: userName, errors: refreshTokenRequestValidationResult.ErrorMessage);
                return InvokeResult<AuthResponse>.FromInvokeResult(refreshTokenRequestValidationResult);
            }
            AppUser appUser =  await _userManager.FindByNameAsync(authRequest.UserName);
            
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrant, userName);
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrantFailed, appUser, errors: "Could not find user account");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_RefreshTokenGrantAsync", UserAdminErrorCodes.AuthCouldNotFindUserAccount.Message, new KeyValuePair<string, string>("id", userName));
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }
            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrant, appUser);

            if (appUser.IsAccountDisabled)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrantFailed, appUser, errors: "Account Disabled");

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_RefreshTokenGrantAsync", "UserLogin Failed - Account Disabled", new KeyValuePair<string, string>("email", userName));
                return InvokeResult<AuthResponse>.FromError("Account Disabled.");
            }

            if (!String.IsNullOrEmpty(authRequest.OrgId))
            {
                if (appUser.CurrentOrganization == null)
                {
                    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrantFailed, appUser, errors: "No Current Org");
                    return InvokeResult<AuthResponse>.FromError($"App User does not have a current organization, no way to confirm access to {authRequest.OrgName} organization.");
                }
                else
                {
                    var userOrgs = await _orgManager.GetOrganizationsForUserAsync(appUser.Id, appUser.ToEntityHeader(), appUser.CurrentOrganization.ToEntityHeader());
                    if (!userOrgs.Where(org => org.OrgId == authRequest.OrgId).Any())
                    {
                        await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrantFailed, appUser, errors: $"Sorry, you do not have access to the {authRequest.OrgName} organization.");

                        return InvokeResult<AuthResponse>.FromError($"Sorry, you do not have access to the {authRequest.OrgName} organization.", "NOORGACESS");
                    }
                }
            }

            var updateLastRefreshTokenResult = (await _appInstanceManager.UpdateLastAccessTokenRefreshAsync(appUser.Id, authRequest));
            if (updateLastRefreshTokenResult.Successful)
            {
                authRequest.AppInstanceId = updateLastRefreshTokenResult.Result.RowKey;
                var refreshTokenResponse = await _refreshTokenManager.RenewRefreshTokenAsync(authRequest.RefreshToken, appUser.Id);
                _adminLogger.LogInvokeResult("AuthTokenManager_RefreshTokenGrantAsync", refreshTokenResponse);
                if(refreshTokenResponse.Successful)
                    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrantSuccess, appUser);
                else
                    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrantFailed, appUser, errors: refreshTokenResponse.ErrorMessage);

                return await _tokenHelper.GenerateAuthResponseAsync(appUser, authRequest, refreshTokenResponse);
            }
            else
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.RefreshTokenGrantFailed, appUser, errors: updateLastRefreshTokenResult.ErrorMessage);
                return InvokeResult<AuthResponse>.FromInvokeResult(updateLastRefreshTokenResult.ToInvokeResult());
            }
        }

        public async Task<InvokeResult<SingleUseToken>> GenerateOneTimeUseTokenAsync(string userId, TimeSpan? expires = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_GenerateOneTimeUseTokenAsync", "Single Time Use Failed could not find user", new KeyValuePair<string, string>("userId", userId));
                return InvokeResult<SingleUseToken>.FromError($"User with id {userId} does not exist");
            }

            if(user.IsAccountDisabled)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_GenerateOneTimeUseTokenAsync", "Single Time Use Failed, user account is disabled", new KeyValuePair<string, string>("userId", userId));
                return InvokeResult<SingleUseToken>.FromError($"User with id {userId} account is disabled.");
            }

            var refreshTokenResponse = await _singleUseTokenManager.CreateAsync(userId, expires);
            return refreshTokenResponse;
        }
    }
}