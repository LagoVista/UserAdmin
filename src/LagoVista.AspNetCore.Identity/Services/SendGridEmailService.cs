﻿using LagoVista.Core.Interfaces;
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

        public SendGridEmailService(ILagoVistaAspNetCoreIdentityProviderSettings settings, IAppConfig appConfig)
        {
            _settings = settings;
            _appConfig = appConfig;
        }

        public async Task SendAsync(string email, string subject, string body)
        {

            body = $@"<body>
<img src=""{_appConfig.AppLogo}"" />
<h1>{_appConfig.AppName}</h1>
<h2>{subject}</h2>
{body}
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
           
        }
    }
}