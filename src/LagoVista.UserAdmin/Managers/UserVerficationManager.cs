using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Interfaces.Repos.Users;

namespace LagoVista.UserAdmin.Managers
{
    public class UserVerficationManager : ManagerBase, IUserVerficationManager
    {
        private readonly IAdminLogger _adminLogger;
        private readonly IUserManager _userManager;
        private readonly IAppConfig _appConfig;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IAppUserRepo _appUserRepo;

        public UserVerficationManager(IAdminLogger adminLogger, IUserManager userMananger, IAppConfig appConfig, ISmsSender smsSender, IAppUserRepo appUserRepo,
                                    IEmailSender emailSender, IDependencyManager depManager, ISecurity security) : base(adminLogger, appConfig, depManager, security)
        {
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _userManager = userMananger ?? throw new ArgumentNullException(nameof(userMananger));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
        }

        public async Task<InvokeResult> CheckConfirmedAsync(EntityHeader orgHeader, EntityHeader userHeader)
        {
            /* This will only take the current user id so we don't have to do any security checks, not really confidential info anyways */
            var user = await _userManager.FindByIdAsync(userHeader.Id);
            if (user == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerifyController_SendConfirmationEmailAsync", "Could not get current user.");
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            if (user.EmailConfirmed)
            {
                return InvokeResult.Success;
            }
            else
            {
                return InvokeResult.FromErrors(new ErrorMessage() { Message = "Email Not Confirmed" });
            }
        }

        //In some cases, this will be called from API, we don't want to return API as part of the link.
        private String GetWebURI()
        {
            var environment = _appConfig.WebAddress;
            if (_appConfig.WebAddress.ToLower().Contains("api"))
            {
                switch (_appConfig.Environment)
                {
                    case Environments.Development: environment = "https://dev.nuviot.com";  break;
                    case Environments.Testing: environment = "https://test.nuviot.com"; break;
                    case Environments.Beta: environment = "https://qa.nuviot.com"; break;
                    case Environments.Staging: environment = "https://stage.nuviot.com"; break;
                    case Environments.Production: environment = "https://www.nuviot.com"; break;
                }
            }

            return environment;
        }

        public async Task<InvokeResult> SendConfirmationEmailAsync(EntityHeader orgHeader, EntityHeader userHeader)
        {
            var appUser = await _userManager.FindByIdAsync(userHeader.Id);
            if (appUser == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerifyController_SendConfirmationEmailAsync", "Could not get current user.");
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                var encodedToken = System.Net.WebUtility.UrlEncode(token);

                

                var callbackUrl = $"{GetWebURI()}/Account/Verify?userId={appUser.Id}&code={encodedToken}";
                var mobileCallbackUrl = $"nuviot:confirmemail/?userId={appUser.Id}&code={encodedToken}";

#if DEBUG
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "UserVerifyController_SendConfirmationEmailAsync", "SentToken",
                     token.ToKVP("token"),
                     appUser.Id.ToKVP("appUserId"),
                     encodedToken.ToKVP("encodedToken"),
                     appUser.Email.ToKVP("toEmailAddress"));
#endif

                var subject = UserAdminResources.Email_Verification_Subject.Replace("[APP_NAME]", _appConfig.AppName);
                var body = UserAdminResources.Email_Verification_Body.Replace("[CALLBACK_URL]", callbackUrl).Replace("[MOBILE_CALLBACK_URL]", mobileCallbackUrl);
                var result = await _emailSender.SendAsync(appUser.Email, subject, body);

                _adminLogger.LogInvokeResult("UserVerficationManager_SendConfirmationEmailAsync", result,
                    new KeyValuePair<string, string>("token", token),
                    new KeyValuePair<string, string>("toUserId", appUser.Id),
                    new KeyValuePair<string, string>("toEmail", appUser.Email));
                return result;

            }
            catch (Exception ex)
            {
                _adminLogger.AddException("UserVerficationManager_SendConfirmationEmailAsync", ex,
                   new KeyValuePair<string, string>("toUserId", appUser.Id),
                   new KeyValuePair<string, string>("toEmail", appUser.Email));

                return InvokeResult.FromErrors(UserAdminErrorCodes.RegErrorSendingEmail.ToErrorMessage(), new ErrorMessage() { Message = ex.Message });
            }
        }

        public async Task<InvokeResult> SendSMSCodeAsync(VerfiyPhoneNumber sendSMSCode, EntityHeader orgHeader, EntityHeader userHeader)
        {
            if (String.IsNullOrEmpty(sendSMSCode.PhoneNumber))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerficationManager_SendSMSCodeAsync", UserAdminErrorCodes.RegMissingEmail.Message);
                return InvokeResult.FromErrors(UserAdminErrorCodes.RegMissingPhoneNumber.ToErrorMessage());
            }

            var user = await _userManager.FindByIdAsync(userHeader.Id);
            if (user == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerficationManager_SendSMSCodeAsync", "Could not get current user.");
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            try
            {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, sendSMSCode.PhoneNumber);
                var result = await _smsSender.SendAsync(sendSMSCode.PhoneNumber, UserAdminResources.SMS_Verification_Body.Replace("[CODE]", code).Replace("[APP_NAME]", _appConfig.AppName));

                _adminLogger.LogInvokeResult("UserVerficationManager_SendSMSCodeAsync", result,
                    new KeyValuePair<string, string>("phone", sendSMSCode.PhoneNumber),
                    new KeyValuePair<string, string>("code", code));

                return result;
            }
            catch (Exception ex)
            {
                _adminLogger.AddException("UserVerficationManager_SendSMSCodeAsync", ex);
                return InvokeResult.FromErrors(UserAdminErrorCodes.RegErrorSendingSMS.ToErrorMessage(), new ErrorMessage() { Message = ex.Message });
            }
        }

        public async Task<InvokeResult> ValidateSMSAsync(VerfiyPhoneNumber verifyRequest, EntityHeader orgHeader, EntityHeader userHeader)
        {
            var user = await _userManager.FindByIdAsync(userHeader.Id);
            if (user == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerficationManager_ValidateSMSAsync", "Could not get current user.");
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            var result = await _userManager.ChangePhoneNumberAsync(user, verifyRequest.PhoneNumber, verifyRequest.SMSCode);
            if (result.Successful)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "UserVerficationManager_ValidateSMSAsync", "Success_ConfirmSMS",
                    new KeyValuePair<string, string>("phone", verifyRequest.PhoneNumber),
                    new KeyValuePair<string, string>("code", verifyRequest.SMSCode));

                return InvokeResult.Success;
            }
            else
            {
                _adminLogger.LogInvokeResult("UserVerficationManager_ValidateEmailAsync", result,
                    new KeyValuePair<string, string>("phone", verifyRequest.PhoneNumber),
                    new KeyValuePair<string, string>("sentToken", verifyRequest.SMSCode));
                return result;
            }
        }

        public async Task<InvokeResult> ValidateEmailAsync(ConfirmEmail confirmemaildto, EntityHeader orgHeader, EntityHeader userHeader)
        {

            var appUser = await _userManager.FindByIdAsync(userHeader.Id);
            if (appUser == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerficationManager_ValidateEmailAsync", "Could not get current user.");
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

#if DEBUG
            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "UserVerficationManager_ValidateEmailAsync", "ReceivedToken",
                 confirmemaildto.ReceivedCode.ToKVP("token"),
                 appUser.Id.ToKVP("appUserId"),
                 appUser.Email.ToKVP("toEmailAddress"));
#endif 


            var result = await _userManager.ConfirmEmailAsync(appUser, confirmemaildto.ReceivedCode);
            if (result.Successful)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "UserVerficationManager_ValidateEmailAsync", "Success_ConfirmEmail",
                    new KeyValuePair<string, string>("userId", appUser.Id),
                    new KeyValuePair<string, string>("code", confirmemaildto.ReceivedCode));

                return InvokeResult.Success;
            }
            else
            {
                _adminLogger.LogInvokeResult("UserVerficationManager_ValidateEmailAsync", result,
                    new KeyValuePair<string, string>("userId", appUser.Id),
                    new KeyValuePair<string, string>("sentToken", confirmemaildto.ReceivedCode));
                return result;
            }
        }

        public async Task<InvokeResult> SetUserSMSValidated(string userId, EntityHeader orgHeader, EntityHeader userHeader)
        {
            var appUser = await _appUserRepo.FindByIdAsync(userHeader.Id);
            if (!appUser.IsSystemAdmin) return InvokeResult.FromError("Must be a system admin to set a users phone number as verified.");

            var user = await _appUserRepo.FindByIdAsync(userId);
            user.PhoneNumber = "5555551212";
            user.PhoneNumberConfirmed = true;
            await _appUserRepo.UpdateAsync(user);

            return InvokeResult.Success;
        }
    }
}
