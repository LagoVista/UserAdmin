// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7cf78365294b1ab0d69ad824e81bc787b74c808958ead145bb1fd126d8807918
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class SignInManager : ManagerBase, ISignInManager
    {
        private readonly IAdminLogger _adminLogger;
        private readonly IUserRoleManager _userRoleManager;
        private readonly IUserManager _userManager;
        private readonly IOrganizationManager _orgManager;
        private readonly IAppUserRepo _appUserRepo;
        private readonly SignInManager<AppUser> _signinManager;
        private readonly IDefaultRoleList _defaultRoleList;
        private readonly IOrganizationRepo _organizationRepo;
        private readonly IOrgUserRepo _orgUserRepo;
        private readonly IUserFavoritesManager _userFavoritesManager;
        private readonly IMostRecentlyUsedManager _mostRecentlyUsedManager;
        private readonly IAuthenticationLogManager _authLogManager;
        private readonly IUserRedirectServices _userRedirectService;
        private readonly IAppConfig _appConfig;

        private static readonly Histogram UserSignInMetrics = Metrics.CreateHistogram("nuviot_user_sign_in", "Use Sign In Metrics.",
           new HistogramConfiguration
           {
               // Here you specify only the names of the labels.
               LabelNames = new[] { "action" },
               Buckets = Histogram.ExponentialBuckets(0.250, 2, 8)
           });

        public static readonly Counter UserLoginAttempts = Metrics.CreateCounter("nuviot_login_attempt", "Number of user login attepts");
        public static readonly Counter UserLoginSuccess = Metrics.CreateCounter("nuviot_login_success", "Number of user login successes");
        public static readonly Counter UserLoginFailures = Metrics.CreateCounter("nuviot_login_failures", "Number of user login failures");

        public SignInManager(IAdminLogger adminLogger, IDefaultRoleList defaultRoleList, IUserRoleManager roleManager, IDependencyManager depManager, IOrgUserRepo orgUserRepo,
                            IUserFavoritesManager userFavoritesManager, IMostRecentlyUsedManager mostRecentlyUsedManager, IAppUserRepo appUserRepo, IUserRedirectServices userRedirectService,
                            IAuthenticationLogManager authenticationLogManager, ISecurity security, IAppConfig appConfig, IUserManager userManager, IOrganizationManager orgManager, IOrganizationRepo orgRepo,
                             SignInManager<AppUser> signInManager)
            : base(adminLogger, appConfig, depManager, security)
        {
            _signinManager = signInManager;
            _adminLogger = adminLogger;
            _orgManager = orgManager;
            _userManager = userManager;
            _userRoleManager = roleManager;
            _defaultRoleList = defaultRoleList;
            _appUserRepo = appUserRepo;
            _organizationRepo = orgRepo;
            _userFavoritesManager = userFavoritesManager;
            _mostRecentlyUsedManager = mostRecentlyUsedManager;
            _authLogManager = authenticationLogManager;
            _userRedirectService = userRedirectService;
            _appConfig = appConfig;
            _orgUserRepo = orgUserRepo;
            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[SignInManager__Constructor]", "Created Sign-in manager");
        }

        public Task SignInAsync(AppUser user, bool isPersistent = false)
        {
            return _signinManager.SignInAsync(user, isPersistent);
        }

        public async Task RefreshUserLoginAsync(AppUser user)
        {
            await _signinManager.SignInAsync(user, true);
        }

        public async Task<InvokeResult<UserLoginResponse>> CompleteSignInToAppAsync(AppUser appUser, Stopwatch sw = null, string inviteId = "", string orgId = "")
        {
            _adminLogger.Trace($"{this.Tag()} - CompleteSignInToAppAsync for user {appUser.UserName} with inviteId {inviteId ?? "-"} and orgId {orgId ?? "-"}");

            var response = new UserLoginResponse();
            var timings = new List<ResultTiming>();

            if(sw == null) 
                sw = Stopwatch.StartNew();

            if (appUser.IsAccountDisabled)
            {
                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PaswwordAuthFailed, appUser.UserName, appUser.Id, extras: "Account Disabled");
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed - Account Disabled", appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());
                UserLoginFailures.Inc();
                return InvokeResult<UserLoginResponse>.FromError($"Account [{appUser.UserName}] is disabled.");
            }

            if (!String.IsNullOrEmpty(inviteId))
            {
                var acceptInviteResult = await _orgManager.AcceptInvitationAsync(inviteId, appUser);
                if (acceptInviteResult.Successful)
                {
                    response.RedirectPage = acceptInviteResult.Result.RedirectPage;
                    response.ResponseMessage = acceptInviteResult.Result.ResponseMessage;

                    // now much sure we sign in with the new org after accepting the invite.
                    await SignInAsync(appUser);
                }
                else
                {
                    await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AcceptInviteFailed, appUser, errors: acceptInviteResult.ErrorMessage, inviteId: inviteId);
                    return InvokeResult<UserLoginResponse>.FromErrors(acceptInviteResult.Errors.ToArray());
                }
            }

            // if current org is null, make an attempt to load another org this person
            // may already exist in, likely will fail...but we'll deal with that later on.
            if (appUser.CurrentOrganization == null)
            {
                _adminLogger.Trace($"{this.Tag()} - App does not have a current user.");

                var firstExisitng = appUser.Organizations.FirstOrDefault();
                if (firstExisitng != null)
                {
                    _adminLogger.Trace($"{this.Tag()} - does have access to {firstExisitng.Text} - attempt to switch to that org.");
                    var switchOrgResult = await _orgManager.ChangeOrgsAsync(firstExisitng.Id, appUser);
                    if(switchOrgResult.Successful)
                        _adminLogger.Trace($"{this.Tag()} - set access for user {firstExisitng.Text} - switched to that org.");
                    else
                    {
                        appUser.Organizations.Clear();
                        appUser.CurrentOrganizationRoles.Clear();
                        _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), "Failed to switch to first existing org", new KeyValuePair<string, string>("orgId", firstExisitng.Id), 
                            new KeyValuePair<string, string>("userId", appUser.Id), new KeyValuePair<string, string>("error", switchOrgResult.ErrorMessage));
                    }
                }
            }
            else
            {
                var userHasOg = await _orgUserRepo.QueryOrgHasUserAsync(appUser.CurrentOrganization.Id, appUser.Id);
                if (!userHasOg)
                {
                    _adminLogger.AddError(this.Tag(),$"User had a current organization set, but they do not have access to that organization Org: {appUser.CurrentOrganization.Text} - {appUser.Email}/{appUser.UserName}");
                    return InvokeResult<UserLoginResponse>.FromError($"User does have org attempting to log in to that organization, please contact a system administrator and provide them with the following id: cid={appUser.CurrentOrganization.Id}, uid={appUser.Id}.");
                }

                var org = await _organizationRepo.GetOrganizationAsync(appUser.CurrentOrganization.Id);
                response.AddAuthMetric("Loaded Organization");
                timings.Add(new ResultTiming() { Key = $"Loaded organiation", Ms = sw.Elapsed.TotalMilliseconds });
                sw.Restart();

                if (!String.IsNullOrEmpty(orgId) && !String.IsNullOrEmpty(org.EndUserHomePage))
                    response.RedirectPage = org.EndUserHomePage;

                if (org.CreatedBy.Id == appUser.Id)
                {
                    _adminLogger.Trace($"{this.Tag()}; User created organization, is an owner.");

                    var ownerRoleId = _defaultRoleList.GetStandardRoles().Single(rl => rl.Key == DefaultRoleList.OWNER).Id;
                    response.AddAuthMetric("Got Owner Role Id");

                    var hasOwnerRole = await _userRoleManager.UserHasRoleAsync(ownerRoleId, appUser.Id, appUser.CurrentOrganization.Id);
                    response.AddAuthMetric("Check User Has Owner Role");

                    if (!hasOwnerRole)
                    {
                        _adminLogger.Trace($"{this.Tag()}; User not owner, adding as role.");
                        await _userRoleManager.GrantUserRoleAsync(appUser.Id, ownerRoleId, appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());
                        response.AddAuthMetric("Grant User Role");
                    }
                    else
                    {
                        response.AddAuthMetric("User was owner, don't need to add role");
                        _adminLogger.Trace($"{this.Tag()}; User already an owner, no need to add role.");

                    }
                }
                else
                {
                    _adminLogger.Trace($"{this.Tag} User did not create organization, thus is not an owner.");
                }

                sw.Restart();

      
                var isOrgAdmin = await _orgManager.IsUserOrgAdminAsync(appUser.CurrentOrganization.Id, appUser.Id);
                response.AddAuthMetric("Check if User is Admin");
                if (isOrgAdmin != appUser.IsOrgAdmin)
                {
                    appUser.IsOrgAdmin = isOrgAdmin;
                }

                timings.Add(new ResultTiming() { Key = $"Org admin check", Ms = sw.Elapsed.TotalMilliseconds });
                sw.Restart();
             
                appUser.CurrentOrganization = org.CreateSummary();
            }

            appUser.LastLogin = DateTime.UtcNow.ToJSONString();
            await _appUserRepo.UpdateAsync(appUser);
            // we can bypass the manager here, we are updating the current user if they are logged in, should not require any security.

            timings.Add(new ResultTiming() { Key = $"User Updated", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            response.AddAuthMetric("Update user");

            UserLoginSuccess.Inc();

            if (appUser.CurrentOrganization != null)
            {
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin", appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());

                var favs = await _userFavoritesManager.GetUserFavoritesAsync(appUser.ToEntityHeader(), appUser.CurrentOrganization.ToEntityHeader());
                response.AddAuthMetric("Add FAVs");

                var mrus = await _mostRecentlyUsedManager.GetMostRecentlyUsedAsync(appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());
                timings.AddRange(mrus.Timings);
                response.AddAuthMetric("Add MRUs");

                response.Favorites = favs;
                response.MostRecentlyUsed = mrus.Result;

                timings.Add(new ResultTiming() { Key = $"Finalize user ", Ms = sw.Elapsed.TotalMilliseconds });
                sw.Restart();
            }

            response.User = appUser;

            await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthSuccess, appUser, inviteId: inviteId, redirectUri: response.RedirectPage);

            var redirectResult = await _userRedirectService.IdentityDefaultRedirectAsync(appUser); 
            if(!redirectResult.Successful)
            {
                response.RedirectPage = redirectResult.RedirectURL;
            }

            var result = InvokeResult<UserLoginResponse>.Create(response);
            result.Timings.AddRange(timings);
            return result;
        }

        public async Task<InvokeResult<UserLoginResponse>> PasswordSignInAsync(AuthLoginRequest loginRequest)
        {
            var email = loginRequest.Email;

            var userName = String.IsNullOrEmpty(loginRequest.EndUserAppOrgId) ? loginRequest.Email : $"{email}@{loginRequest.EndUserAppOrgId}";

            var timings = new List<ResultTiming>();

            var response = new UserLoginResponse();

            if (string.IsNullOrEmpty(email)) return InvokeResult<UserLoginResponse>.FromError($"User name is a required field [{email}].");
            if (string.IsNullOrEmpty(loginRequest.Password)) return InvokeResult<UserLoginResponse>.FromError($"Password is a required field [{email}].");
            var sw = Stopwatch.StartNew();
            await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthStart, userName: email, inviteId: loginRequest.InviteId);
            timings.Add(new ResultTiming() { Key = "Add password start, auth manager", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            var signIn = UserSignInMetrics.WithLabels(nameof(PasswordSignInAsync));
            UserLoginAttempts.Inc();

            var appUser = await _userManager.FindByEmailAsync(userName);
            response.AddAuthMetric("Got User");
            timings.Add(new ResultTiming() { Key = $"Find by email", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            if (appUser == null)
            {
                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthUserNotFound, userName);
                await LogEntityActionAsync(email, typeof(AppUser).Name, $"Could not find user with account [{email}].", EntityHeader.Create("unkonwn", "unknown"), EntityHeader.Create(email, email));
                UserLoginFailures.Inc();
                signIn.Dispose();
                return InvokeResult<UserLoginResponse>.FromError($"SignInManager__PasswordSignInAsync;  Could not find user [{email}].");
            }

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, this.Tag(), "User Login", email.ToKVP("email"));
            timings.Add(new ResultTiming() { Key = $"Log event", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            var signInResult = await _signinManager.PasswordSignInAsync(userName, loginRequest.Password, loginRequest.RememberMe, loginRequest.LockoutOnFailure);
            _adminLogger.Trace($"{this.Tag()} - Sign in result: {signInResult.Succeeded}");
            timings.Add(new ResultTiming() { Key = $"Password sign in", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            response.AddAuthMetric("Password Sign In");

            if (signInResult.Succeeded)
            {
                _adminLogger.Trace($"{this.Tag()} - finalize sign in");
                var result = await CompleteSignInToAppAsync(appUser, sw, loginRequest.InviteId, loginRequest.EndUserAppOrgId);
                result.Timings.AddRange(timings);
                signIn.Dispose();

                return result;
            }

            _adminLogger.Trace($"{this.Tag()} - didn't sign in, handling error");

            if (signInResult.IsLockedOut)
            {
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed - Locked Out", appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());
                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PaswwordAuthFailed, appUser, errors: "User is locked out.");

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.AuthUserLockedOut.Message, new KeyValuePair<string, string>("email", email));
                signIn.Dispose();
                UserLoginFailures.Inc();
                return InvokeResult<UserLoginResponse>.FromErrors(UserAdminErrorCodes.AuthUserLockedOut.ToErrorMessage());
            }

            var orgEH = appUser.CurrentOrganization == null ? _appConfig.SystemOwnerOrg : appUser.CurrentOrganization.ToEntityHeader(); 
            await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed", orgEH, appUser.ToEntityHeader());

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.AuthInvalidCredentials.Message, new KeyValuePair<string, string>("email", email));
            signIn.Dispose();
            UserLoginSuccess.Inc();

            await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PaswwordAuthFailed, appUser, errors: "Likely invalid credentials.");

            return InvokeResult<UserLoginResponse>.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage());
        }


        public Task SignOutAsync()
        {
            return _signinManager.SignOutAsync();
        }
    }
}
