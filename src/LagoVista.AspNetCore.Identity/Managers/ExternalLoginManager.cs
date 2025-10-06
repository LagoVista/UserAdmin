using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class ExternalLoginManager : IExternalLoginManager
    {
        private readonly ITwitterAuthService _twitterAuthorization;
        private readonly IAdminLogger _adminLogger;
        private readonly IAuthenticationLogManager _authLogManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAuthTokenManager _authTokenManager;
        private readonly IAppUserManager _appUserManager;
        private readonly IOrganizationManager _orgManager;
        private readonly IAppConfig _appConfig;
        private readonly IUserVerficationManager _userVerficationManager;
        private readonly IUserRegistrationManager _userRegistrationManager;

        public ExternalLoginManager(ITwitterAuthService twitterAuthorization, SignInManager<AppUser> signInManager, IAppConfig appConfig, IUserVerficationManager userVerficationManager, IAuthTokenManager authTokenManager, IOrganizationManager orgManager, IAdminLogger adminLogger, IAuthenticationLogManager authLogManager,
                                   IUserRegistrationManager userRegistrationManager, IAppUserManager appUserManager)
        {
            _twitterAuthorization = twitterAuthorization ?? throw new ArgumentNullException(nameof(twitterAuthorization));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _authLogManager = authLogManager ?? throw new ArgumentNullException(nameof(authLogManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _authTokenManager = authTokenManager ?? throw new ArgumentNullException(nameof(authTokenManager));
            _orgManager = orgManager ?? throw new ArgumentNullException(nameof(orgManager));
            _appUserManager = appUserManager ?? throw new ArgumentNullException(nameof(appUserManager));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _userVerficationManager = userVerficationManager ?? throw new ArgumentNullException(nameof(userVerficationManager));
            _userRegistrationManager = userRegistrationManager ?? throw new ArgumentNullException(nameof(userRegistrationManager));
        }

        public async Task<ExternalLogin> GetExternalLoginAsync(ExternalLoginInfo loginInfo)
        {
            var externalLogin = new ExternalLogin();
            switch (loginInfo.LoginProvider)
            {
                case "Microsoft":
                    {
                        externalLogin.Provider = EntityHeader<ExternalLoginTypes>.Create(ExternalLoginTypes.Microsoft);

                        externalLogin.Id = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.NameIdentifier).First().Value;
                        externalLogin.UserName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Email).First().Value;
                        externalLogin.Email = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Email).First().Value;
                        externalLogin.FirstName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.GivenName).First().Value;
                        externalLogin.LastName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Surname).First().Value;
                        externalLogin.Organization = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Surname).First().Value;
                    }
                    break;
                case "GitHub":
                    {
                        externalLogin.Provider = EntityHeader<ExternalLoginTypes>.Create(ExternalLoginTypes.GitHub);
                        externalLogin.Id = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.NameIdentifier).First().Value;
                        externalLogin.UserName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Name).First().Value;
                        externalLogin.Email = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Email).First().Value;
                        externalLogin.FirstName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.GivenName).First().Value;
                        externalLogin.LastName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Surname).First().Value;
                        if (loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimsFactory.Organization).Any())
                            externalLogin.Organization = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimsFactory.Organization).First().Value;
                        else
                            externalLogin.Organization = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Surname).First().Value;
                    }
                    break;
                case "LinkedIn":
                    {
                        externalLogin.Provider = EntityHeader<ExternalLoginTypes>.Create(ExternalLoginTypes.LinkedIn);
                        externalLogin.Id = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.NameIdentifier).First().Value;
                        externalLogin.UserName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Email).First().Value;
                        externalLogin.Email = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Email).First().Value;
                        externalLogin.FirstName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.GivenName).First().Value;
                        externalLogin.LastName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Surname).First().Value;
                        externalLogin.Organization = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Surname).First().Value;
                    }
                    break;
                case "Google":
                    {
                        externalLogin.Provider = EntityHeader<ExternalLoginTypes>.Create(ExternalLoginTypes.Google);
                        externalLogin.Id = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.NameIdentifier).First().Value;
                        externalLogin.UserName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Email).First().Value;
                        externalLogin.Email = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Email).First().Value;
                        externalLogin.FirstName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.GivenName).First().Value;
                        externalLogin.LastName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Surname).First().Value;
                        externalLogin.Organization = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Surname).First().Value;
                    }
                    break;
                case "Twitter":
                    {
                        externalLogin.Provider = EntityHeader<ExternalLoginTypes>.Create(ExternalLoginTypes.Twitter);
                        externalLogin.Id = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.NameIdentifier).First().Value;
                        externalLogin.UserName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Name).First().Value;
                        externalLogin.Email = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Email).First().Value;
                        externalLogin.FirstName = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimTypes.Name).First().Value;
                        externalLogin.LastName = "From Twitter";
                        externalLogin.Organization = externalLogin.UserName;
                        externalLogin.OAuthToken = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimsFactory.OAuthToken).First().Value;
                        externalLogin.OAuthTokenVerifier = loginInfo.Principal.Claims.Where(clm => clm.Type == ClaimsFactory.OAuthTokenVerifier).First().Value;

                        var requestToken = await _twitterAuthorization.ObtainRequestTokenAsync();
                    }
                    break;
                default:
                    throw new ArgumentNullException("");
            }

            return externalLogin;
        }


        public async Task<InvokeResult<string>> FinalizeExternalLogin(AppUser appUser, Dictionary<string, string> cookies, string provider, string inviteId, string returnUrl)
        {
            if (!String.IsNullOrEmpty(returnUrl))
            {
                // in some cases a call to a service will return with a 401 and it will be marked
                // as the return url.  Redirecting to a service, doesn't make sense and thus we'll 
                // just return to the /home or /home/welcome page. 
                if (returnUrl.ToLower().StartsWith("/api") || returnUrl.ToLower().StartsWith("api"))
                    returnUrl = null;
            }

            _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] User: {appUser.Email}");
            await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizingLogin, appUser, inviteId: inviteId, redirectUri: String.IsNullOrEmpty( returnUrl) ? String.Empty : returnUrl,  oauthProvider: provider);

            await _signInManager.SignInAsync(appUser, false);

            if (!String.IsNullOrEmpty(returnUrl))
            {
                if (returnUrl == "nuviot-mobile")
                {
                    var scheme = "nuviot";
                    if (cookies.ContainsKey("mobile_app_scheme"))
                    {
                        scheme = cookies["mobile_app_scheme"];
                    }

                    var ipAddr = cookies.ContainsKey("expo_dev_ip_addr") ? cookies["expo_dev_ip_addr"].Replace("-", ".") : String.Empty;
                    var rootScheme = String.IsNullOrEmpty(ipAddr) ? $"{scheme}://" : $"exp+device-mgr://{ipAddr}:8081/--/";

                    var singleUseToken = await _authTokenManager.GenerateOneTimeUseTokenAsync(appUser.Id);
                    if (singleUseToken.Successful)
                    {
                        if (!String.IsNullOrEmpty(inviteId))
                        {
                            var response = await _orgManager.AcceptInvitationAsync(inviteId, appUser);
                            if (response.Successful)
                            {
                                var redirectUrl = $"{rootScheme}acceptinvite?userid={singleUseToken.Result.UserId}&token={singleUseToken.Result.Token}&inviteid={inviteId}&page=acceptinvite";
                                await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, inviteId: inviteId, oauthProvider: provider, extras: $"Mobile - Accept Invite", redirectUri: redirectUrl);
                                return InvokeResult<string>.Create(redirectUrl);
                            }
                            else
                            {
                                await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, inviteId: inviteId, oauthProvider: provider, extras: $"Could not accept invitation - {response.ErrorMessage}", redirectUri: response.RedirectURL);
                                return InvokeResult<string>.Create(response.RedirectURL);
                            }
                        }
                        else if (!appUser.EmailConfirmed)
                        {
                            var redirectUrl = $"{rootScheme}confirmemail?userid={singleUseToken.Result.UserId}&token={singleUseToken.Result.Token}";
                            await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, oauthProvider: provider, extras: $"Mobile - Confirm Email", redirectUri: redirectUrl);
                            return InvokeResult<string>.Create(redirectUrl);
                        }
                        else if (appUser.CurrentOrganization == null)
                        {
                            var redirectUrl = $"{rootScheme}createorg?userid={singleUseToken.Result.UserId}&token={singleUseToken.Result.Token}";
                            await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, oauthProvider: provider, extras: $"Mobile - Create Org", redirectUri: redirectUrl);
                            return InvokeResult<string>.Create(redirectUrl);
                        }
                        else if (appUser.ShowWelcome)
                        {
                            var redirectUrl = $"{rootScheme}welcome?userid={singleUseToken.Result.UserId}&token={singleUseToken.Result.Token}";
                            _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] - Mobile - Welcome View: {redirectUrl}");
                            await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, oauthProvider: provider, extras: $"Mobile - Show Welcome", redirectUri: redirectUrl);
                            return InvokeResult<string>.Create(redirectUrl);
                        }
                        else
                        {
                            var redirectUrl = $"{rootScheme}home?userid={singleUseToken.Result.UserId}&token={singleUseToken.Result.Token}&page=home";
                            _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] - Mobile - Home View: {redirectUrl}");
                            await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, oauthProvider: provider, extras: $"Mobile - Show Home", redirectUri: redirectUrl);
                            return InvokeResult<string>.Create(redirectUrl);
                        }
                    }
                    else
                    {
                        var redirectUrl = $"{rootScheme}error?msg=could_not_generate_single_use_token";
                        _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] - Mobile: Error Generating Single Use Token: {redirectUrl}");

                        await _authLogManager.AddAsync(AuthLogTypes.OAuthError, appUser, oauthProvider: provider, extras: $"Mobile - Error", redirectUri: redirectUrl, errors: singleUseToken.Errors.First().Message);
                        return InvokeResult<string>.Create(redirectUrl);
                    }
                }
                else
                {
                    await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, oauthProvider: provider, extras: $"Return URL Provided", redirectUri: returnUrl);
                    return InvokeResult<string>.Create(returnUrl);
                }
            }
            // user does exist and is configured properly.
            else if (!String.IsNullOrEmpty(inviteId))
            {
                var redirectUri = $"{CommonLinks.InviteAccepted}?inviteid={inviteId}&emailconfirmed={appUser.EmailConfirmed.ToString().ToLower()}&email={appUser.Email.ToLower()}";

                var response = await _orgManager.AcceptInvitationAsync(inviteId, appUser);
                if (response.Successful)
                {
                    _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] - Web - Email was confirmed, has invite");
                    await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, inviteId: inviteId, oauthProvider: provider, extras: appUser.EmailConfirmed ?
                        $"Web - Email was confirmed, has invite" : $"Web - Email was not confirmed, has invite"
                        , redirectUri: redirectUri);
                    return InvokeResult<string>.Create(redirectUri);
                }
                else
                {
                    await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, inviteId: inviteId, oauthProvider: provider, extras: $"Could not accept invitation - {response.ErrorMessage}", redirectUri: response.RedirectURL);
                    return InvokeResult<string>.Create(response.RedirectURL);
                }
            }
            else if (!appUser.EmailConfirmed)
            {
                _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] - Web - Email Not Confirmed");
                await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, oauthProvider: provider, extras: $"Web - Email Not Confirmed", redirectUri: $"{CommonLinks.ConfirmEmail}?email={appUser.Email}");
                return InvokeResult<string>.Create($"{CommonLinks.ConfirmEmail}?email={appUser.Email.ToLower()}");
            }
            else if (appUser.CurrentOrganization == null)
            {
                _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] - Web - No Current Organization");
                await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, oauthProvider: provider, extras: $"Web - No Current Organization", redirectUri: CommonLinks.CreateDefaultOrg);
                return InvokeResult<string>.Create(CommonLinks.CreateDefaultOrg);
            }
            else if (appUser.ShowWelcome)
            {
                _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] - Web - Welcome View");
                await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser.ToEntityHeader(), appUser.CurrentOrganization.ToEntityHeader(), oauthProvider: provider, extras: $"Web - Welcome View", redirectUri: CommonLinks.HomeWelcome);
                return InvokeResult<string>.Create(CommonLinks.HomeWelcome);
            }
            else
            {
                _adminLogger.Trace($"[OAUTH__FinalizeExternalLogin] - Web - Return Home View");
                await _authLogManager.AddAsync(AuthLogTypes.OAuthFinalizedLogin, appUser, oauthProvider: provider, extras: $"Web - Home", redirectUri: CommonLinks.Home);
                return InvokeResult<string>.Create(CommonLinks.Home);
            }
        }

        public async Task<InvokeResult<string>> AssociateExistingUserAsync(ExternalLogin externalLoginInfo, Dictionary<string, string> cookies, AppUser appUser, string inviteId, string returnUrl = null)
        {
            _adminLogger.Trace("[OAUTH_HandleExternalLogin] - User currently logged in, associate user credentials.");
            await _appUserManager.AssociateExternalLoginAsync(appUser.Id, externalLoginInfo, appUser.ToEntityHeader());

            appUser = await _signInManager.UserManager.FindByIdAsync(appUser.Id);

            await _authLogManager.AddAsync(AuthLogTypes.OAuthLogin, appUser.Id, appUser.Name, inviteId: inviteId, oauthProvier: externalLoginInfo.Provider.Text, extras: $"user already logged in, associate login if necessary");

            return await FinalizeExternalLogin(appUser, cookies, externalLoginInfo.Provider.Text, inviteId, returnUrl);
        }

        public async Task<InvokeResult<string>> HandleExternalLogin(ExternalLogin externalLoginInfo, Dictionary<string, string> cookies, string inviteId, string returnUrl = null)
        {
            if (externalLoginInfo.Provider.Value == ExternalLoginTypes.SLTesting && 
                (_appConfig.Environment != Environments.Development &&
                 _appConfig.Environment != Environments.Local &&
                 _appConfig.Environment != Environments.LocalDevelopment))
                throw new NotSupportedException($"OAuth Provider SL Testing is only available in the development environment, currently in {_appConfig.Environment}.");

            if (!externalLoginInfo.Provider.HasValue)
                throw new ArgumentNullException("externalLogin.Provider");

            _adminLogger.Trace($"[OAUTH_HandleEXternalLogin] - {externalLoginInfo.Id} - {externalLoginInfo.Provider.Value}");

            await _authLogManager.AddAsync(AuthLogTypes.OAuthCallback, oauthProvier: externalLoginInfo.Provider.Text, inviteId: inviteId, extras: $"Email: {externalLoginInfo.Email}, First Name: {externalLoginInfo.FirstName}, Last Name: {externalLoginInfo.LastName}");

            _adminLogger.Trace($"[OAUTH_HandleExternalLogin] - User not logged in, attempt to find - {externalLoginInfo.Id} - {externalLoginInfo.Provider.Value}");
            var appUser = await _appUserManager.GetUserByExternalLoginAsync(externalLoginInfo.Provider.Value, externalLoginInfo.Id);
            if (appUser != null)
            {
                _adminLogger.Trace($"[OAUTH_HandleExternalLogin] - Found a user by external credentials, finalize login and return - {externalLoginInfo.Id} - {externalLoginInfo.Provider.Value}");

                await _authLogManager.AddAsync(AuthLogTypes.OAuthLogin, appUser.Id, appUser.Name, inviteId: inviteId, oauthProvier: externalLoginInfo.Provider.Text, extras: $"found existing user, logging in, primary account name: {appUser.UserName} - found with id: {externalLoginInfo.Id}");

                return await FinalizeExternalLogin(appUser, cookies, externalLoginInfo.Provider.Text, inviteId, returnUrl);
            }

            // Path 1 - user does not exist as a OAuth user directly or is not logged in.
            _adminLogger.Trace($"[OAUTH_HandleExternalLogin] - Could not find by external provider id - {externalLoginInfo.Id} - {externalLoginInfo.Provider.Value}");

            // it's possible the Email address is registered as primary NuvIoT account
            // but it's not associated with this OAuth login, if that's the case
            if (!String.IsNullOrEmpty(externalLoginInfo.Email))
                appUser = await _signInManager.UserManager.FindByEmailAsync(externalLoginInfo.Email);

            if (appUser != null)
            {
                _adminLogger.Trace($"[OAUTH_HandleExternalLogin] - Found user by email address - {externalLoginInfo.Id} - {externalLoginInfo.Provider.Value} - {externalLoginInfo.Email}");

                // we found it, no OAuth login, but matching email, associate and login.
                await _appUserManager.AssociateExternalLoginAsync(appUser.Id, externalLoginInfo, appUser.ToEntityHeader());
                // reload after associating the third party login.
                appUser = await _signInManager.UserManager.FindByEmailAsync(externalLoginInfo.Email);
                await _authLogManager.AddAsync(AuthLogTypes.OAuthAppendUserLogin, appUser.Id, appUser.Name, inviteId: inviteId, oauthProvier: externalLoginInfo.Provider.Text, extras: $"found by email, associating oauth account with email {externalLoginInfo.Email} and logging in.");

                if (!appUser.EmailConfirmed)
                    returnUrl = $"{CommonLinks.ConfirmEmail}?email={externalLoginInfo.Email.ToLower()}";
                else if (appUser.CurrentOrganization == null)
                    returnUrl = CommonLinks.CreateDefaultOrg;
                else if (!String.IsNullOrEmpty(appUser.CurrentOrganization.HomePage) && appUser.EmailConfirmed)
                    returnUrl = appUser.CurrentOrganization.HomePage;

                return await FinalizeExternalLogin(appUser, cookies, externalLoginInfo.Provider.Text, inviteId, returnUrl);
            }
            else
            {
                _adminLogger.Trace($"[OAUTH_HandleExternalLogin] - Did not find existing user, creating new one: {externalLoginInfo.Id} - {externalLoginInfo.Provider.Value} - {externalLoginInfo.Email}");

                //  User does not exist, go ahead and create a new one along with org.
                var newUser = new UserAdmin.Models.DTOs.RegisterUser()
                {
                    FirstName = externalLoginInfo.FirstName,
                    LastName = externalLoginInfo.LastName,
                    Email = externalLoginInfo.Email,
                    DeviceId = "N/A",
                    InviteId = inviteId,
                    AppInstanceId = "N/A",
                    Password = $"External123-{Guid.NewGuid().ToId().ToLower()}", // For an external account just generate a guid.  The user can change at a later time if they want.
                    AppId = "1844A92CDDDF4B59A3BB294A1524D93A", // The one, the only app id for NuvIoT.
                    ClientType = "WEBAPP",
                };

                var result = await _userRegistrationManager.CreateUserAsync(newUser, externalLogin: externalLoginInfo);
                if (!result.Successful)
                {
                    await _authLogManager.AddAsync(AuthLogTypes.CreateUserError, inviteId: inviteId, oauthProvier: externalLoginInfo.Provider.Text, extras: $"New User Created: {newUser.FirstName} {newUser.LastName} - {newUser.Email}");
                    _adminLogger.Trace($"[OAUTH_HandleEXternalLogin] - Could not create new user: {externalLoginInfo.Id} - {externalLoginInfo.Provider.Value} - {externalLoginInfo.Email} - {result.Errors.First().Message}");
                    return InvokeResult<string>.Create($"/auth/error?{result.ErrorMessage}");
                }

                appUser = result.Result.AppUser;
           
                _adminLogger.Trace($"[OAUTH_HandleEXternalLogin] - Created new user: {externalLoginInfo.Id} - {externalLoginInfo.Provider.Value} - {externalLoginInfo.Email} - {newUser.FirstName} {newUser.LastName}");
                //if (appUser.CurrentOrganization == null)
                //{
                //    var inUse = true;
                //    var idx = 0;
                //    var baseName = String.Join("", externalLoginInfo.LastName.Where(c => Char.IsLetter(c))).ToLower();
                //    var orgNamespace = baseName;
                //    while (inUse)
                //    {
                //        orgNamespace = baseName + (idx > 0 ? idx.ToString() : String.Empty);
                //        inUse = await _orgManager.QueryOrgNamespaceInUseAsync(orgNamespace);
                //        idx++;
                //    }

                //    _adminLogger.Trace($"[OAUTH_HandleEXternalLogin] - Found/Created Available OrgNamespace {orgNamespace}");

                //    var newOrg = new UserAdmin.ViewModels.Organization.CreateOrganizationViewModel()
                //    {
                //        Namespace = orgNamespace,
                //        NickName = $"{newUser.LastName} Organization"
                //    };

                //    var orgResult = await _orgManager.CreateNewOrganizationAsync(newOrg, result.Result.User);
                //    _adminLogger.Trace($"[OAUTH_HandleEXternalLogin] - Created New Organization {newOrg.NickName} - {orgNamespace}");
                //    await _authLogManager.AddAsync(AuthLogTypes.OAuthCreateOrg, result.Result.User.Id, result.Result.User.Text, orgResult.Result.Id, orgResult.Result.NickName, oauthProvier: externalLoginInfo.Provider.Text, extras: $"Organization Created for New User: {newOrg.NickName} ({orgNamespace})");
                //    appUser = await _appUserManager.GetUserByExternalLoginAsync(externalLoginInfo.Provider.Value, externalLoginInfo.Id);
                //}

                return await FinalizeExternalLogin(appUser, cookies, externalLoginInfo.Provider.Text, inviteId, returnUrl);
            }
        }

    }
}

