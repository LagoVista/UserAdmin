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
using System.Security.Claims;
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
        private readonly IAuthenticationLogManager _authLogManager;
        private readonly IUserRedirectServices _userRedirectService;
        private readonly IAppConfig _appConfig;

        private static readonly Histogram UserSignInMetrics = Metrics.CreateHistogram("nuviot_user_sign_in", "Use Sign In Metrics.",
            new HistogramConfiguration
            {
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

        public Task SignInProvisionalAsync(AppUser user, string actorId, bool isPersistent = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (String.IsNullOrWhiteSpace(actorId)) throw new ArgumentNullException(nameof(actorId));

            var claims = new[]
            {
                new Claim(ClaimsFactory.ActorId, actorId),
                new Claim(ClaimsFactory.IdentityStage, ClaimsFactory.ProvisionalIdentityStage)
            };

            return _signinManager.SignInWithClaimsAsync(user, isPersistent, claims);
        }

        public Task RefreshUserLoginAsync(AppUser user)
        {
            return _signinManager.SignInAsync(user, true);
        }

        public async Task<InvokeResult<AuthenticationResponse>> CompleteSignInToAppAsync(AppUser appUser, Stopwatch sw = null, string inviteId = "", string orgId = "")
        {
            if (appUser == null) throw new ArgumentNullException(nameof(appUser));

            _adminLogger.Trace($"{this.Tag()} - CompleteSignInToAppAsync for user {appUser.UserName} with inviteId {inviteId ?? "-"} and orgId {orgId ?? "-"}");

            if (appUser.IsAccountDisabled)
            {
                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthenticationFailed, appUser.UserName, appUser.Id, extras: "Account Disabled");
                UserLoginFailures.Inc();
                return InvokeResult<AuthenticationResponse>.FromError($"Account [{appUser.UserName}] is disabled.");
            }

            var response = new AuthenticationResponse
            {
                AuthenticationState = AuthenticationResponseState.Authenticated,
                InviteId = inviteId ?? String.Empty
            };

            if (!String.IsNullOrEmpty(inviteId))
            {
                var acceptInviteResult = await _orgManager.AcceptInvitationAsync(inviteId, appUser);
                if (!acceptInviteResult.Successful)
                    return InvokeResult<AuthenticationResponse>.FromErrors(acceptInviteResult.Errors.ToArray());

                response.RedirectPage = acceptInviteResult.Result.RedirectPage ?? String.Empty;
                response.ResponseMessage = acceptInviteResult.Result.ResponseMessage ?? String.Empty;
                await SignInAsync(appUser);
            }

            if (appUser.CurrentOrganization == null)
            {
                var firstExisting = appUser.Organizations.FirstOrDefault();
                if (firstExisting != null)
                    await _orgManager.ChangeOrgsAsync(firstExisting.Id, appUser);
            }

            if (appUser.CurrentOrganization != null)
            {
                var userHasOrg = await _orgUserRepo.QueryOrgHasUserAsync(appUser.CurrentOrganization.Id, appUser.Id);
                if (!userHasOrg)
                    return InvokeResult<AuthenticationResponse>.FromError($"User does not have access to organization [{appUser.CurrentOrganization.Id}].");

                var org = await _organizationRepo.GetOrganizationAsync(appUser.CurrentOrganization.Id);
                if (!String.IsNullOrEmpty(orgId) && !String.IsNullOrEmpty(org.EndUserHomePage))
                    response.RedirectPage = org.EndUserHomePage;

                if (org.CreatedBy.Id == appUser.Id)
                {
                    var ownerRoleId = _defaultRoleList.GetStandardRoles().Single(rl => rl.Key == DefaultRoleList.OWNER).Id;
                    var hasOwnerRole = await _userRoleManager.UserHasRoleAsync(ownerRoleId, appUser.Id, appUser.CurrentOrganization.Id);
                    if (!hasOwnerRole)
                        await _userRoleManager.GrantUserRoleAsync(appUser.Id, ownerRoleId, appUser.CurrentOrganization.ToEntityHeader(), appUser.ToEntityHeader());
                }

                appUser.IsOrgAdmin = await _orgManager.IsUserOrgAdminAsync(appUser.CurrentOrganization.Id, appUser.Id);
                appUser.CurrentOrganization = org.CreateSummary();
            }

            appUser.LastLogin = DateTime.UtcNow.ToJSONString();
            await _appUserRepo.UpdateAsync(appUser);

            UserLoginSuccess.Inc();
            await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthenticationSucceeded, appUser, inviteId: inviteId, redirectUri: response.RedirectPage);

            var redirectResult = await _userRedirectService.IdentityDefaultRedirectAsync(appUser);
            if (!redirectResult.Successful)
                return redirectResult.ToInvokeResult<AuthenticationResponse>();

            response.RedirectPage = redirectResult.Result ?? response.RedirectPage;
            return InvokeResult<AuthenticationResponse>.Create(response);
        }

        public async Task<InvokeResult<AuthenticationResponse>> PasswordSignInAsync(AuthLoginRequest loginRequest)
        {
            if (loginRequest == null) throw new ArgumentNullException(nameof(loginRequest));

            var email = loginRequest.Email;
            var userName = String.IsNullOrEmpty(loginRequest.EndUserAppOrgId) ? loginRequest.Email : $"{email}@{loginRequest.EndUserAppOrgId}";

            if (string.IsNullOrEmpty(email)) return InvokeResult<AuthenticationResponse>.FromError("User name is a required field.");
            if (string.IsNullOrEmpty(loginRequest.Password)) return InvokeResult<AuthenticationResponse>.FromError("Password is a required field.");

            await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthenticationStarted, userName: email, inviteId: loginRequest.InviteId);

            var signIn = UserSignInMetrics.WithLabels(nameof(PasswordSignInAsync));
            UserLoginAttempts.Inc();

            var appUser = await _userManager.FindByNameAsync(userName);
            if (appUser == null)
            {
                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthUserNotFound, userName: userName);
                UserLoginFailures.Inc();
                signIn.Dispose();
                return InvokeResult<AuthenticationResponse>.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage());
            }

            var signInResult = await _signinManager.PasswordSignInAsync(userName, loginRequest.Password, loginRequest.RememberMe, loginRequest.LockoutOnFailure);
            if (signInResult.Succeeded)
            {
                var result = await CompleteSignInToAppAsync(appUser, Stopwatch.StartNew(), loginRequest.InviteId, loginRequest.EndUserAppOrgId);
                signIn.Dispose();
                return result;
            }

            if (signInResult.RequiresTwoFactor)
            {
                signIn.Dispose();
                return InvokeResult<AuthenticationResponse>.Create(new AuthenticationResponse
                {
                    AuthenticationState = AuthenticationResponseState.MfaRequired,
                    Provider = "totp",
                    InviteId = loginRequest.InviteId ?? String.Empty
                });
            }

            if (signInResult.IsLockedOut)
            {
                await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthenticationFailed, appUser, errors: "User is locked out.");
                UserLoginFailures.Inc();
                signIn.Dispose();
                return InvokeResult<AuthenticationResponse>.FromErrors(UserAdminErrorCodes.AuthUserLockedOut.ToErrorMessage());
            }

            await _authLogManager.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasswordAuthenticationFailed, appUser, errors: "Likely invalid credentials.");
            UserLoginFailures.Inc();
            signIn.Dispose();
            return InvokeResult<AuthenticationResponse>.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage());
        }

        public Task SignOutAsync()
        {
            return _signinManager.SignOutAsync();
        }
    }
}
