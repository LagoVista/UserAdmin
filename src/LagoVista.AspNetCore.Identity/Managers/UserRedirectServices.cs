using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class UserRedirectServices : IUserRedirectServices
    {
        private readonly IAppConfig _appConfig;
        private readonly IAdminLogger _adminLogger;
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<AppUser> _signinManager;
        private readonly UserManager<AppUser> _aspNetUserManager;

        public UserRedirectServices(UserManager<AppUser> userManager, IAppConfig appConfig, IEmailSender emailSender,
                                    SignInManager<AppUser> signinManager, IAdminLogger adminLogger,IAuthenticationLogManager authLogMgr)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _aspNetUserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _signinManager = signinManager ?? throw new ArgumentNullException(nameof(signinManager));   
        }

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


        public async Task<InvokeResult<string>> SendConfirmationEmailAsync(AppUser appUser, string confirmSubject = "", string confirmBody = "", string appName = "", string logoFile = "")
        {
            var userHeader = appUser.ToEntityHeader();

            try
            {
                var token = await _aspNetUserManager.GenerateEmailConfirmationTokenAsync(appUser);
                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendingEmailConfirm, userId: userHeader.Id, userName: userHeader.Text, extras: $"Raw Token={token}");

                var encodedToken = System.Net.WebUtility.UrlEncode(token);

                var callbackUrl = $"{GetWebURI()}/auth/email/confirm?p={appUser.Id}&c={encodedToken}";
                var mobileCallbackUrl = $"nuviot:confirmemail/?userId={appUser.Id}&code={encodedToken}";

                var subject = String.IsNullOrEmpty(confirmSubject) ? UserAdminResources.Email_Verification_Subject.Replace("[APP_NAME]", _appConfig.AppName) : confirmSubject;
                var body = String.IsNullOrEmpty(confirmBody) ? UserAdminResources.Email_Verification_Body.Replace("[CALLBACK_URL]", callbackUrl).Replace("[MOBILE_CALLBACK_URL]", mobileCallbackUrl) :
                                        confirmBody.Replace("[CALLBACK_URL]", callbackUrl).Replace("[MOBILE_CALLBACK_URL]", mobileCallbackUrl);

                var result = await _emailSender.SendAsync(appUser.Email, subject, body, _appConfig.SystemOwnerOrg, appUser.ToEntityHeader(), appName, logoFile);


                await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.SendEmailConfirmSuccess, userId: userHeader.Id, userName: userHeader.Text);
                if (result.Successful)
                {
                    appUser.VerifyEmailSentTimeStamp = DateTime.UtcNow.ToJSONString();
                    _adminLogger.Trace($"{this.Tag()} Success Sending Verification Email",
                        new KeyValuePair<string, string>("callbackLink", callbackUrl),
                        new KeyValuePair<string, string>("token", token),
                        new KeyValuePair<string, string>("toUserId", appUser.Id),
                        new KeyValuePair<string, string>("toEmail", appUser.Email));

                    await _signinManager.RefreshSignInAsync(appUser);
                    return InvokeResult<string>.Create(_appConfig.Environment == Environments.Development ||
                        _appConfig.Environment == Environments.Local ||
                        _appConfig.Environment == Environments.LocalDevelopment ? encodedToken : String.Empty);
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

        public async Task<InvokeResult<string>> IdentityDefaultRedirectAsync(AppUser user, string defaultRedirect = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (String.IsNullOrEmpty(user.Email) || String.IsNullOrEmpty(user.FirstName) || String.IsNullOrEmpty(user.LastName))
                return InvokeResult<string>.Create(CommonLinks.CompleteUserRegistration);
            if (!user.EmailConfirmed)
            {
                var sendConfirmResult = await SendConfirmationEmailAsync(user);
                if (sendConfirmResult.Successful)
                    return InvokeResult<string>.Create(CommonLinks.ConfirmEmailSent);
                else
                    return sendConfirmResult.ToInvokeResult<string>();
            }
            if (user.CurrentOrganization == null) return InvokeResult<string>.Create(CommonLinks.CreateDefaultOrg);
            if (user.PendingInviteIds.Length == 1) return InvokeResult<string>.Create(CommonLinks.AcceptInviteId.Replace("{inviteid}", user.PendingInviteIds.First()));
            else if (user.PendingInviteIds.Any()) return InvokeResult<string>.Create(CommonLinks.Invitations);

            if (!String.IsNullOrEmpty(user.PendingRedirect)) return InvokeResult<string>.Create(user.PendingRedirect);
            if (!String.IsNullOrEmpty(user.CurrentOrganization?.HomePage)) return InvokeResult<string>.Create(user.CurrentOrganization?.HomePage);

            if (defaultRedirect != null) return InvokeResult<string>.Create(defaultRedirect);
            if (user.ShowWelcome) return InvokeResult<string>.Create(CommonLinks.HomeWelcome);
            return InvokeResult<string>.Create(CommonLinks.Home);
        }
    }
}
