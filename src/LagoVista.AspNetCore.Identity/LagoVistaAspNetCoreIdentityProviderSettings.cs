using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using Microsoft.Extensions.Configuration;

namespace LagoVista.AspNetCore.Identity
{
    public class LagoVistaAspNetCoreIdentityProviderSettings : ILagoVistaAspNetCoreIdentityProviderSettings
    {
        public string SmtpFrom { get; }

        public string FromPhoneNumber { get; }

        public IConnectionSettings SmsServer { get; }

        public IConnectionSettings SmtpServer { get; }

        public LagoVistaAspNetCoreIdentityProviderSettings(IConfiguration configuration)
        {
            var smtpSection = configuration.GetRequiredSection("Smtp");
            SmtpFrom = smtpSection.Require("FromAddress");
            SmtpServer = new ConnectionSettings
            {
                Uri = smtpSection.Require("Server"),
                UserName = smtpSection.Require("UserName"),
                Password = smtpSection.Require("Password")
            };


            var smsSection = configuration.GetRequiredSection("Sms");
            FromPhoneNumber = configuration.Require("OutgoingNumber");
            SmsServer = new ConnectionSettings
            {
                AccountId = smsSection.Require("AccountId"),
                AccessKey = smsSection.Require("AuthToken"),
                Password = smsSection.Require("Password")
            };
        }

    }
}
