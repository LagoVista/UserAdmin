// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1bbddc28712320979b2f0c638f45c696c6bbd9deff3ad8eaf79e7ebe9060ec54
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.ML;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        private readonly IEmailVerificationCodeRepo _emailVerificationCodeRepo;

         public UserVerficationManager(IAdminLogger adminLogger, IUserManager userMananger, IAppConfig appConfig, ISmsSender smsSender, IAppUserRepo appUserRepo, IAuthenticationLogManager authLogManager,
                                       IOrganizationManager orgManager, ISignInManager signInManager, IEmailSender emailSender, IEmailVerificationCodeRepo emailVerificationCodeRepo, IDependencyManager depManager, ISecurity security) : base(adminLogger, appConfig, depManager, security)
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
            _emailVerificationCodeRepo = emailVerificationCodeRepo ?? throw new ArgumentNullException(nameof(emailVerificationCodeRepo));
        }

        public async Task<InvokeResult> CheckConfirmedAsync(EntityHeader userHeader)
        {
            /* This will only take the current user id so we don't have to do any security checks, not really confidential info anyways */
            var user = await _userManager.FindByIdAsync(userHeader.Id);
            if (user == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), "Could not get current user.");
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

        private static string ComputeEmailVerificationCodeHash(AppUser appUser, string code)
        {
            var key = !String.IsNullOrWhiteSpace(appUser.SecurityStamp) ? appUser.SecurityStamp : appUser.PasswordHash;
            if (String.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("The user does not have security state available for email verification.");

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(code)));
            }
        }

        private static bool EmailVerificationCodeHashesMatch(string expectedHash, string actualHash)
        {
            if (String.IsNullOrWhiteSpace(expectedHash) || String.IsNullOrWhiteSpace(actualHash)) return false;
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expectedHash), Encoding.UTF8.GetBytes(actualHash));
        }

        public async Task<InvokeResult<string>> SendConfirmationEmailAsync(string userId, string confirmSubject = "", string confirmBody = "", string appName = "", string logoFile = "")
        {
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendEmailConfirmFailed, userId: userId, extras: $"Could not find user with id: {userId}");
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, this.Tag(), "Could not get current user.");
                return InvokeResult<string>.FromErrors(UserAdminErrorCodes.AuthCouldNotFindUserAccount.ToErrorMessage());
            }
                  
            await _userManager.UpdateAsync(appUser);

            return await SendConfirmationEmailAsync(appUser, confirmSubject, confirmBody, appName, logoFile);
        }

        public async Task<InvokeResult<string>> SendConfirmationEmailAsync(AppUser appUser, string confirmSubject = "", string confirmBody = "", string appName = "", string logoFile= "")
        {
            var userHeader = appUser.ToEntityHeader();

            try
            {
                var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
                var now = DateTime.UtcNow;
                var verificationCode = new EmailVerificationCode
                {
                    Id = Guid.NewGuid().ToId(),
                    UserId = appUser.Id,
                    CodeHash = ComputeEmailVerificationCodeHash(appUser, code),
                    CreatedUtc = now,
                    ExpiresUtc = now.AddMinutes(10),
                    AttemptCount = 0
                };

                await _emailVerificationCodeRepo.StoreAsync(verificationCode);
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendingEmailConfirm, userId: userHeader.Id, userName: userHeader.Text);

                var subject = String.IsNullOrEmpty(confirmSubject) ? UserAdminResources.Email_Verification_Subject.Replace("[APP_NAME]", _appConfig.AppName) : confirmSubject;
                var bodyTemplate = String.IsNullOrEmpty(confirmBody) ? UserAdminResources.Email_Verification_Body : confirmBody;
                var body = bodyTemplate.Replace("[VERIFICATION_CODE]", code).Replace("[CODE]", code);

                var result = await _emailSender.SendAsync(appUser.Email, subject, body, _appConfig.SystemOwnerOrg, appUser.ToEntityHeader(), appName, logoFile);

                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendEmailConfirmSuccess, userId: userHeader.Id, userName: userHeader.Text);
                if (result.Successful)
                {
                    appUser.VerifyEmailSentTimeStamp = DateTime.UtcNow.ToJSONString();
                    _adminLogger.Trace($"{this.Tag()} Success Sending Verification Email",
                        new KeyValuePair<string, string>("toUserId", appUser.Id),
                        new KeyValuePair<string, string>("toEmail", appUser.Email));

                    await _signInManager.RefreshUserLoginAsync(appUser);
                    return InvokeResult<string>.Create(_appConfig.Environment == Environments.Development ||
                        _appConfig.Environment == Environments.Local ||
                        _appConfig.Environment == Environments.LocalDevelopment ? code : String.Empty);
                }
                else
                    return InvokeResult<string>.FromInvokeResult(result);

            }
            catch (Exception ex)
            {
                _adminLogger.AddException(this.Tag(), ex,
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

            InvokeResult result;

            if (appUser.EmailConfirmed)
            {
                result = InvokeResult.Success;
            }
            else
            {
                const string invalidCodeMessage = "The email verification code is invalid or expired.";

                if (confirmemaildto == null || String.IsNullOrWhiteSpace(confirmemaildto.ReceivedCode) || confirmemaildto.ReceivedCode.Length != 6)
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.EmailConfirmFailed, appUser, extras: invalidCodeMessage);
                    return InvokeResult.FromError(invalidCodeMessage);
                }

                var verificationCode = await _emailVerificationCodeRepo.GetLatestAsync(appUser.Id);
                if (verificationCode == null || verificationCode.ConsumedUtc.HasValue || verificationCode.ExpiresUtc <= DateTime.UtcNow || verificationCode.AttemptCount >= 5)
                {
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.EmailConfirmFailed, appUser, extras: invalidCodeMessage);
                    return InvokeResult.FromError(invalidCodeMessage);
                }

                var submittedHash = ComputeEmailVerificationCodeHash(appUser, confirmemaildto.ReceivedCode);
                if (!EmailVerificationCodeHashesMatch(verificationCode.CodeHash, submittedHash))
                {
                    verificationCode.AttemptCount++;
                    if (verificationCode.AttemptCount >= 5) verificationCode.ConsumedUtc = DateTime.UtcNow;
                    await _emailVerificationCodeRepo.UpdateAsync(verificationCode);
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.EmailConfirmFailed, appUser, extras: invalidCodeMessage);
                    return InvokeResult.FromError(invalidCodeMessage);
                }

                appUser.EmailConfirmed = true;
                result = await _userManager.UpdateAsync(appUser);
                if (result.Successful)
                {
                    verificationCode.ConsumedUtc = DateTime.UtcNow;
                    await _emailVerificationCodeRepo.UpdateAsync(verificationCode);
                    await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "ConfirmedEmail", appUser.CurrentOrganization?.ToEntityHeader(), appUser.ToEntityHeader());
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.ConfirmEmailSuccess, appUser);
                }
            }

            if (result.Successful)
            {
                await _signInManager.SignInAsync(appUser);

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "UserVerficationManager_ValidateEmailAsync", "Success_ConfirmEmail",
                    new KeyValuePair<string, string>("userId", appUser.Id));

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
                    new KeyValuePair<string, string>("userId", appUser.Id));
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
