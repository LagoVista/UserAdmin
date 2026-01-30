// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 710724c8330f999f4e2175097ea50bc1d453a8ef94f3f8727b34df46bb05698a
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using LagoVista.UserAdmin.Models.Testing;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Utils;
using RingCentral;
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
// --- BEGIN: CreateUserAsync helper methods (block 1) ---
            private static void EnsureInviteOrOrgExclusive(RegisterUser newUser)
            {
                if (!String.IsNullOrEmpty(newUser.InviteId) && !String.IsNullOrEmpty(newUser.OrgId))
                {
                    throw new InvalidOperationException("InviteId and OrgId are mutually exclusive.");
                }
            }

            private static string ResolveUserName(RegisterUser newUser, ExternalLogin externalLogin)
            {
                if (externalLogin != null)
                {
                    if (String.IsNullOrEmpty(externalLogin.UserName))
                    {
                        throw new InvalidOperationException("ExternalLogin.UserName is required.");
                    }

                    return $"usr_{externalLogin.Provider.Text}_{externalLogin.UserName}".ToUpper();
                }

                // email-based username (existing behavior)
                if (String.IsNullOrEmpty(newUser.Email))
                {
                    return EntityHeader.IsNullOrEmpty(newUser.EndUserAppOrg)
                        ? RandomNameGenerator.Generate()
                        : $"{RandomNameGenerator.Generate()}@{newUser.EndUserAppOrg.Id}";
                }

                return EntityHeader.IsNullOrEmpty(newUser.EndUserAppOrg)
                    ? newUser.Email
                    : $"{newUser.Email}@{newUser.EndUserAppOrg.Id}";
            }

            private static void AddPendingInviteId(AppUser appUser, string inviteId)
            {
                if (String.IsNullOrEmpty(inviteId))
                {
                    return;
                }

                var existing = appUser.PendingInviteIds ?? Array.Empty<string>();

                // de-dupe
                foreach (var id in existing)
                {
                    if (String.Equals(id, inviteId, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                var updated = new string[existing.Length + 1];
                Array.Copy(existing, updated, existing.Length);
                updated[existing.Length] = inviteId;

                appUser.PendingInviteIds = updated;
            }
            // --- END: CreateUserAsync helper methods (block 1) ---


            // --- BEGIN: CreateUserAsync helper methods (block 2) ---
            private async Task<InvokeResult> ValidateCreateUserRequestAsync(RegisterUser newUser, ExternalLogin externalLogin, string userName)
            {
                // Core required fields (always)
                if (String.IsNullOrEmpty(newUser.AppId))
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing app id");
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.AuthMissingAppId.Message);
                    return InvokeResult.FromError(UserAdminErrorCodes.AuthMissingAppId.Message);
                }

                if (String.IsNullOrEmpty(newUser.ClientType))
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing client type");
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.AuthMissingClientType.Message);
                    return InvokeResult.FromError(UserAdminErrorCodes.AuthMissingClientType.Message);
                }

                if (String.IsNullOrEmpty(newUser.DeviceId))
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing device id");
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.AuthMissingDeviceId.Message);
                    return InvokeResult.FromError(UserAdminErrorCodes.AuthMissingDeviceId.Message);
                }

                // Email required only for non-external registrations
                if (newUser.Source == UserCreationSource.UserSelfRegistration && String.IsNullOrEmpty(newUser.Email))
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateEmailUser, userName: "NOT PROVIDED",
                        extras: $"Client Type: {newUser.ClientType}, Login Type {newUser.LoginType}");
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: "?", errors: "Email address not provided.");

                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.RegMissingEmail.Message);
                    return InvokeResult.FromError(UserAdminErrorCodes.RegMissingEmail.Message);
                }

                // First/Last required only for non-external registrations
                if (newUser.Source == UserCreationSource.UserSelfRegistration)
                {
                    if (String.IsNullOrEmpty(newUser.FirstName) || String.IsNullOrEmpty(newUser.LastName))
                    {
                        await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing first/last name");
                        _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.RegMissingFirstLastName.Message);
                        return InvokeResult.FromError(UserAdminErrorCodes.RegMissingFirstLastName.Message);
                    }
                }

                // Password required only for non-external registrations
                if (newUser.Source == UserCreationSource.UserSelfRegistration && String.IsNullOrEmpty(newUser.Password))
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing password");
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.RegMissingPassword.Message);
                    return InvokeResult.FromError(UserAdminErrorCodes.RegMissingPassword.Message);
                }

                // If email is present, validate it (both flows)
                if (!String.IsNullOrEmpty(newUser.Email))
                {
                    newUser.Email = newUser.Email.Trim();

                    if (!IsValidEmail(newUser.Email))
                    {
                        await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Invalid email address");
                        return InvokeResult.FromError(UserAdminErrorCodes.AuthEmailInvalidFormat.Message);
                    }

                    var emailRegEx = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,12})+)$");
                    if (!emailRegEx.Match(newUser.Email).Success)
                    {
                        await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: $"Invalid Email Address [{newUser.Email}]");
                        _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.RegInvalidEmailAddress.Message);
                        return InvokeResult.FromError(UserAdminErrorCodes.RegInvalidEmailAddress.Message);
                    }
                }

                return InvokeResult.Success;
            }
            // --- END: CreateUserAsync helper methods (block 2) ---

            // --- BEGIN: CreateUserAsync helper methods (block 3) ---
            private AppUser BuildAppUser(RegisterUser newUser, string userName, string defaultUserId)
            {
                var createdBy = $"{newUser.FirstName} {newUser.LastName}".Trim();
                if (String.IsNullOrEmpty(createdBy))
                {
                    createdBy = userName ?? "UNKNOWN";
                }

                var appUser = new AppUser(newUser.Email, userName, createdBy)
                {
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    EndUserAppOrg = newUser.EndUserAppOrg,
                    Customer = newUser.Customer,
                    CustomerContact = newUser.CustomerContact,
                    LoginType = newUser.LoginType,
                };

                if (!String.IsNullOrEmpty(defaultUserId))
                {
                    if (defaultUserId != TestUserSeed.User.Id)
                        throw new InvalidOperationException("Can only force the user id to the test user id to be used for testing");

                    appUser.Id = defaultUserId;
                }

                // simplified invite handling: just stash id(s) for later UI flow
                AddPendingInviteId(appUser, newUser.InviteId);

                return appUser;
            }
            // --- END: CreateUserAsync helper methods (block 3) ---

            // --- BEGIN: CreateUserAsync helper methods (block 4) ---
            private async Task ApplyExternalLoginAsync(AppUser appUser, ExternalLogin externalLogin)
            {
                if (externalLogin == null)
                {
                    appUser.HasGeneratedPassword = false;
                    return;
                }

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
            }

            // --- END: CreateUserAsync helper methods (block 4) ---

            // --- BEGIN: CreateUserAsync helper methods (block 5) ---
            private async Task<InvokeResult> CreateIdentityUserAsync(AppUser appUser, RegisterUser newUser, ExternalLogin externalLogin)
            {
                _adminLogger.Trace($"{this.Tag()} - Before User Manager - User Type {appUser.LoginType} {appUser.LoginTypeName} Creating User Email: {appUser.Email} and User Name: {appUser.UserName}");

                InvokeResult identityResult;
                if (newUser.Source == UserCreationSource.UserSelfRegistration)
                {
                    identityResult = await _userManager.CreateAsync(appUser, newUser.Password);
                }
                else
                {
                    identityResult = await _userManager.CreateAsync(appUser);
                }

                if (!identityResult.Successful)
                {
                    _adminLogger.AddError(this.Tag(), $"Could not create user - {identityResult.ErrorMessage}.");
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, appUser.Id, appUser.UserName, errors: identityResult.ErrorMessage);
                    return identityResult;
                }

                _adminLogger.Trace($"{this.Tag()} - After User Manager - Created User Email: {appUser.Email} and User Name: {appUser.UserName}");
                return InvokeResult.Success;
            }

            // --- BEGIN: CreateUserAsync helper methods (block 6) ---
            private async Task HandleOrgAssignmentAsync(RegisterUser newUser, AppUser appUser, CreateUserResponse response)
            {
                EnsureInviteOrOrgExclusive(newUser);

                if (String.IsNullOrEmpty(newUser.OrgId))
                {
                    return;
                }

                var org = await _orgRepo.GetOrganizationAsync(newUser.OrgId);

                // Keep existing behavior: add user to org + set current org summary
                var orgEH = new EntityHeader() { Id = newUser.OrgId, Text = $"{newUser.FirstName} {newUser.LastName}".Trim() };
                await _orgManager.AddUserToOrgAsync(newUser.OrgId, appUser.Id, org.ToEntityHeader(), orgEH);
                appUser.CurrentOrganization = org.CreateSummary();

                // Redirect can be simplified later; keep minimal, non-breaking behavior for now
                if (!String.IsNullOrEmpty(org.EndUserHomePage))
                {
                    response.RedirectPage = org.EndUserHomePage;
                }
                else if (!String.IsNullOrEmpty(org.HomePage))
                {
                    response.RedirectPage = org.HomePage;
                }
            }

            // --- BEGIN: CreateUserAsync helper methods (block 7) ---
            private async Task<InvokeResult> SendEmailConfirmationIfNeededAsync(RegisterUser newUser, AppUser appUser)
            {
                // Per new contract: if no email, skip completely
                if(newUser.Source != UserCreationSource.UserSelfRegistration)
                {
                    _adminLogger.Trace($"{this.Tag()} Send Email Confirmation - Skipped for source {newUser.Source}.");
                    return InvokeResult.Success;
                }

                var confirmSubject = "";
                var confirmBody = "";
                var appName = "";
                var appLogo = "";

                // If org is present, use org-provided email branding (existing behavior)
                if (!String.IsNullOrEmpty(newUser.OrgId))
                {
                    var org = await _orgRepo.GetOrganizationAsync(newUser.OrgId);

                    confirmSubject = org.EmailConfirmSubject;
                    confirmBody = org.EmailConfirmMessage;
                    appName = org.Name;

                    if (!EntityHeader.IsNullOrEmpty(org.LightLogo))
                    {
                        appLogo = $"{_appConfig.WebAddress.TrimEnd('/')}/api/media/resource/{org.Id}/{org.LightLogo.Id}/download";
                    }
                }

                _adminLogger.Trace($"{this.Tag()} Send Email Confirmation - Start.");
                var sendEmailResult = await _userVerificationmanager.SendConfirmationEmailAsync(appUser.Id, confirmSubject, confirmBody, appName, appLogo);
                if (!sendEmailResult.Successful)
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, appUser, errors: sendEmailResult.ErrorMessage, extras: $"Submitted by client: {newUser.ClientType}.");
                    return sendEmailResult.ToInvokeResult();
                }

                _adminLogger.Trace($"{this.Tag()} Send Email Confirmation - Complete.");
                return InvokeResult.Success;
            }

            private async Task FinalizeUserAsync(RegisterUser newUser, AppUser appUser, CreateUserResponse response, bool autoLogin)
            {
                await _appUserRepo.UpdateAsync(appUser);

                if (autoLogin)
                {
                    _adminLogger.Trace($"{this.Tag()} Auto-SignIn - Start.");
                    await _signInManager.SignInAsync(appUser);
                    _adminLogger.Trace($"{this.Tag()} Auto-SignIn - Complete.");
                }

                response.UserSetupState = appUser.GetUserSetupState();
            }

            private async Task<InvokeResult<CreateUserResponse>> IssueTokenIfNecessary(RegisterUser newUser,  ExternalLogin externalLogin, AppUser appUser,CreateUserResponse response, string userName)
            {
                if (externalLogin == null && newUser.ClientType != "WEBAPP")
                {
                    var authRequest = new AuthRequest()
                    {
                        AppId = newUser.AppId,
                        DeviceId = newUser.DeviceId,
                        AppInstanceId = newUser.AppInstanceId,
                        ClientType = newUser.ClientType,
                        GrantType = "password",
                        Email = newUser.Email,
                        UserName = userName,
                        Password = newUser.Password,
                    };

                    var tokenResponse = await _authTokenManager.AccessTokenGrantAsync(authRequest);
                    if (!tokenResponse.Successful)
                    {
                        await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, appUser, errors: tokenResponse.ErrorMessage, extras: $"Client Type: {newUser.ClientType}.");
                        var failed = new InvokeResult<CreateUserResponse>();
                        failed.Concat(tokenResponse);
                        return failed;
                    }

                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserSuccess, appUser, redirectUri: response.RedirectPage, extras: $"Submitted by client: {newUser.ClientType}.");

                    // Preserve prior behavior: return token-derived response
                    var tokenBased = CreateUserResponse.FromAuthResponse(tokenResponse.Result);
                    tokenBased.RedirectPage = response.RedirectPage;
                    tokenBased.ResponseMessage = response.ResponseMessage;
                    tokenBased.UserSetupState = response.UserSetupState;

                    return InvokeResult<CreateUserResponse>.Create(tokenBased, tokenBased.RedirectPage);
                }
                else
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserSuccess, appUser, redirectUri: response.RedirectPage, extras: $"Submitted by client: {newUser.ClientType}.");
                    return InvokeResult<CreateUserResponse>.Create(response, response.RedirectPage);
                }            
        }

            public async Task<InvokeResult<CreateUserResponse>> CreateUserAsync(RegisterUser newUser, bool autoLogin = true, ExternalLogin externalLogin = null, string defaultUserId = null)
            {
                if (externalLogin != null && newUser.ClientType != "WEBAPP")
                    throw new InvalidOperationException("External login registration is only supported for WEBAPP clients.");

                EnsureInviteOrOrgExclusive(newUser);

                var userName = ResolveUserName(newUser, externalLogin);

                // Validate request (implements external-login rules)
                var validation = await ValidateCreateUserRequestAsync(newUser, externalLogin, userName);
                if (!validation.Successful)
                {
                    return InvokeResult<CreateUserResponse>.FromInvokeResult(validation);
                }

                // If email is present (either flow), check for existing user by username
                if (!String.IsNullOrEmpty(userName))
                {
                    var existing = await _appUserRepo.FindByNameAsync(userName);
                    if (existing != null)
                    {
                        await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "User already exists");
                        _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), UserAdminErrorCodes.RegErrorUserExists.Message);

                        // Keep existing error selection behavior
                        if (externalLogin == null)
                        {
                            return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegErrorUserExists.ToErrorMessage());
                        }
                        else
                        {
                            return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegisterUserExists_3rdParty.ToErrorMessage());
                        }
                    }
                }

                _adminLogger.Trace($"{this.Tag()} - Prevalidation complete.");

                var appUser = BuildAppUser(newUser, userName, defaultUserId);
                await ApplyExternalLoginAsync(appUser, externalLogin);

                var createIdentityResult = await CreateIdentityUserAsync(appUser, newUser, externalLogin);
                if (!createIdentityResult.Successful)
                {
                    return InvokeResult<CreateUserResponse>.FromInvokeResult(createIdentityResult);
                }

                var response = new CreateUserResponse()
                {
                    AccessToken = "N/A",
                    AccessTokenExpiresUTC = "N/A",
                    RefreshToken = "N/A",
                    RefreshTokenExpiresUTC = "N/A",
                    AppInstanceId = "N/A",
                    IsLockedOut = false,
                    AppUser = appUser,
                    User = appUser.ToEntityHeader(),
                    Roles = new List<EntityHeader>(),
                    RedirectPage = String.Empty,
                };

                // Org assignment (InviteId is only stashed; OrgId is applied here)
                await HandleOrgAssignmentAsync(newUser, appUser, response);

                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "New User Registered", null, appUser.ToEntityHeader());

                // Email confirmation (skips if no email)
                var emailConfirmResult = await SendEmailConfirmationIfNeededAsync(newUser, appUser);
                if (!emailConfirmResult.Successful)
                {
                    return InvokeResult<CreateUserResponse>.FromInvokeResult(emailConfirmResult);
                }

                await FinalizeUserAsync(newUser, appUser, response, autoLogin);

                return await IssueTokenIfNecessary(newUser, externalLogin, appUser, response, userName);              
            }
    }
}
