using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Resources;
using System.Text.RegularExpressions;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.Core;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Exceptions;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System.Linq;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.Core.Models.ML;

namespace LagoVista.UserAdmin.Managers
{
    public class AppUserManager : AppUserManagerReadOnly, IAppUserManager
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly IOrganizationManager _orgManager;
        private readonly IAdminLogger _adminLogger;
        private readonly IAuthTokenManager _authTokenManager;
        private readonly IUserManager _userManager;
        private readonly ISignInManager _signInManager;
        private readonly IUserVerficationManager _userVerificationmanager;
        private readonly IAppConfig _appConfig;
        private readonly IOrgUserRepo _orgUserRepo;
        private readonly IOrganizationRepo _orgRepo;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ISecureStorage _secureStorage;
        private readonly IAuthenticationLogManager _authLogMgr;

        public AppUserManager(IAppUserRepo appUserRepo, IUserRoleRepo userRoleRepo, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IOrganizationManager orgManager, IOrgUserRepo orgUserRepo, IAppConfig appConfig, IUserVerficationManager userVerificationmanager,
           IOrganizationRepo orgRepo, IAuthTokenManager authTokenManager, ISubscriptionManager subscriptionManager, IUserManager userManager, ISecureStorage secureStorage,
           IAuthenticationLogManager authLogMgr, ISignInManager signInManager, IAdminLogger adminLogger) : base(appUserRepo, userRoleRepo, depManager, security, logger, appConfig)
        {
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _orgRepo = orgRepo ?? throw new ArgumentNullException(nameof(orgRepo));
            _orgManager = orgManager ?? throw new ArgumentNullException(nameof(orgManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _orgUserRepo = orgUserRepo ?? throw new ArgumentNullException(nameof(orgUserRepo));
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authTokenManager = authTokenManager ?? throw new ArgumentNullException(nameof(authTokenManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _userVerificationmanager = userVerificationmanager ?? throw new ArgumentNullException(nameof(userVerificationmanager));
            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
        }

        public async Task<InvokeResult> AddUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUser)
        {
            ValidationCheck(user, Actions.Create);
            await AuthorizeAsync(user, AuthorizeResult.AuthorizeActions.Create, updatedByUser, org);

            if (!String.IsNullOrEmpty(user.Ssn))
            {
                var result = await _secureStorage.AddUserSecretAsync(user.ToEntityHeader(), user.Ssn);
                if (!result.Successful)
                    return result.ToInvokeResult();

                user.SsnSecretId = result.Result;
                user.Ssn = null;
            }

            await _appUserRepo.CreateAsync(user);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Read, user, org);

            return await CheckForDepenenciesAsync(appUser);
        }

        public async Task<InvokeResult> DeleteUserAsync(String id, EntityHeader org, EntityHeader deletedByUser)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            if (id != deletedByUser.Id)
            {
                var deletedUser = await _appUserRepo.FindByIdAsync(deletedByUser.Id);
                if (!deletedUser.IsOrgAdmin)
                {
                    return InvokeResult.FromError($"Can not delete user, user deleting user must be the same user being deleted, or deleting user must be an org admin.");
                }
            }

            var appUser = await _appUserRepo.FindByIdAsync(id);
            if (appUser == null) throw new RecordNotFoundException(nameof(AppUser), id);
            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Delete, deletedByUser, org);

            var users = await _orgUserRepo.GetUsersForOrgAsync(org.Id);
            if (users.Count() == 1 && users.First().UserId == id)
            {
                await _subscriptionManager.DeleteSubscriptionsForOrgAsync(org.Id, org, deletedByUser);
                await _orgRepo.DeleteOrgAsync(org.Id);
            }
            else
            {
                var orgs = await _orgRepo.GetBillingContactOrgsForUserAsync(id);
                if (orgs.Any())
                {
                    var orgList = String.Join(",", orgs);
                    return InvokeResult.FromError($"Can not delete user, user is billing contact for the following org[s] {orgList}");
                }
            }


            var userOrgs = await _orgUserRepo.GetOrgsForUserAsync(id);
            foreach (var userOrg in userOrgs)
            {
                await _orgUserRepo.RemoveUserFromOrgAsync(userOrg.OrgId, id, deletedByUser);
            }

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "AppUserManager__DeleteuserAsync", $"{deletedByUser.Text} delete the user {appUser.Name}");

            await _appUserRepo.DeleteAsync(appUser);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateUserAsync(CoreUserInfo user, EntityHeader org, EntityHeader updatedByUser)
        {
            ValidationCheck(user, Actions.Update);

            var existingUser = await _appUserRepo.FindByIdAsync(user.Id);
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.EmailSignature = user.EmailSignature;
            existingUser.Bio = user.Bio;
            existingUser.Title = user.Title;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.TeamsAccountName = user.TeamsAccountName;
            existingUser.Address1 = user.Address1;
            existingUser.Address2 = user.Address2;
            existingUser.City = user.City;
            existingUser.State = user.State;
            existingUser.PostalCode = user.PostalCode;
            existingUser.Country = user.Country;

            if (!String.IsNullOrEmpty(user.Ssn))
            {

                if (user.Id != updatedByUser.Id)
                    await AuthorizeFinanceAdminAsync(updatedByUser, org, $"{nameof(UpdateUserAsync)}_Update_SSN_For_Other_User", user.Id);

                if (existingUser.SsnSecretId != null)
                    await _secureStorage.RemoveUserSecretAsync(existingUser.ToEntityHeader(), existingUser.SsnSecretId);

                var result = await _secureStorage.AddUserSecretAsync(existingUser.ToEntityHeader(), user.Ssn);
                if (!result.Successful)
                    return result.ToInvokeResult();

                existingUser.SsnSecretId = result.Result;
                existingUser.Ssn = null;
            }

            await AuthorizeAsync(existingUser, AuthorizeResult.AuthorizeActions.Update, updatedByUser, org);

            await _appUserRepo.UpdateAsync(existingUser);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUser)
        {
            ValidationCheck(user, Actions.Update);

            await AuthorizeAsync(user, AuthorizeResult.AuthorizeActions.Update, updatedByUser, org);

            if (!String.IsNullOrEmpty(user.Ssn))
            {

                if (user.Id != updatedByUser.Id)
                    await AuthorizeFinanceAdminAsync(updatedByUser, org, $"{nameof(UpdateUserAsync)}_Update_SSN_For_Other_User", user.Id);

                if (user.SsnSecretId != null)
                    await _secureStorage.RemoveUserSecretAsync(user.ToEntityHeader(), user.SsnSecretId);

                var result = await _secureStorage.AddUserSecretAsync(user.ToEntityHeader(), user.Ssn);
                if (!result.Successful)
                    return result.ToInvokeResult();

                user.SsnSecretId = result.Result;
                user.Ssn = null;
            }

            await _appUserRepo.UpdateAsync(user);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateUserAsync(UserInfo user, EntityHeader org, EntityHeader updatedByUser)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!string.IsNullOrEmpty(user.EmailSignature)) appUser.EmailSignature = user.EmailSignature;
            if (!String.IsNullOrEmpty(user.FirstName)) appUser.FirstName = user.FirstName;
            if (!String.IsNullOrEmpty(user.LastName)) appUser.LastName = user.LastName;
            if (!String.IsNullOrEmpty(user.PhoneNumber))
            {
                appUser.PhoneNumber = user.PhoneNumber;
                appUser.PhoneNumberConfirmed = true;
            }

            if ((user.ProfileImageUrl != null)) appUser.ProfileImageUrl = user.ProfileImageUrl;

            if (!String.IsNullOrEmpty(user.Ssn))
            {
                if (user.Id != updatedByUser.Id)
                    await AuthorizeFinanceAdminAsync(updatedByUser, org, $"{nameof(UpdateUserAsync)}_Update_SSN_For_Other_User", user.Id);

                if (appUser.SsnSecretId != null)
                    await _secureStorage.RemoveUserSecretAsync(user.ToEntityHeader(), appUser.SsnSecretId);

                var result = await _secureStorage.AddUserSecretAsync(user.ToEntityHeader(), user.Ssn);
                if (!result.Successful)
                    return result.ToInvokeResult();

                appUser.SsnSecretId = result.Result;
            }


            appUser.ShowWelcome = user.ShowWelcome;
            appUser.Notes = user.Notes;
            appUser.LastUpdatedBy = updatedByUser;
            appUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            if (!String.IsNullOrEmpty(user.TeamsAccountName))
            {
                appUser.TeamsAccountName = user.TeamsAccountName;
            }

            if (appUser.IsSystemAdmin != user.IsSystemAdmin)
            {
                var updateByAppUser = await GetUserByIdAsync(updatedByUser.Id, org, updatedByUser);
                if (updateByAppUser == null)
                {
                    return InvokeResult.FromError($"Could not find updating user with id: {updateByAppUser.Id}.");
                }

                if (!updateByAppUser.IsSystemAdmin)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_UpdateUserAsync", UserAdminErrorCodes.AuthNotSysAdmin.Message);
                    return InvokeResult.FromErrors(UserAdminErrorCodes.AuthNotSysAdmin.ToErrorMessage());
                }
                appUser.IsSystemAdmin = user.IsSystemAdmin;
                appUser.IsAppBuilder = user.IsAppBuilder;
                appUser.IsOrgAdmin = user.IsOrgAdmin;
                appUser.IsRuntimeuser = user.IsRuntimeUser;
                appUser.IsUserDevice = user.IsUserDevice;
            }

            ValidationCheck(appUser, Actions.Update);

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Update, updatedByUser, org);

            await _appUserRepo.UpdateAsync(appUser);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<PaymentAccounts>> GetPaymentAccountsAsync(string userId, EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(userId);
            var paymentAccount = new PaymentAccounts();

            if (user.Id != userId)
                await AuthorizeFinanceAdminAsync(user, org, $"{nameof(UpdateUserAsync)}_GetPaymentAccountsAsync_Other_User", user.Id);

            if (!string.IsNullOrEmpty(appUser.PaymentAccount1Secureid))
            {
                var result = await _secureStorage.GetSecretAsync(org, appUser.PaymentAccount1Secureid, user);
                if (!result.Successful) return InvokeResult<PaymentAccounts>.FromInvokeResult(result.ToInvokeResult());
                paymentAccount.PaymentAccount1 = result.Result;
            }

            if (!string.IsNullOrEmpty(appUser.PaymentAccount2Secureid))
            {
                var result = await _secureStorage.GetSecretAsync(org, appUser.PaymentAccount2Secureid, user);
                if (!result.Successful) return InvokeResult<PaymentAccounts>.FromInvokeResult(result.ToInvokeResult());
                paymentAccount.PaymentAccount2 = result.Result;
            }

            if (!string.IsNullOrEmpty(appUser.RoutingAccount1SecureId))
            {
                var result = await _secureStorage.GetSecretAsync(org, appUser.RoutingAccount1SecureId, user);
                if (!result.Successful) return InvokeResult<PaymentAccounts>.FromInvokeResult(result.ToInvokeResult());
                paymentAccount.RoutingAccount1 = result.Result;
            }

            if (!string.IsNullOrEmpty(appUser.RoutingAccount2SecureId))
            {
                var result = await _secureStorage.GetSecretAsync(org, appUser.RoutingAccount2SecureId, user);
                if (!result.Successful) return InvokeResult<PaymentAccounts>.FromInvokeResult(result.ToInvokeResult());
                paymentAccount.RoutingAccount2 = result.Result;
            }

            return InvokeResult<PaymentAccounts>.Create(paymentAccount);
        }

        public async Task<InvokeResult> UpdatePaymentAccountsAsync(string userId, PaymentAccounts accounts, EntityHeader org, EntityHeader user)
        {
            if (user.Id != userId)
                await AuthorizeFinanceAdminAsync(user, org, $"{nameof(UpdateUserAsync)}_UpdatePaymentAccountsAsync_Other_User", user.Id);

            if (accounts == null)
                throw new ArgumentNullException(nameof(accounts));

            var appUser = await _appUserRepo.FindByIdAsync(userId);

            if (appUser == null)
                throw new RecordNotFoundException(nameof(appUser), userId);

            if (!string.IsNullOrEmpty(accounts.PaymentAccount1))
            {
                var result = await _secureStorage.AddSecretAsync(org, accounts.PaymentAccount1);
                if (!result.Successful) return result.ToInvokeResult();
                appUser.PaymentAccount1Secureid = result.Result;
            }

            if (!string.IsNullOrEmpty(accounts.PaymentAccount2))
            {
                var result = await _secureStorage.AddSecretAsync(org, accounts.PaymentAccount2);
                if (!result.Successful) return result.ToInvokeResult();
                appUser.PaymentAccount2Secureid = result.Result;
            }

            if (!string.IsNullOrEmpty(accounts.RoutingAccount1))
            {
                var result = await _secureStorage.AddSecretAsync(org, accounts.RoutingAccount1);
                if (!result.Successful) return result.ToInvokeResult();
                appUser.RoutingAccount1SecureId = result.Result;
            }

            if (!string.IsNullOrEmpty(accounts.RoutingAccount2))
            {
                var result = await _secureStorage.AddSecretAsync(org, accounts.RoutingAccount2);
                if (!result.Successful) return result.ToInvokeResult();
                appUser.RoutingAccount2SecureId = result.Result;
            }

            ValidationCheck(appUser, Actions.Update);

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Update, user, org);

            await _appUserRepo.UpdateAsync(appUser);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> SetApprovedAsync(string userId, EntityHeader org, EntityHeader approvingUser)
        {
            await AuthorizeAsync(approvingUser, org, "SetApporvedStatus", userId);

            var appUser = await GetUserByIdAsync(approvingUser.Id, org, approvingUser);
            if (appUser == null)
            {
                return InvokeResult.FromError($"Could not find approving user with id: {approvingUser.Id}.");
            }

            if (appUser.CurrentOrganization.Id != org.Id)
            {
                return InvokeResult.FromError("Org Mismatch on current user.");
            }

            if (!appUser.IsOrgAdmin)
            {
                return InvokeResult.FromError("Must be an org admin to automically approve a user.");
            }



            var user = await _appUserRepo.FindByIdAsync(userId);
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            user.IsAccountDisabled = false;

            var fullOrg = await _orgManager.GetOrganizationAsync(org.Id, org, approvingUser);

            user.CurrentOrganization = fullOrg.CreateSummary();

            await _appUserRepo.UpdateAsync(user);

            await LogEntityActionAsync(userId, typeof(AppUser).Name, "Auto Approved", org, approvingUser);
            await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.ManualApproveUser, userId, appUser.UserName, appUser.CurrentOrganization?.Id, appUser.CurrentOrganization?.Text);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DisableAccountAsync(string userId, EntityHeader org, EntityHeader adminUser)
        {
            await AuthorizeAsync(adminUser, org, "DisabledUser", userId);

            var appUser = await GetUserByIdAsync(adminUser.Id, org, adminUser);
            if (appUser == null)
            {
                return InvokeResult.FromError($"Could not find admin user with id: {adminUser.Id}.");
            }

            if (appUser.CurrentOrganization.Id != org.Id)
            {
                return InvokeResult.FromError("Org Mismatch on current user.");
            }

            if (!appUser.IsOrgAdmin)
            {
                return InvokeResult.FromError("Must be an org admin to disable a user.");
            }

            var user = await _appUserRepo.FindByIdAsync(userId);
            user.IsAccountDisabled = true;
            await _appUserRepo.UpdateAsync(user);
            await LogEntityActionAsync(userId, typeof(AppUser).Name, "Disabe User Account", org, adminUser);
            await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.DisableUser, appUser.UserName, appUser.CurrentOrganization?.Id, appUser.CurrentOrganization?.Text);

            return InvokeResult.Success;
        }

        public async Task<ListResponse<UserInfoSummary>> GetDeviceUsersAsync(string deviceRepoId, EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeAsync(user, org, "GetDeviceUsersAsync", deviceRepoId);

            return await _appUserRepo.GetDeviceUsersAsync(deviceRepoId, listRequest);
        }

        public async Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeAsync(user, org, "GetAllUsersAsync", nameof(AppUser));

            return await _appUserRepo.GetAllUsersAsync(listRequest);
        }

        public async Task<ListResponse<UserInfoSummary>> GetActiveUsersAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeAsync(user, org, "GetAllUsersAsync", nameof(AppUser));

            return await _appUserRepo.GetActiveUsersAsync(listRequest);
        }


        public async Task<InvokeResult<CreateUserResponse>> CreateUserAsync(RegisterUser newUser, bool sendAuthEmail = true, bool autoLogin = true, ExternalLogin externalLogin = null)
        {
            if (String.IsNullOrEmpty(newUser.Email))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.RegMissingEmail.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegMissingEmail.ToErrorMessage());
            }

            var user = await _appUserRepo.FindByEmailAsync(newUser.Email);
            if (user != null)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Email already exists");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.RegErrorUserExists.Message);
                if (sendAuthEmail)
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
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.AuthMissingAppId.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.AuthMissingAppId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.ClientType))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing client type");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.AuthMissingClientType.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.AuthMissingClientType.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.DeviceId))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing device id");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.AuthMissingDeviceId.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.AuthMissingDeviceId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.FirstName))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing first name");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.RegMissingFirstLastName.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegMissingFirstLastName.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.LastName))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing last name");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.RegMissingLastName.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegMissingLastName.ToErrorMessage());
            }


            if (String.IsNullOrEmpty(newUser.Password))
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: "Missing password");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.RegMissingPassword.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegMissingPassword.ToErrorMessage());
            }

            var emailRegEx = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,12})+)$");
            if (!emailRegEx.Match(newUser.Email).Success)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, userName: newUser.Email, extras: $"Invalid Email Address [{newUser.Email}]");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateUserAsync", UserAdminErrorCodes.RegInvalidEmailAddress.Message);
                return InvokeResult<CreateUserResponse>.FromErrors(UserAdminErrorCodes.RegInvalidEmailAddress.ToErrorMessage());
            }

            var appUser = new AppUser(newUser.Email, $"{newUser.FirstName} {newUser.LastName}")
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
            };

            if (externalLogin != null)
            {
                appUser.ExternalLogins.Add(externalLogin);
                appUser.EmailConfirmed = true;
                appUser.PhoneNumberConfirmed = true;
                appUser.PhoneNumberConfirmedForBilling = false;

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

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.OAuthCreateUser, appUser.Id, appUser.UserName, oauthProvier: externalLogin.Provider.Text);
            }
            else
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateEmailUser, appUser.Id, appUser.UserName);
            }

            var identityResult = await _userManager.CreateAsync(appUser, newUser.Password);
            if (!identityResult.Successful)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.CreateUserError, appUser.Id, appUser.UserName, errors: identityResult.ErrorMessage);
                return InvokeResult<CreateUserResponse>.FromInvokeResult(identityResult);
            }

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
                RedirectPage = "/auth/org/createdefault"
            };

            if (!String.IsNullOrEmpty(newUser.InviteId))
            {
                var response = await _orgManager.AcceptInvitationAsync(newUser.InviteId, appUser);

                createUserResponse.RedirectPage = response.Result.RedirectPage;
                createUserResponse.ResponseMessage = response.Result.ResponseMessage;
            }

            await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "New User Registered", null, appUser.ToEntityHeader());

            if (autoLogin)
            {
                await _signInManager.SignInAsync(appUser);
            }

            if (!String.IsNullOrEmpty(newUser.OrgId))
            {
                var org = await _orgRepo.GetOrganizationAsync(newUser.OrgId);
                var orgEH = new EntityHeader() { Id = newUser.OrgId, Text = newUser.FirstName + " " + newUser.LastName };
                await _orgManager.AddUserToOrgAsync(newUser.OrgId, appUser.Id, org.ToEntityHeader(), orgEH);
                appUser.CurrentOrganization = org.CreateSummary();
                await _userManager.UpdateAsync(appUser);
            }

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
                if (tokenResponse.Successful)
                {
                    await _userVerificationmanager.SendConfirmationEmailAsync(null, appUser.ToEntityHeader());
                    return InvokeResult<CreateUserResponse>.Create(CreateUserResponse.FromAuthResponse(tokenResponse.Result));
                }
                else
                {
                    var failedValidationResult = new InvokeResult<CreateUserResponse>();
                    failedValidationResult.Concat(tokenResponse);
                    return failedValidationResult;
                }
            }
            else
            {
                if (sendAuthEmail)
                {
                    await _userVerificationmanager.SendConfirmationEmailAsync(null, appUser.ToEntityHeader());
                }

                /* If we are logging in as web app, none of this applies */
                return InvokeResult<CreateUserResponse>.Create(createUserResponse);
            }
        }

        public async Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(bool? emailConfirmed, bool? smsConfirmed, EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin to check for billing records.");
            }

            await AuthorizeAsync(user, org, "GetAllUsersAsync", nameof(AppUser));

            return await _appUserRepo.GetAllUsersAsync(listRequest, emailConfirmed, smsConfirmed);
        }

        public async Task<ListResponse<UserInfoSummary>> GetUsersWithoutOrgsAsync(EntityHeader user, ListRequest listRequest)
        {
            var appUser = await _appUserRepo.FindByIdAsync(user.Id);

            if (!appUser.IsSystemAdmin) // Eventually if we need to delete all the data && ((org.Id != orgId) || (org.Id == orgId && !appUser.IsOrgAdmin)))
            {
                //throw new NotAuthorizedException("Must be system admin or belong to the org and be an org admin for the org that is to be deleted, neither of these are the case.");
                throw new NotAuthorizedException("Must be a system admin to check for billing records.");
            }

            return await _appUserRepo.GetUsersWithoutOrgsAsync(listRequest);
        }

        public async Task<AppUser> AssociateExternalLoginAsync(string userId, ExternalLogin external, EntityHeader user)
        {
            if (userId != user.Id)
            {
                throw new NotAuthorizedException("User Id Mis-Match.");
            }

            if (!String.IsNullOrEmpty(external.OAuthToken))
            {
                var result = await _secureStorage.AddSecretAsync(user, external.OAuthToken);
                external.OAuthTokenSecretId = result.Result;
                external.OAuthToken = String.Empty;
            }

            if (!String.IsNullOrEmpty(external.OAuthTokenVerifier))
            {
                var result = await _secureStorage.AddSecretAsync(user, external.OAuthTokenVerifier);
                external.OAuthTokenVerifierSecretId = result.Result;
                external.OAuthTokenVerifier = String.Empty;
            }

            return await _appUserRepo.AssociateExternalLoginAsync(userId, external);
        }

        public async Task<InvokeResult<AppUser>> RemoveExternalLoginAsync(string userId, string externalLoginId, EntityHeader user)
        {
            if (userId != user.Id)
            {
                throw new NotAuthorizedException("User Id Mis-Match.");
            }

            await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.OAuthRemoveUserLogin, user.Id, user.Text);

            return InvokeResult<AppUser>.Create(await _appUserRepo.RemoveExternalLoginAsync(userId, externalLoginId));
        }


        public async Task<ExternalLogin> PopulateExternalLoginSecretsAsync(string userId, ExternalLogin external, EntityHeader user)
        {
            if (!String.IsNullOrEmpty(external.OAuthTokenSecretId))
            {
                var result = await _secureStorage.GetSecretAsync(user, external.OAuthTokenSecretId, user);
                external.OAuthToken = result.Result;
            }

            if (!String.IsNullOrEmpty(external.OAuthTokenVerifierSecretId))
            {
                var result = await _secureStorage.GetSecretAsync(user, external.OAuthTokenVerifierSecretId, user);
                external.OAuthTokenVerifier = result.Result;
            }

            return external;
        }


        public Task<AppUser> GetUserByExternalLoginAsync(ExternalLoginTypes loginType, string id)
        {
            return _appUserRepo.GetUserByExternalLoginAsync(loginType, id);
        }

        public async Task<InvokeResult> AddMediaResourceAsync(string userId, EntityHeader mediaResource, EntityHeader org, EntityHeader updatedByUser)
        {
            var user = await _appUserRepo.FindByIdAsync(userId);
            await AuthorizeAsync(user, AuthorizeResult.AuthorizeActions.Update, updatedByUser, org, nameof(AppUserManager.AddMediaResourceAsync));
            user.MediaResources.Add(mediaResource);
            await _appUserRepo.UpdateAsync(user);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<AppUser>> AcceptTermsAndConditionsAsync(string ipAddress, EntityHeader org, EntityHeader userEH)
        {
            var user = await _appUserRepo.FindByIdAsync(userEH.Id);
            await AuthorizeAsync(user, AuthorizeResult.AuthorizeActions.Update, userEH, org, nameof(AppUserManager.AcceptTermsAndConditionsAsync));
            user.TermsAndConditionsAccepted = true;
            user.TermsAndConditionsAcceptedDateTime = DateTime.UtcNow.ToJSONString();
            user.TermsAndConditionsAcceptedIPAddress = ipAddress;
            await _appUserRepo.UpdateAsync(user);

            await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.AcceptTermsAndConditions, userEH.Id, userEH.Text, extras: $"ip:{ipAddress}");

            return InvokeResult<AppUser>.Create(user);
        }

        public async Task<InvokeResult<string>> GetUserSSNAsync(string userId, EntityHeader org, EntityHeader user)
        {
            if (user.Id != userId)
                await AuthorizeFinanceAdminAsync(user, org, $"{nameof(UpdateUserAsync)}_Get_SSN_For_Other_User", user.Id);

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null)
                return InvokeResult<string>.FromError($"Could not find user with id: {user.Id}");

            if (String.IsNullOrEmpty(appUser.SsnSecretId))
                return InvokeResult<string>.FromError($"{appUser.Name} does not have a SSN stored.");

            return await _secureStorage.GetUserSecretAsync(appUser.ToEntityHeader(), appUser.SsnSecretId);
        }

        public async Task<InvokeResult> MarkUserViewedSystemSerialIndex(int idx, EntityHeader org, EntityHeader user)
        {
            var appUser = await _userManager.FindByIdAsync(user.Id);
            appUser.ViewedSystemNotificationIndex = idx;
            await _userManager.UpdateAsync(appUser);
            return InvokeResult.Success;
        }
    }
}