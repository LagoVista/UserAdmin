using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using Microsoft.Extensions.Configuration;

namespace LagoVista.AspNetCore.Identity
{
    public class LagoVistaAspNetCoreIdentityProviderSettings : ILagoVistaAspNetCoreIdentityProviderSettings
    {
        public string SmtpFrom { get; }

        public string SmtpFromName { get;  }
        public string FromPhoneNumber { get; }

        public IConnectionSettings SmsServer { get; }

        public IConnectionSettings SmtpServer { get; }

        public LagoVistaAspNetCoreIdentityProviderSettings(IConfiguration configuration)
        {
            var smtpSection = configuration.GetSection("Smtp");
            SmtpFrom = smtpSection.Require("FromAddress");
            SmtpFromName = smtpSection.Require("FromName");
            SmtpServer = new ConnectionSettings
            {
                Uri = smtpSection.Require("Server"),
                UserName = smtpSection.Require("UserName"),
                Password = smtpSection.Require("Password")
            };


            var smsSection = configuration.GetSection("Sms");
            FromPhoneNumber = smsSection.Require("OutgoingNumber");
            SmsServer = new ConnectionSettings
            {
                AccountId = smsSection.Require("AccountId"),
                AccessKey = smsSection.Require("AuthToken"),
            };
        }

    }
}
