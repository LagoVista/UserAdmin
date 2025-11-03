// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fd506bed119eb385ea2dc4e6369dbe611839a013be8b1db9c80b3a2a1ffc9e9a
// IndexVersion: 0
// --- END CODE INDEX META ---
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
using LagoVista.Core.Models.ML;

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
        private readonly ISignInManager _signInManager;
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly IOrganizationManager _orgManager;

         public UserVerficationManager(IAdminLogger adminLogger, IUserManager userMananger, IAppConfig appConfig, ISmsSender smsSender, IAppUserRepo appUserRepo, IAuthenticationLogManager authLogManager,
                                       IOrganizationManager orgManager, ISignInManager signInManager, IEmailSender emailSender, IDependencyManager depManager, ISecurity security) : base(adminLogger, appConfig, depManager, security)
        {
            _authLogMgr = authLogManager ?? throw new ArgumentNullException(nameof(authLogManager));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _userManager = userMananger ?? throw new ArgumentNullException(nameof(userMananger));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _orgManager = orgManager ?? throw new ArgumentNullException(nameof(orgManager));
        }

        public async Task<InvokeResult> CheckConfirmedAsync(EntityHeader userHeader)
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
                    case Environments.Development: environment = "https://dev.nuviot.com"; break;
                    case Environments.Testing: environment = "https://test.nuviot.com"; break;
                    case Environments.Beta: environment = "https://qa.nuviot.com"; break;
                    case Environments.Staging: environment = "https://stage.nuviot.com"; break;
                    case Environments.Production: environment = "https://www.nuviot.com"; break;
                }
            }

            return environment;
        }

        public async Task<InvokeResult<string>> SendConfirmationEmailAsync(string userId, string confirmSubject = "", string confirmBody = "", string appName = "", string logoFile= "")
        {
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendEmailConfirmFailed, userId: userId, extras: $"Could not find user with id: {userId}");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "[UserVerifyManager__SendConfirmationEmailAsync]", "Could not get current user.");
                return InvokeResult<string>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            var userHeader = appUser.ToEntityHeader();

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendingEmailConfirm, userId: userHeader.Id, userName: userHeader.Text, extras: $"Raw Token={token}");
                Console.WriteLine($"[UserVerficationManager_SendConfirmationEmailAsync] Raw Token: [{token}]");

                var encodedToken = System.Net.WebUtility.UrlEncode(token);

                Console.WriteLine($"[UserVerficationManager_SendConfirmationEmailAsync] Encoded Token: [{encodedToken}]");

                Console.WriteLine($"[UserVerficationManager_SendConfirmationEmailAsync] Decoded Token: [{System.Net.WebUtility.UrlDecode(encodedToken)}]");

                var callbackUrl = $"{GetWebURI()}/api/verify/email?userid={appUser.Id}&code={encodedToken}";
                var mobileCallbackUrl = $"nuviot:confirmemail/?userId={appUser.Id}&code={encodedToken}";

#if DEBUG
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[UserVerifyManager_SendConfirmationEmailAsync]", "SentToken",
                     token.ToKVP("token"),
                     appUser.Id.ToKVP("appUserId"),
                     encodedToken.ToKVP("encodedToken"),
                     appUser.Email.ToKVP("toEmailAddress"));
#endif

                var subject = String.IsNullOrEmpty(confirmSubject) ? UserAdminResources.Email_Verification_Subject.Replace("[APP_NAME]", _appConfig.AppName) : confirmSubject;
                var body = String.IsNullOrEmpty(confirmBody) ? UserAdminResources.Email_Verification_Body.Replace("[CALLBACK_URL]", callbackUrl).Replace("[MOBILE_CALLBACK_URL]", mobileCallbackUrl) :
                                        confirmBody.Replace("[CALLBACK_URL]", callbackUrl).Replace("[MOBILE_CALLBACK_URL]", mobileCallbackUrl);

                var result = await _emailSender.SendAsync(appUser.Email, subject, body, _appConfig.SystemOwnerOrg, appUser.ToEntityHeader(), appName, logoFile);

                _adminLogger.LogInvokeResult("[UserVerficationManager_SendConfirmationEmailAsync]", result,
                    new KeyValuePair<string, string>("callbackLink", callbackUrl),
                    new KeyValuePair<string, string>("token", token),
                    new KeyValuePair<string, string>("toUserId", appUser.Id),
                    new KeyValuePair<string, string>("toEmail", appUser.Email));

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendEmailConfirmSuccess, userId: userHeader.Id, userName: userHeader.Text);
                if (result.Successful)
                    return InvokeResult<string>.Create(_appConfig.Environment == Environments.Development ||
                        _appConfig.Environment == Environments.Local ||
                        _appConfig.Environment == Environments.LocalDevelopment ? encodedToken : String.Empty);
                else
                    return InvokeResult<string>.FromInvokeResult(result);


            }
            catch (Exception ex)
            {
                _adminLogger.AddException("UserVerficationManager_SendConfirmationEmailAsync", ex,
                   new KeyValuePair<string, string>("toUserId", appUser.Id),
                   new KeyValuePair<string, string>("toEmail", appUser.Email));

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendEmailConfirmFailed, userId: userHeader.Id, userName: userHeader.Text, extras: ex.Message);

                return InvokeResult<string>.FromErrors(UserAdminErrorCodes.RegErrorSendingEmail.ToErrorMessage(), new ErrorMessage() { Message = ex.Message });
            }
        }

        public async Task<InvokeResult<string>> SendSMSCodeAsync(VerfiyPhoneNumber sendSMSCode, EntityHeader userHeader)
        {
            if (String.IsNullOrEmpty(sendSMSCode.PhoneNumber))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerficationManager_SendSMSCodeAsync", UserAdminErrorCodes.RegMissingEmail.Message);

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendSMSConfirmFailed, userId: userHeader.Id, userName: userHeader.Text, extras: $"Empty Phone Number");
                return InvokeResult<string>.FromErrors(UserAdminErrorCodes.RegMissingPhoneNumber.ToErrorMessage());
            }

            var user = await _userManager.FindByIdAsync(userHeader.Id);
            if (user == null)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendSMSConfirmFailed, userId: userHeader.Id, userName: userHeader.Text, extras: $"Could not find user with id: {userHeader.Id}");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerficationManager_SendSMSCodeAsync", "Could not get current user.");
                return InvokeResult<string>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            try
            {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, sendSMSCode.PhoneNumber);
                var result = await _smsSender.SendAsync(sendSMSCode.PhoneNumber, UserAdminResources.SMS_Verification_Body.Replace("[CODE]", code).Replace("[APP_NAME]", _appConfig.AppName));
                user.PhoneNumber = sendSMSCode.PhoneNumber;
                user.PhoneNumberConfirmed = true;
                user.PhoneNumberConfirmedForBilling = false;

                await _userManager.UpdateAsync(user);

                _adminLogger.LogInvokeResult("UserVerficationManager_SendSMSCodeAsync", result,
                    new KeyValuePair<string, string>("phone", sendSMSCode.PhoneNumber),
                    new KeyValuePair<string, string>("code", code));

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendSMSConfirmSuccess, userId: userHeader.Id, userName: userHeader.Text);

                if (result.Successful)
                    return InvokeResult<string>.Create(_appConfig.Environment == Environments.Development ||
                        _appConfig.Environment == Environments.Local ||
                        _appConfig.Environment == Environments.LocalDevelopment ? code : String.Empty);
                else
                    return InvokeResult<string>.FromInvokeResult(result);
            }
            catch (Exception ex)
            {
                _adminLogger.AddException("UserVerficationManager_SendSMSCodeAsync", ex);

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendSMSConfirmFailed, userId: userHeader.Id, userName: userHeader.Text, extras: ex.Message);

                return InvokeResult<string>.FromErrors(UserAdminErrorCodes.RegErrorSendingSMS.ToErrorMessage(), new ErrorMessage() { Message = ex.Message });
            }
        }

        public async Task<InvokeResult> ValidateSMSAsync(VerfiyPhoneNumber verifyRequest, EntityHeader userHeader)
        {
            var user = await _userManager.FindByIdAsync(userHeader.Id);
            if (user == null)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SMSConfirmFailed, userId: userHeader.Id, userName: userHeader.Text, extras: $"Could not find user with id: {userHeader.Id}");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "UserVerficationManager_ValidateSMSAsync", "Could not get current user.");
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }

            if (verifyRequest.SkipStep)
            {
                verifyRequest.SMSCode = await _userManager.GenerateChangePhoneNumberTokenAsync(user, verifyRequest.PhoneNumber);
            }

            if(!String.IsNullOrEmpty(verifyRequest.PhoneNumber) && user.PhoneNumber != verifyRequest.PhoneNumber )
            {
                var errorMessage = $"Phone numbers do not match: Sent to SMS Code: {user.PhoneNumber} - {verifyRequest.PhoneNumber}";
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SMSConfirmFailed, userId: userHeader.Id, userName: userHeader.Text, extras: errorMessage);
                return InvokeResult.FromError(errorMessage);
            }

            var result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, verifyRequest.SMSCode);
            if (result.Successful)
            {
                await _authLogMgr.AddAsync(verifyRequest.SkipStep ? Models.Security.AuthLogTypes.SMSConfirmedBypass : Models.Security.AuthLogTypes.SMSConfirmSuccess, userId: userHeader.Id, userName: userHeader.Text);

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "UserVerficationManager_ValidateSMSAsync", "Success_ConfirmSMS",
                    new KeyValuePair<string, string>("phone", verifyRequest.PhoneNumber),
                    new KeyValuePair<string, string>("code", verifyRequest.SMSCode));

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SMSConfirmSuccess, userId: userHeader.Id, userName: userHeader.Text);

                user.PhoneNumberConfirmedForBilling = !verifyRequest.SkipStep;
                await _userManager.UpdateAsync(user);

                return InvokeResult.Success;
            }
            else
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SMSConfirmFailed, userId: userHeader.Id, userName: userHeader.Text, extras: result.ErrorMessage);

                _adminLogger.LogInvokeResult("UserVerficationManager_ValidateEmailAsync", result,
                    new KeyValuePair<string, string>("phone", verifyRequest.PhoneNumber),
                    new KeyValuePair<string, string>("sentToken", verifyRequest.SMSCode));
                return result;
            }
        }

        public async Task<InvokeResult> ValidateEmailAsync(ConfirmEmail confirmemaildto, EntityHeader userHeader)
        {

            var appUser = await _userManager.FindByIdAsync(userHeader.Id);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.EmailConfirmFailed, userId: userHeader.Id, userName: userHeader.Text, extras: $"Could not find user with id: {userHeader.Id}");
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
                await _signInManager.SignInAsync(appUser);

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "UserVerficationManager_ValidateEmailAsync", "Success_ConfirmEmail",
                    new KeyValuePair<string, string>("userId", appUser.Id),
                    new KeyValuePair<string, string>("code", confirmemaildto.ReceivedCode));

                if(null != appUser.CurrentOrganization)
                {
                    var org = await _orgManager.GetPublicOrginfoAsync(appUser.CurrentOrganization.Namespace);
                    if(!String.IsNullOrEmpty(org.EndUserHomePage) && appUser.LoginType == Models.Users.LoginTypes.AppEndUser)
                        return InvokeResult.SuccessRedirect(org.EndUserHomePage);

                    if(!String.IsNullOrEmpty(org.HomePage))
                        return InvokeResult.SuccessRedirect(org.HomePage);

                    if(appUser.ShowWelcome)
                        return InvokeResult.SuccessRedirect(CommonLinks.HomeWelcome);

                    return InvokeResult.SuccessRedirect(CommonLinks.Home);
                }

                return InvokeResult.SuccessRedirect(CommonLinks.CreateDefaultOrg);
            }
            else
            {
                _adminLogger.LogInvokeResult("UserVerficationManager_ValidateEmailAsync", result,
                    new KeyValuePair<string, string>("userId", appUser.Id),
                    new KeyValuePair<string, string>("sentToken", confirmemaildto.ReceivedCode));
                return result;
            }
        }

        public async Task<InvokeResult> SetUserSMSValidated(string userId, EntityHeader userHeader)
        {
            var appUser = await _appUserRepo.FindByIdAsync(userHeader.Id);
            if (!appUser.IsSystemAdmin) return InvokeResult.FromError("Must be a system admin to set a users phone number as verified.");

            var user = await _appUserRepo.FindByIdAsync(userId);
            user.PhoneNumber = "5555551212";
            user.PhoneNumberConfirmed = true;
            user.PhoneNumberConfirmedForBilling = false;
            await _appUserRepo.UpdateAsync(user);

            await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SMSConfirmedBypass, userId: userHeader.Id, userName: userHeader.Text);

            return InvokeResult.Success;
        }
    }
}
