using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    public class SendGridEmailService : IEmailSender
    {
        ILagoVistaAspNetCoreIdentityProviderSettings _settings;
        IAppConfig _appConfig;
        IAdminLogger _adminLogger;

        public SendGridEmailService(ILagoVistaAspNetCoreIdentityProviderSettings settings, IAppConfig appConfig, IAdminLogger adminLogger)
        {
            _settings = settings;
            _appConfig = appConfig;
            _adminLogger = adminLogger;
        }

        public async Task<InvokeResult> SendAsync(string email, string subject, string body)
        {

            try
            {
                body = $@"<body>
<img src=""{_appConfig.AppLogo}"" />
<h1>{_appConfig.AppName}</h1>
<h2>{subject}</h2>
{body}
<img src=""{_appConfig.CompanyLogo}"" />
<p>Please do not reply to this email as it is an unmonitored address</p>
<a mailto:""support@software-logistics.com"">Contact Support</a>
</body>";

                var msg = new MimeMessage()
                {
                    Subject = subject,
                    Body = new TextPart("html", body),
                };

                msg.To.Add(new MailboxAddress(email));
                msg.From.Add(new MailboxAddress(_settings.SmtpFrom));

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_settings.SmtpServer.Uri.ToString(), 587, false);
                    await client.AuthenticateAsync(_settings.SmtpServer.UserName, _settings.SmtpServer.Password);
                    await client.SendAsync(msg);
                    await client.DisconnectAsync(true);
                }

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "SendGridEmailServices_SendAsync", "EmailSent",
                    new System.Collections.Generic.KeyValuePair<string, string>("Subject", subject),
                    new System.Collections.Generic.KeyValuePair<string, string>("to", email));

                return InvokeResult.Success;
            }
            catch (Exception ex)
            {
                _adminLogger.AddException("SendGridEmailServices_SendAsync", ex,
                    new System.Collections.Generic.KeyValuePair<string, string>("Subject", subject),
                    new System.Collections.Generic.KeyValuePair<string, string>("to", email));

                return InvokeResult.FromException("SendGridEmailServices_SendAsync", ex);
            }

        }
    }
}
