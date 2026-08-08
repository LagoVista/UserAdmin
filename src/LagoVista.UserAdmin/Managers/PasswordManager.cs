// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 44fe8b563193fe121e24a94f08919222860237c681124e41bbc58edbc517b411
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using System.Security.Cryptography;
using System.Text;

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
        private readonly IPasswordResetCodeRepo _passwordResetCodeRepo;

        public const string ACTION_RESET_PASSWORD = "/auth/resetpassword";


        public PasswordManager(IAuthRequestValidators authRequestValidators, IUserManager userManager, IEmailSender emailSender, IPasswordResetCodeRepo passwordResetCodeRepo, IDependencyManager depManager, ISecurity security, IAuthenticationLogManager authLogMgr, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _adminLogger = logger;
            _emailSender = emailSender;
            _appConfig = appConfig;
            _userManager = userManager;
            _authRequestValidators = authRequestValidators;
            _authLogMgr = authLogMgr;
            _passwordResetCodeRepo = passwordResetCodeRepo;
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

        private static string BuildPasswordRecoveryUserName(string email, string endUserAppOrgId)
        {
            return String.IsNullOrWhiteSpace(endUserAppOrgId) ? email : $"{email}@{endUserAppOrgId.Trim()}";
        }

        private static string ComputeResetCodeHash(AppUser appUser, string code)
        {
            var key = !String.IsNullOrWhiteSpace(appUser.SecurityStamp) ? appUser.SecurityStamp : appUser.PasswordHash;
            if (String.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("The user does not have security state available for password recovery.");

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(code)));
            }
        }

        private static bool ResetCodeHashesMatch(string expectedHash, string actualHash)
        {
            if (String.IsNullOrWhiteSpace(expectedHash) || String.IsNullOrWhiteSpace(actualHash)) return false;
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expectedHash), Encoding.UTF8.GetBytes(actualHash));
        }

        public async Task<InvokeResult> SendResetPasswordLinkAsync(SendResetPasswordLink sendResetPasswordLink)
        {
            var validationResult = _authRequestValidators.ValidateSendPasswordLinkRequest(sendResetPasswordLink);
            if (!validationResult.Successful) return validationResult;

            await _authLogMgr.AddAsync(AuthLogTypes.PasswordRecoveryRequested, userName: sendResetPasswordLink.Email);

            var userName = BuildPasswordRecoveryUserName(sendResetPasswordLink.Email, sendResetPasswordLink.EndUserAppOrgId);
            var appUser = await _userManager.FindByNameAsync(userName);

            if (appUser == null)
            {
                _adminLogger.AddError("PasswordManager_SendResetPasswordLinkAsync", "CouldNotFindUser", new System.Collections.Generic.KeyValuePair<string, string>("email", sendResetPasswordLink.Email));
                return InvokeResult.Success;
            }

            var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            var now = DateTime.UtcNow;
            var resetCode = new PasswordResetCode
            {
                Id = Guid.NewGuid().ToId(),
                UserId = appUser.Id,
                CodeHash = ComputeResetCodeHash(appUser, code),
                CreatedUtc = now,
                ExpiresUtc = now.AddMinutes(10),
                AttemptCount = 0
            };

            await _passwordResetCodeRepo.StoreAsync(resetCode);
            await _authLogMgr.AddAsync(AuthLogTypes.PasswordRecoveryCodeGenerated, appUser);

            var subject = UserAdminResources.Email_ResetPassword_Subject.Replace("[APP_NAME]", _appConfig.AppName);
            var body = UserAdminResources.Email_ResetPassword_Body.Replace("[RESET_CODE]", code);

            var result = await _emailSender.SendAsync(sendResetPasswordLink.Email, subject, body, _appConfig.SystemOwnerOrg, appUser.ToEntityHeader());
            if (result.Successful)
            {
                await _authLogMgr.AddAsync(AuthLogTypes.PasswordRecoveryMessageSent, appUser);

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "PasswordManager_SendResetPasswordLinkAsync", "SentCode",
                     appUser.Id.ToKVP("appUserId"),
                     appUser.Email.ToKVP("toEmailAddress"));


                await LogEntityActionAsync(appUser.Id, typeof(AppUser).Name, "SentResetPasswordCode", _appConfig.SystemOwnerOrg, appUser.ToEntityHeader());
            }
            else
            {
                _adminLogger.AddError("PasswordManager_SendResetPasswordLinkAsync", "Could Not Send Password Link", result.ErrorsToKVPArray());
            }

            return result;
        }


        public async Task<InvokeResult<string>> VerifyPasswordResetCodeAsync(VerifyPasswordResetCode request)
        {
            const string invalidCodeMessage = "The password recovery code is invalid or expired.";

            if (request == null || String.IsNullOrWhiteSpace(request.Email) || String.IsNullOrWhiteSpace(request.Code) || request.Code.Length != 6)
                return InvokeResult<string>.FromErrors(new ErrorMessage(invalidCodeMessage));

            var userName = BuildPasswordRecoveryUserName(request.Email, request.EndUserAppOrgId);
            var appUser = await _userManager.FindByNameAsync(userName);
            if (appUser == null)
                return InvokeResult<string>.FromErrors(new ErrorMessage(invalidCodeMessage));

            var resetCode = await _passwordResetCodeRepo.GetLatestAsync(appUser.Id);
            if (resetCode == null || resetCode.ConsumedUtc.HasValue || resetCode.ExpiresUtc <= DateTime.UtcNow || resetCode.AttemptCount >= 5)
            {
                await _authLogMgr.AddAsync(AuthLogTypes.PasswordRecoveryCodeVerificationFailed, appUser);
                return InvokeResult<string>.FromErrors(new ErrorMessage(invalidCodeMessage));
            }

            var submittedHash = ComputeResetCodeHash(appUser, request.Code);
            if (!ResetCodeHashesMatch(resetCode.CodeHash, submittedHash))
            {
                resetCode.AttemptCount++;
                if (resetCode.AttemptCount >= 5) resetCode.ConsumedUtc = DateTime.UtcNow;
                await _passwordResetCodeRepo.UpdateAsync(resetCode);
                await _authLogMgr.AddAsync(AuthLogTypes.PasswordRecoveryCodeVerificationFailed, appUser);
                return InvokeResult<string>.FromErrors(new ErrorMessage(invalidCodeMessage));
            }

            resetCode.ConsumedUtc = DateTime.UtcNow;
            await _passwordResetCodeRepo.UpdateAsync(resetCode);

            var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            await _authLogMgr.AddAsync(AuthLogTypes.PasswordRecoveryCodeVerified, appUser);
            return InvokeResult<string>.Create(token);
        }

        public async Task<InvokeResult> SetUserPasswordAsync(ChangePassword changeRequest, EntityHeader org, EntityHeader user)
        {
            AssertRole(CoreSecurityRoles.OrgAdmin);

            var appUser = await _userManager.FindByIdAsync(changeRequest.UserId);
            if (appUser == null)
            {
                _adminLogger.AddError(this.Tag(), "CouldNotFindUser", new System.Collections.Generic.KeyValuePair<string, string>("id", changeRequest.UserId));
                return InvokeResult.FromErrors(new ErrorMessage(UserAdminResources.Err_PwdChange_CouldNotFindUser));
            }

            var tokenCode = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            var resetStatus = await _userManager.ResetPasswordAsync(appUser, tokenCode, changeRequest.NewPassword);
            if (resetStatus.Successful)
                await _authLogMgr.AddAsync(AuthLogTypes.PasswordSetByAdminSucceeded, appUser.Id, appUser.UserName, org.Id, org.Text, extras: $"Set By Admin: {user.Id}");
            else
                await _authLogMgr.AddAsync(AuthLogTypes.PasswordSetByAdminFailed, appUser.Id, appUser.UserName,  org.Id, errors: resetStatus.ErrorMessage,extras: $"Set By Admin: {user.Id}");

            return resetStatus;

        }


        public async Task<InvokeResult> ChangePasswordAsync(ChangePassword changePassword, EntityHeader orgEntityHeader, EntityHeader userEntityHeader)
        {
            var validationResult = _authRequestValidators.ValidatePasswordChangeRequest(changePassword, userEntityHeader.Id);
            if (!validationResult.Successful) return validationResult;

            var appUser = await _userManager.FindByIdAsync(userEntityHeader.Id);
            if (appUser == null)
            {
                _adminLogger.AddError(this.Tag(), "CouldNotFindUser", new System.Collections.Generic.KeyValuePair<string, string>("id", userEntityHeader.Id));
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

            var userName = BuildPasswordRecoveryUserName(resetPassword.Email, resetPassword.EndUserAppOrgId);
            var appUser = await _userManager.FindByNameAsync(userName);
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
                await _authLogMgr.AddAsync(AuthLogTypes.PasswordRecoveryCompleted, appUser);

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
