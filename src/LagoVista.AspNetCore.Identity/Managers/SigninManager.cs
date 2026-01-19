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
        private readonly IUserFavoritesManager _userFavoritesManager;
        private readonly IMostRecentlyUsedManager _mostRecentlyUsedManager;
        private readonly IAuthenticationLogManager _authLogManager;
        private readonly IBackgroundServiceTaskQueue _bgServiceQueue;

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

        public SignInManager(IAdminLogger adminLogger, IDefaultRoleList defaultRoleList, IUserRoleManager roleManager, IDependencyManager depManager,
                            IUserFavoritesManager userFavoritesManager, IMostRecentlyUsedManager mostRecentlyUsedManager, IAppUserRepo appUserRepo, IBackgroundServiceTaskQueue bgServiceQueue,
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
            _bgServiceQueue = bgServiceQueue;

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[SignInManager__Constructor]", "Created Sign-in manager");
        }

        public Task SignInAsync(AppUser user, bool isPersistent = false)
        {
            return _signinManager.SignInAsync(user, isPersistent);
        }

        public async Task RefreshUserLoginAsync(AppUser user)
        {
            var appUser = await _userManager.FindByIdAsync(user.Id);
            await _signinManager.SignInAsync(appUser, true);
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
            await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthStart, userName: email, inviteId:loginRequest.InviteId);
            timings.Add(new ResultTiming() { Key = "Add password start, auth manager", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            var signIn = UserSignInMetrics.WithLabels(nameof(PasswordSignInAsync));
            UserLoginAttempts.Inc();

            var appUser = await _userManager.FindByNameAsync(userName);
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

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[SignInManager__PasswordSignInAsync]", "User Login", email.ToKVP("email"));
            timings.Add(new ResultTiming() { Key = $"Log event", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            var signInResult = await _signinManager.PasswordSignInAsync(userName, loginRequest.Password, loginRequest.RememberMe, loginRequest.LockoutOnFailure);
            timings.Add(new ResultTiming() { Key = $"Password sign in", Ms = sw.Elapsed.TotalMilliseconds });
            sw.Restart();

            response.AddAuthMetric("Password Sign In");

            if (signInResult.Succeeded)
            {
                if (appUser.IsAccountDisabled)
                {
                    await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PaswwordAuthFailed, email, appUser.Id, extras:"Account Disabled");
                    await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed - Account Disabled", appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());
                    UserLoginFailures.Inc();
                    signIn.Dispose();
                    return InvokeResult<UserLoginResponse>.FromError($"Account [{email}] is disabled.");
                }

                if (!String.IsNullOrEmpty(loginRequest.InviteId))
                {
                    var acceptInviteResult = await _orgManager.AcceptInvitationAsync(loginRequest.InviteId, appUser);
                    if (acceptInviteResult.Successful)
                    {
                        response.RedirectPage = acceptInviteResult.Result.RedirectPage;
                        response.ResponseMessage = acceptInviteResult.Result.ResponseMessage;

                        // now much sure we sign in with the new org after accepting the invite.
                        await SignInAsync(appUser);
                    }
                    else
                    {
                        await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.AcceptInviteFailed, appUser, errors: acceptInviteResult.ErrorMessage, inviteId: loginRequest.InviteId);
                        return InvokeResult<UserLoginResponse>.FromErrors(acceptInviteResult.Errors.ToArray());
                    }
                }

                // if current org is null, make an attempt to load another org this person
                // may already exist in, likely will fail...but we'll deal with that later on.
                if(appUser.CurrentOrganization == null)
                {
                    var firstExisitng = appUser.Organizations.FirstOrDefault();
                    if(firstExisitng != null)
                    {
                        await _orgManager.ChangeOrgsAsync(firstExisitng.Id, firstExisitng, appUser);
                    } 
                    else if(String.IsNullOrEmpty(response.RedirectPage))
                    {

                    }
                }

                if (appUser.CurrentOrganization != null)
                {
                    var org = await _organizationRepo.GetOrganizationAsync(appUser.CurrentOrganization.Id);
                    response.AddAuthMetric("Loaded Organization");
                    timings.Add(new ResultTiming() { Key = $"Loaded organiation", Ms = sw.Elapsed.TotalMilliseconds });
                    sw.Restart();

                    if (!String.IsNullOrEmpty(loginRequest.EndUserAppOrgId) && !String.IsNullOrEmpty(org.EndUserHomePage))
                        response.RedirectPage = org.EndUserHomePage;

                    if (org.CreatedBy.Id == appUser.Id)
                    {
                        _adminLogger.Trace("SignInManager__PasswordSignInAsync; User created organization, is an owner.");

                        var ownerRoleId = _defaultRoleList.GetStandardRoles().Single(rl => rl.Key == DefaultRoleList.OWNER).Id;
                        response.AddAuthMetric("Got Owner Role Id");

                        var hasOwnerRole = await _userRoleManager.UserHasRoleAsync(ownerRoleId, appUser.Id, appUser.CurrentOrganization.Id);
                        response.AddAuthMetric("Check User Has Owner Role");

                        if (!hasOwnerRole)
                        {
                            _adminLogger.Trace("SignInManager__PasswordSignInAsync; User not owner, adding as role.");
                            await _userRoleManager.GrantUserRoleAsync(appUser.Id, ownerRoleId, appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());
                            response.AddAuthMetric("Grant User Role");
                        }
                        else
                        {
                            response.AddAuthMetric("User was owner, don't need to add role");
                            _adminLogger.Trace("SignInManager__PasswordSignInAsync; User already an owner, no need to add role.");

                        }
                    }
                    else
                    {
                        _adminLogger.Trace("SignInManager__PasswordSignInAsync; User did not create organization, thus is not an owner.");
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


                    if (String.IsNullOrEmpty(response.RedirectPage))
                    {
                        if (!String.IsNullOrEmpty(org.HomePage))
                        {
                            response.RedirectPage = org.HomePage;
                        }
                        else if (appUser.ShowWelcome)
                        {
                            response.RedirectPage = CommonLinks.HomeWelcome;
                        }
                        else
                        {
                            response.RedirectPage = CommonLinks.Home;
                        }
                    }

                    appUser.CurrentOrganization = org.CreateSummary();
                }
                else
                {
                    if (!appUser.EmailConfirmed)
                        response.RedirectPage = $"{CommonLinks.ConfirmEmail}?email={appUser.Email.ToLower()}";
                    else if (appUser.CurrentOrganization == null)
                        response.RedirectPage = CommonLinks.CreateDefaultOrg;
                }

                if (String.IsNullOrEmpty(response.RedirectPage)) {
                    if (!appUser.EmailConfirmed)
                        response.RedirectPage = $"{CommonLinks.ConfirmEmail}?email={appUser.Email.ToLower()}";
                    else if (appUser.CurrentOrganization == null)
                        response.RedirectPage = CommonLinks.CreateDefaultOrg;
                }

                appUser.LastLogin = DateTime.UtcNow.ToJSONString();
                // we can bypass the manager here, we are updating the current user if they are logged in, should not require any security.

                await _bgServiceQueue.QueueBackgroundWorkItemAsync(ct =>
                {
                    return _appUserRepo.UpdateAsync(appUser);
                });

                timings.Add(new ResultTiming() { Key = $"User Updated", Ms = sw.Elapsed.TotalMilliseconds });
                sw.Restart();

                response.AddAuthMetric("Update user");
                
                signIn.Dispose();
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

                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthSuccess, appUser, inviteId:loginRequest.InviteId, redirectUri: response.RedirectPage);

                var result = InvokeResult<UserLoginResponse>.Create(response);
                result.Timings.AddRange(timings);
                return result;
            }

            if (signInResult.IsLockedOut)
            {
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed - Locked Out", appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());
                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PaswwordAuthFailed, appUser, errors: "User is locked out.");

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", UserAdminErrorCodes.AuthUserLockedOut.Message, new KeyValuePair<string, string>("email", email));
                signIn.Dispose();
                UserLoginFailures.Inc();
                return InvokeResult<UserLoginResponse>.FromErrors(UserAdminErrorCodes.AuthUserLockedOut.ToErrorMessage());
            }

            await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed", appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());


            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[AuthTokenManager_AccessTokenGrantAsync]", UserAdminErrorCodes.AuthInvalidCredentials.Message, new KeyValuePair<string, string>("email", email));
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
