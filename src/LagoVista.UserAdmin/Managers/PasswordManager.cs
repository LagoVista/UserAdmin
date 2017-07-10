using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Resources;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class PasswordManager : ManagerBase, IPasswordManager
    {
        IAdminLogger _adminLogger;
        IAppConfig _appConfig;
        IEmailSender _emailSender;
        IUserManager _userManager;
        IAuthRequestValidators _authRequestValidators;

        public const string ACTION_RESET_PASSWORD = "/Account/ResetPassword";


        public PasswordManager(IAuthRequestValidators authRequestValidators, IUserManager userManager, IEmailSender emailSender, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _adminLogger = logger;
            _emailSender = emailSender;
            _appConfig = appConfig;
            _userManager = userManager;
            _authRequestValidators = authRequestValidators;
        }

        public async Task<InvokeResult> SendResetPasswordLinkAsync(SendResetPasswordLink sendResetPasswordLink)
        {
            var validationResult = _authRequestValidators.ValidateSendPasswordLinkRequest(sendResetPasswordLink);
            if(!validationResult.Successful) return validationResult;

            try
            {
                var appUser = await _userManager.FindByEmailAsync(sendResetPasswordLink.Email);
                if (appUser == null)
                {
                    _adminLogger.AddError("PasswordManager_SendResetPasswordLinkAsync", "CouldNotFindUser", new System.Collections.Generic.KeyValuePair<string, string>("email", sendResetPasswordLink.Email));
                    return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_ResetPwd_CouldNotFindUser));
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(appUser);
                var callbackUrl = $"{_appConfig.WebAddress}/{ACTION_RESET_PASSWORD}?code={code}";
                var mobileCallbackUrl = $"nuviot://resetpassword?code={code}";

                var subject = UserAdminResources.Email_ResetPassword_Subject.Replace("[APP_NAME]", _appConfig.AppName);
                var body = UserAdminResources.Email_ResetPassword_Body.Replace("[CALLBACK_URL]", callbackUrl).Replace("[MOBILE_CALLBACK_URL]", mobileCallbackUrl);

                return await _emailSender.SendAsync(sendResetPasswordLink.Email, subject, body);
            }
            catch (Exception ex)
            {
                _adminLogger.AddException("PasswordManager_SendResetPasswordLinkAsync", ex);
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Email_RestPassword_ErrorSending), new ErrorMessage() { Message = ex.Message });
            }
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

            return await _userManager.ChangePasswordAsync(appUser, changePassword.OldPassword, changePassword.NewPassword);
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

            return await _userManager.ResetPasswordAsync(appUser, resetPassword.Token, resetPassword.NewPassword);

        }
    }
}
