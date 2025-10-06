using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    internal class UserRegistrationManager : ManagerBase, IUserRegistrationManager
    {
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly IAdminLogger _adminLogger;
        private readonly IAppUserRepo _appUserRepo;
        private readonly ISecureStorage _secureStorage;
        private readonly IAppConfig _appConfig;
        private readonly IOrganizationManager _orgManager;
        private readonly IUserManager _userManager;
        private readonly ISignInManager _signInManager;
        private readonly IUserVerficationManager _userVerificationmanager;
        private readonly IAuthTokenManager _authTokenManager;
        private readonly IOrganizationRepo _orgRepo;

        public UserRegistrationManager(ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security, IAuthenticationLogManager authLogMgr, 
            IAdminLogger adminLogger, IAppUserRepo appUserRepo, ISecureStorage secureStorage, IOrganizationManager orgManager, IUserManager userManager, ISignInManager signInManager,
            IUserVerficationManager userVerificationManager, IAuthTokenManager authTokenManager, IOrganizationRepo orgRepo) : base(logger, appConfig, dependencyManager, security)
        {
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
            _orgManager = orgManager ?? throw new ArgumentNullException(nameof(orgManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userVerificationmanager = userVerificationManager ?? throw new ArgumentNullException(nameof(userVerificationManager));
            _authTokenManager = authTokenManager ?? throw new ArgumentNullException(nameof(authTokenManager));
            _orgRepo = orgRepo ?? throw new ArgumentNullException(nameof(orgRepo));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }


        bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public async Task<InvokeResult<CreateUserResponse>> CreateUserAsync(RegisterUser newUser, bool autoLogin = true, ExternalLogin externalLogin = null)
        {
            if (String.IsNullOrEmpty(newUser.Email))
            {
                await _authLogMgr.AddAsync(externalLogin == null ? Models.Security.AuthLogTypes.CreateEmailUser : Models.Security.AuthLogTypes.CreateExernalLoginUser, userName: "NOT PROVIDED", extras: $"Client Type: {newUser.ClientType}, Login Type {newUser.LoginType}");
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: "?", errors: "Email address not provided.");

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.RegMissingEmail.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegMissingEmail.ToErrorMessage());
            }

            newUser.Email = newUser.Email.Trim();

            var userName = EntityHeader.IsNullOrEmpty(newUser.EndUserAppOrg) ? newUser.Email : $"{newUser.Email}@{newUser.EndUserAppOrg.Id}";

            await _authLogMgr.AddAsync(externalLogin == null ? Models.Security.AuthLogTypes.CreateEmailUser : Models.Security.AuthLogTypes.CreateExernalLoginUser,
                userName: userName, oauthProvier: externalLogin?.Provider.ToString(), extras: $"Client Type: {newUser.ClientType}, Login Type: {newUser.LoginType}.");
            if (!IsValidEmail(newUser.Email))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Invalid email address");
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.AuthEmailInvalidFormat.ToErrorMessage());
            }

            var user = await _appUserRepo.FindByNameAsync(userName);
            if (user != null)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Email already exists");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.RegErrorUserExists.Message);
                if (externalLogin == null)
                {
                    return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegErrorUserExists.ToErrorMessage());
                }
                else
                {
                    return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegisterUserExists_3rdParty.ToErrorMessage());
                }
            }

            /* Need to check all these, if any fail, we want to aboart, we need to refactor this into the UserAdmin module :( */
            if (String.IsNullOrEmpty(newUser.AppId))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing app id");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.AuthMissingAppId.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.AuthMissingAppId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.ClientType))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing client type");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.AuthMissingClientType.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.AuthMissingClientType.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.DeviceId))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing device id");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.AuthMissingDeviceId.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.AuthMissingDeviceId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.FirstName))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing first name");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.RegMissingFirstLastName.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegMissingFirstLastName.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.LastName))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing last name");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.RegMissingLastName.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegMissingLastName.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.Password))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing password");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.RegMissingPassword.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegMissingPassword.ToErrorMessage());
            }

            var emailRegEx = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,12})+)$");
            if (!emailRegEx.Match(newUser.Email).Success)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: $"Invalid Email Address [{newUser.Email}]");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserRegistrationManager_CreateUserAsync]", UserAdminErrorCodes.RegInvalidEmailAddress.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegInvalidEmailAddress.ToErrorMessage());
            }

            Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - Prevalidation complete.");

            var appUser = new AppUser(newUser.Email, userName, $"{newUser.FirstName} {newUser.LastName}")
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                EndUserAppOrg = newUser.EndUserAppOrg,
                Customer = newUser.Customer,
                CustomerContact = newUser.CustomerContact
            };

            Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - Created app user.");

            if (externalLogin != null)
            {
                Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - 3rd party registration - start.");

                appUser.ExternalLogins.Add(externalLogin);
                appUser.PhoneNumberConfirmedForBilling = false;
                appUser.HasGeneratedPassword = true;

                if (!String.IsNullOrEmpty(externalLogin.OAuthToken))
                {
                    var result = await _secureStorage.AddSecretAsync(appUser.ToEntityHeader(), externalLogin.OAuthToken);
                    externalLogin.OAuthTokenSecretId = result.Result;
                    externalLogin.OAuthToken = String.Empty;
                }

                if (!String.IsNullOrEmpty(externalLogin.OAuthTokenVerifier))
                {
                    var result = await _secureStorage.AddSecretAsync(appUser.ToEntityHeader(), externalLogin.OAuthTokenVerifier);
                    externalLogin.OAuthTokenVerifierSecretId = result.Result;
                    externalLogin.OAuthTokenVerifier = String.Empty;
                }

                Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - 3rd party registration - complete.");
            }
            else
            {
                appUser.HasGeneratedPassword = false;
                Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - Not 3rd party registration.");
            }

            if (_appConfig.Environment == Environments.Local ||
                _appConfig.Environment == Environments.LocalDevelopment)
            {
                appUser.EmailConfirmed = true;
                appUser.PhoneNumberConfirmed = true;
            }

            var identityResult = await _userManager.CreateAsync(appUser, newUser.Password);
            if (!identityResult.Successful)
            {
                Console.WriteLine($"[UserRegistrationManager_CreateUserAsync] - Could not create user - {identityResult.ErrorMessage}.");
          
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, appUser.Id, appUser.UserName, errors: identityResult.ErrorMessage);
                return InvokeResult<CreateUserResponse>.FromInvokeResult(identityResult);
            }

            Console.WriteLine($"[UserRegistrationManager_CreateUserAsync] - Could not created user.");

            var createUserResponse = new CreateUserResponse()
            {
                AccessToken = "N/A",
                AccessTokenExpiresUTC = "N/A",
                RefreshToken = "N/A",
                AppInstanceId = "N/A",
                RefreshTokenExpiresUTC = "N/A",
                IsLockedOut = false,
                AppUser = appUser,
                User = appUser.ToEntityHeader(),
                Roles = new List<EntityHeader>(),
                RedirectPage = $"{CommonLinks.ConfirmEmail}?email={appUser.Email.ToLower()}"
            };

            if (_appConfig.Environment == Environments.Local ||
               _appConfig.Environment == Environments.LocalDevelopment)
            {
                createUserResponse.RedirectPage = CommonLinks.Home;
            }

            appUser.LoginType = newUser.LoginType;

            if (!String.IsNullOrEmpty(newUser.InviteId))
            {
                Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - invitation handling started.");
          
                var response = await _orgManager.AcceptInvitationAsync(newUser.InviteId, appUser);
                if (!String.IsNullOrEmpty(response.Result.RedirectPage))
                    createUserResponse.RedirectPage = response.Result.RedirectPage;

                createUserResponse.ResponseMessage = response.Result.ResponseMessage;

                // If we sent them an email and the used that same one to register, they have
                // access to that email by definition.
                if (appUser.Email.ToLower() == response.Result.OriginalEmail.ToLower())
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.AutoConfirmEmail, appUser, extras: $"email: {newUser.Email}.");
                    appUser.EmailConfirmed = true;
                }

                if (!EntityHeader.IsNullOrEmpty(appUser.Customer) && !EntityHeader.IsNullOrEmpty(appUser.CustomerContact))
                {
                    appUser.Customer = response.Result.Customer;
                    appUser.CustomerContact = response.Result.CustomerContact;
                    appUser.LoginType = LoginTypes.AppUser;
                }

                if (!EntityHeader.IsNullOrEmpty(appUser.EndUserAppOrg))
                    appUser.EndUserAppOrg = response.Result.EndUserAppOrg;

                Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - invitation handling completed.");
            }
            else
                Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - not invition handling.");

            await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "New User Registered", null, appUser.ToEntityHeader());

            // this is if we create a profileImage by registering them, they will not get an invite but they should be added to the org.
            if (!String.IsNullOrEmpty(newUser.OrgId))
            {
                var org = await _orgRepo.GetOrganizationAsync(newUser.OrgId);
                var orgEH = new EntityHeader() { Id = newUser.OrgId, Text = newUser.FirstName + " " + newUser.LastName };
                await _orgManager.AddUserToOrgAsync(newUser.OrgId, appUser.Id, org.ToEntityHeader(), orgEH);
                appUser.CurrentOrganization = org.CreateSummary();

                if (!String.IsNullOrEmpty(org.HomePage))
                {
                    createUserResponse.RedirectPage = org.HomePage;
                }
            }

            createUserResponse.IsSetupComplete = appUser.EmailConfirmed && appUser.CurrentOrganization != null;

            await _appUserRepo.UpdateAsync(appUser);

            if (autoLogin)
            {
                Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - Auto-SignIn - Start.");
                await _signInManager.SignInAsync(appUser);
                Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - Auto-SignIn - Complete.");
            }

            Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - Send Email Confirmation - Start.");
            var sendEmailResult = await _userVerificationmanager.SendConfirmationEmailAsync(appUser.ToEntityHeader());
            if (!sendEmailResult.Successful)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, appUser, errors: sendEmailResult.ErrorMessage, extras: $"Submitted by client: {newUser.ClientType}.");
                return InvokeResult<CreateUserResponse>.FromInvokeResult(sendEmailResult.ToInvokeResult());
            }
            Console.WriteLine("[UserRegistrationManager_CreateUserAsync] - Send Email Confirmation - Complete.");

            if (newUser.ClientType != "WEBAPP")
            {
                var authRequest = new AuthRequest()
                {
                    AppId = newUser.AppId,
                    DeviceId = newUser.DeviceId,
                    AppInstanceId = newUser.AppInstanceId,
                    ClientType = newUser.ClientType,
                    GrantType = "password",
                    Email = newUser.Email,
                    UserName = newUser.Email,
                    Password = newUser.Password,
                };

                var tokenResponse = await _authTokenManager.AccessTokenGrantAsync(authRequest);
                if (!tokenResponse.Successful)
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, appUser, errors: tokenResponse.ErrorMessage, extras: $"Client Type: {newUser.ClientType}.");
                    var failedValidationResult = new InvokeResult<CreateUserResponse>();
                    failedValidationResult.Concat(tokenResponse);
                    return failedValidationResult;
                }

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserSuccess, appUser, redirectUri: createUserResponse.RedirectPage, extras: $"Submitted by client: {newUser.ClientType}.");

                return InvokeResult<CreateUserResponse>.Create(CreateUserResponse.FromAuthResponse(tokenResponse.Result), createUserResponse.RedirectPage);
            }
            else
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserSuccess, appUser, redirectUri: createUserResponse.RedirectPage, extras: $"Submitted by client: {newUser.ClientType}.");

                /* If we are logging in as web app, none of this applies */
                return InvokeResult<CreateUserResponse>.Create(createUserResponse, createUserResponse.RedirectPage);
            }
        }
    }
}
