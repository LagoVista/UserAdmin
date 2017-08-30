using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.Core.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Resources;
using System.Text.RegularExpressions;
using LagoVista.UserAdmin.Interfaces.Managers;

namespace LagoVista.UserAdmin.Managers
{
    public class AppUserManager : ManagerBase, IAppUserManager
    {
        IAppUserRepo _appUserRepo;
        IAdminLogger _adminLogger;
        IAuthTokenManager _authTokenManager;
        IUserManager _userManager;
        ISignInManager _signInManager;
        

        public AppUserManager(IAppUserRepo appUserRepo, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig,
            IAuthTokenManager authTokenManager, IUserManager userManager, ISignInManager signInManager, IAdminLogger adminLogger) : base(logger, appConfig, depManager, security)
        {
            _appUserRepo = appUserRepo;
            _adminLogger = logger;
            _authTokenManager = authTokenManager;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<InvokeResult> AddUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUser)
        {
            ValidationCheck(user, Actions.Create);

            await AuthorizeAsync(user, AuthorizeResult.AuthorizeActions.Create, updatedByUser, org);

            await _appUserRepo.CreateAsync(user);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Read, user, org);

            return await CheckForDepenenciesAsync(appUser);
        }

        public async Task<InvokeResult> DeleteUserAsync(String  id, EntityHeader org, EntityHeader deletedByUser)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Delete, deletedByUser, org);      
            await _appUserRepo.DeleteAsync(appUser);

            return InvokeResult.Success;
        }

        public async Task<AppUser> GetUserByIdAsync(string id, EntityHeader org, EntityHeader requestedByUser)
        {
            var appUser = await _appUserRepo.FindByIdAsync(id);
            appUser.PasswordHash = null;

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Read, requestedByUser, org);
            return appUser;
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName, EntityHeader org, EntityHeader requestedByUser)
        {            
            var appUser = await _appUserRepo.FindByNameAsync(userName);
            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Read, requestedByUser, org);

            appUser.PasswordHash = null;
            return appUser;
        }

        public Task<IEnumerable<EntityHeader>> SearchUsers(string firstName, string lastName, EntityHeader searchedBy)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> UpdateUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUser)
        {
            ValidationCheck(user, Actions.Update);

            await AuthorizeAsync(user, AuthorizeResult.AuthorizeActions.Update, updatedByUser, org);

            await _appUserRepo.UpdateAsync(user);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateUserAsync(UserInfo user, EntityHeader org, EntityHeader updatedByUser)
        {
            var appUser = await GetUserByIdAsync(user.Id, org, updatedByUser);
            appUser.FirstName = user.FirstName;
            appUser.LastName = user.LastName;
            appUser.ProfileImageUrl = user.ProfileImageUrl;
            if(appUser.IsSystemAdmin != user.IsSystemAdmin)
            {
                var updateByAppUser = await GetUserByIdAsync(updatedByUser.Id, org, updatedByUser);
                if(!updateByAppUser.IsSystemAdmin)
                {
                    _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_UpdateUserAsync", UserAdminErrorCodes.AuthNotSysAdmin.Message);
                    return InvokeResult.FromErrors(UserAdminErrorCodes.AuthNotSysAdmin.ToErrorMessage());
                }
                appUser.IsSystemAdmin = user.IsSystemAdmin;
            }


            ValidationCheck(appUser, Actions.Update);

            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Update, updatedByUser, org);

            await _appUserRepo.UpdateAsync(appUser);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<AuthResponse>> CreateUserAsync(RegisterUser newUser)
        {
            /* Need to check all these, if any fail, we want to aboart, we need to refactor this into the UserAdmin module :( */
            if (String.IsNullOrEmpty(newUser.AppId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateNewAsync", UserAdminErrorCodes.AuthMissingAppId.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingAppId.ToErrorMessage());
            }


            if (String.IsNullOrEmpty(newUser.ClientType))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateNewAsync", UserAdminErrorCodes.AuthMissingClientType.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingClientType.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.DeviceId))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateNewAsync", UserAdminErrorCodes.AuthMissingDeviceId.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.AuthMissingDeviceId.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.FirstName))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateNewAsync", UserAdminErrorCodes.RegMissingFirstLastName.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.RegMissingFirstLastName.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.LastName))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateNewAsync", UserAdminErrorCodes.RegMissingLastName.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.RegMissingLastName.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.Email))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateNewAsync", UserAdminErrorCodes.RegMissingEmail.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.RegMissingEmail.ToErrorMessage());
            }

            if (String.IsNullOrEmpty(newUser.Password))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateNewAsync", UserAdminErrorCodes.RegMissingEmail.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.RegMissingEmail.ToErrorMessage());
            }

            var emailRegEx = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if (!emailRegEx.Match(newUser.Email).Success)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserServicesController_CreateNewAsync", UserAdminErrorCodes.RegInvalidEmailAddress.Message);
                return InvokeResult<AuthResponse>.FromErrors(UserAdminErrorCodes.RegInvalidEmailAddress.ToErrorMessage());
            }

            var lagoVistaUser = new AppUser(newUser.Email, $"{newUser.FirstName} {newUser.LastName}")
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
            };

            var identityResult = await _userManager.CreateAsync(lagoVistaUser, newUser.Password);
            if (identityResult.Successful)
            {
                await _signInManager.SignInAsync(lagoVistaUser);
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
                    return InvokeResult<AuthResponse>.Create(tokenResponse.Result);
                }
                else
                {
                    var failedValidationResult = new InvokeResult<AuthResponse>();
                    failedValidationResult.Concat(tokenResponse);
                    return failedValidationResult;
                }
            }
            else
            {
                return InvokeResult<AuthResponse>.FromInvokeResult(identityResult);
            }
        }
    }
}