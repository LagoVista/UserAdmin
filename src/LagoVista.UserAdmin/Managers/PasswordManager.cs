//#define DIAG

using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Resources;
using System;
using LagoVista.Core;
using System.Threading.Tasks;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Security;

namespace LagoVista.UserAdmin.Managers
{
    public class PasswordManager : ManagerBase, IPasswordManager
    {
        IAdminLogger _adminLogger;
        IAppConfig _appConfig;
        IEmailSender _emailSender;
        IUserManager _userManager;
        IAuthRequestValidators _authRequestValidators;
        private readonly IAuthenticationLogManager _authLogMgr;

        public const string ACTION_RESET_PASSWORD = "/Account/ResetPassword";


        public PasswordManager(IAuthRequestValidators authRequestValidators, IUserManager userManager, IEmailSender emailSender, IDependencyManager depManager, ISecurity security, IAuthenticationLogManager authLogMgr, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _adminLogger = logger;
            _emailSender = emailSender;
            _appConfig = appConfig;
            _userManager = userManager;
            _authRequestValidators = authRequestValidators;
            _authLogMgr = authLogMgr;
        }

        //In some cases, this will be called from API, we don't want to return API as part of the link.
        private String GetWebURI()
        {
            var environment = _appConfig.WebAddress;
            if (_appConfig.WebAddress.ToLower().Contains("api"))
            {
                switch (_appConfig.Environment)
                {
                    case Environments.Development: environment = "https://dev.nuviot.com"; break;
                    case Environments.Testing: environment = "https://test.nuviot.com"; break;
                    case Environments.Beta: environment = "https://qa.nuviot.com"; break;
                    case Environments.Staging: environment = "https://stage.nuviot.com"; break;
                    case Environments.Production: environment = "https://www.nuviot.com"; break;
                }
            }

            return environment;
        }

        public async Task<InvokeResult> SendResetPasswordLinkAsync(SendResetPasswordLink sendResetPasswordLink)
        {
            var validationResult = _authRequestValidators.ValidateSendPasswordLinkRequest(sendResetPasswordLink);
            if (!validationResult.Successful) return validationResult;

            var appUser = await _userManager.FindByEmailAsync(sendResetPasswordLink.Email);

            if (appUser == null)
            {
                _adminLogger.AddError("PasswordManager_SendResetPasswordLinkAsync", "CouldNotFindUser", new System.Collections.Generic.KeyValuePair<string, string>("email", sendResetPasswordLink.Email));
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_ResetPwd_CouldNotFindUser));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var callbackUrl = $"{GetWebURI()}{ACTION_RESET_PASSWORD}?code={encodedToken}";
            var mobileCallbackUrl = $"nuviot://resetpassword?code={token}";
#if DIAG
            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "PasswordManager_SendResetPasswordLinkAsync", "SentToken",
                 token.ToKVP("token"),
                 appUser.Id.ToKVP("appUserId"),
                 encodedToken.ToKVP("encodedToken"),
                 appUser.Email.ToKVP("toEmailAddress"));
#endif 

            var subject = UserAdminResources.Email_ResetPassword_Subject.Replace("[APP_NAME]", _appConfig.AppName);
            var body = UserAdminResources.Email_ResetPassword_Body.Replace("[CALLBACK_URL]", callbackUrl).Replace("[MOBILE_CALLBACK_URL]", mobileCallbackUrl);

            var result = await _emailSender.SendAsync(sendResetPasswordLink.Email, subject, body);
            if (result.Successful)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "PasswordManager_SendResetPasswordLinkAsync", "SentLink",
                     appUser.Id.ToKVP("appUserId"),
                     appUser.Email.ToKVP("toEmailAddress"));

                var org = appUser.CurrentOrganization == null ? EntityHeader.Create(Guid.Empty.ToId(), "????") : appUser.CurrentOrganization.ToEntityHeader();

                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "SentResetPasswordLink", org, appUser.ToEntityHeader());
            }
            else
            {
                _adminLogger.AddError("PasswordManager_SendResetPasswordLinkAsync", "Could Not Send Password Link", result.ErrorsToKVPArray());
            }

            return result;
        }

        public async Task<InvokeResult> ChangePasswordAsync(ChangePassword changePassword, EntityHeader orgEntityHeader, EntityHeader userEntityHeader)
        {
            var validationResult = _authRequestValidators.ValidatePasswordChangeRequest(changePassword, userEntityHeader.Id);
            if (!validationResult.Successful) return validationResult;

            var appUser = await _userManager.FindByIdAsync(userEntityHeader.Id);
            if (appUser == null)
            {
                _adminLogger.AddError("PasswordManager_ChangePasswordAsync", "CouldNotFindUser", new System.Collections.Generic.KeyValuePair<string, string>("id", userEntityHeader.Id));
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_CouldNotFindUser));
            }

            var result = await _userManager.ChangePasswordAsync(appUser, changePassword.OldPassword, changePassword.NewPassword);
            if (result.Successful)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "PasswordManager_ChangePasswordAsync", "PasswordChange",
                 appUser.Id.ToKVP("appUserId"),
                 appUser.Email.ToKVP("userEmailAddress"));

                var org = appUser.CurrentOrganization == null ? EntityHeader.Create(Guid.Empty.ToId(), "????") : appUser.CurrentOrganization.ToEntityHeader();
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "ChangePassword", org, appUser.ToEntityHeader());
                await _authLogMgr.AddAsync(AuthLogTypes.ChangePasswordSuccess, appUser.Id, appUser.Name, org.Id, org.Text);
            }
            else
            {
                _adminLogger.AddError("PasswordManager_ChangePasswordAsync", "Could Not Chance Password", result.ErrorsToKVPArray());
            }

            return result;
        }

        public async Task<InvokeResult> ResetPasswordAsync(ResetPassword resetPassword)
        {
            var validationResult = _authRequestValidators.ValidateResetPasswordRequest(resetPassword);
            if (!validationResult.Successful) return validationResult;

            var appUser = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (appUser == null)
            {
                _adminLogger.AddError("PasswordManager_ResetPasswordAsync", "CouldNotFindUser", new System.Collections.Generic.KeyValuePair<string, string>("resetPwdEmail", resetPassword.Email));
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_CouldNotFindUser));
            }

#if DIAG
            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "PasswordManager_ResetPasswordAsync", "ReceivedToken",
                 resetPassword.Token.ToKVP("token"),
                 appUser.Id.ToKVP("appUserId"),
                 appUser.Email.ToKVP("toEmailAddress"));
#endif 
            
            var result = await _userManager.ResetPasswordAsync(appUser, resetPassword.Token, resetPassword.NewPassword);
            if (result.Successful)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "PasswordManager_ResetPasswordAsync", "PasswordChange",
                 appUser.Id.ToKVP("appUserId"),
                 appUser.Email.ToKVP("userEmailAddress"));

                var org = appUser.CurrentOrganization == null ? EntityHeader.Create(Guid.Empty.ToId(), "????") : appUser.CurrentOrganization.ToEntityHeader();
                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "RestPassword", org, appUser.ToEntityHeader());
            }
            else
            {
                _adminLogger.AddError("PasswordManager_ResetPasswordAsync", "Could Not Reset Password", result.ErrorsToKVPArray());
            }

            return result;
        }
    }
}
