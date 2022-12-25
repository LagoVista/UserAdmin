using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.AspNetCore.Identity;
using Prometheus;
using System.Collections.Generic;
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
        private readonly SignInManager<AppUser> _signinManager;
        private readonly IDefaultRoleList _defaultRoleList;
        private readonly IOrganizationRepo _organizationRepo;

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
                             ISecurity security, IAppConfig appConfig, IUserManager userManager, IOrganizationManager orgManager,  IOrganizationRepo orgRepo,
                             SignInManager<AppUser> signInManager)
            : base(adminLogger, appConfig, depManager, security)
        {
            _signinManager = signInManager;
            _adminLogger = adminLogger;
            _orgManager = orgManager;
            _userManager = userManager;
            _userRoleManager = roleManager;
            _defaultRoleList = defaultRoleList;
            _organizationRepo = orgRepo;
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

        public async Task<InvokeResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var signIn = UserSignInMetrics.WithLabels(nameof(PasswordSignInAsync));
            UserLoginAttempts.Inc();

            System.Console.WriteLine($"SignInManager__PasswordSignInAsync; User: {userName}");

            if (string.IsNullOrEmpty(userName)) return InvokeResult.FromError($"User name is a required field [{userName}].");
            if (string.IsNullOrEmpty(password)) return InvokeResult.FromError($"Password is a required field [{userName}].");            

            var signInResult = await _signinManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

            var appUser = await _userManager.FindByEmailAsync(userName);
            if (appUser == null)
            {
                await LogEntityActionAsync(userName, typeof(AppUser).Name, $"Could not find user with account [{userName}].", EntityHeader.Create("unkonwn", "unknown"), EntityHeader.Create(userName, userName));
                UserLoginFailures.Inc();
                return InvokeResult.FromError($"SignInManager__PasswordSignInAsync;  Could not find user [{userName}].");
            }

            if(appUser.IsAccountDisabled)
            {
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed - Account Disabled", appUser.CurrentOrganization, appUser.ToEntityHeader());
                UserLoginFailures.Inc(); 
                return InvokeResult.FromError($"Account [{userName}] is disabled.");
            }

            if (appUser.CurrentOrganization != null)
            {
                var org = await _organizationRepo.GetOrganizationAsync(appUser.CurrentOrganization.Id);
                if(org.CreatedBy.Id == appUser.Id)
                {
                    System.Console.WriteLine("SignInManager__PasswordSignInAsync; User created organization, is an owner.");
              
                    var ownerRoleId = _defaultRoleList.GetStandardRoles().Single(rl => rl.Key == DefaultRoleList.OWNER).Id;
                    var hasOwnerRole = await _userRoleManager.UserHasRoleAsync(ownerRoleId, appUser.Id, appUser.CurrentOrganization.Id);
                    if (!hasOwnerRole)
                    {
                        System.Console.WriteLine("SignInManager__PasswordSignInAsync; User not owner, adding as role.");
                        await _userRoleManager.GrantUserRoleAsync(appUser.Id, ownerRoleId, appUser.CurrentOrganization, appUser.ToEntityHeader());
                    }
                    else
                    {
                        System.Console.WriteLine("SignInManager__PasswordSignInAsync; User already an owner, no need to add role.");
                    }
                }
                else
                {
                    System.Console.WriteLine("SignInManager__PasswordSignInAsync; User did not create organization, thus is not an owner.");
                }

                var isOrgAdmin = await _orgManager.IsUserOrgAdminAsync(appUser.CurrentOrganization.Id, appUser.Id);
                if (isOrgAdmin != appUser.IsOrgAdmin)
                {
                    appUser.IsOrgAdmin = isOrgAdmin;
                    await _userManager.UpdateAsync(appUser);
                }
            }

            if (signInResult.Succeeded)
            {
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin", appUser.CurrentOrganization, appUser.ToEntityHeader());
                signIn.Dispose();
                UserLoginFailures.Inc();
                return InvokeResult.Success;
            }

            if (signInResult.IsLockedOut)
            {
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed - Locked Out", appUser.CurrentOrganization, appUser.ToEntityHeader());

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", UserAdminErrorCodes.AuthUserLockedOut.Message, new KeyValuePair<string, string>("email", userName));
                signIn.Dispose();
                UserLoginFailures.Inc();
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthUserLockedOut.ToErrorMessage());
            }

            await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "UserLogin Failed", appUser.CurrentOrganization, appUser.ToEntityHeader());


            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_AccessTokenGrantAsync", UserAdminErrorCodes.AuthInvalidCredentials.Message, new KeyValuePair<string, string>("email", userName));
            signIn.Dispose();
            UserLoginSuccess.Inc();
            return InvokeResult.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage());
        }


        public Task SignOutAsync()
        {
            return _signinManager.SignOutAsync();
        }
    }
}
