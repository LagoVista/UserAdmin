using LagoVista.AspNetCore.Identity;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Tests
{
    public class IdentitySettings : ILagoVistaAspNetCoreIdentityProviderSettings
    {
        public string SmtpFrom => throw new NotImplementedException();

        public string FromPhoneNumber => throw new NotImplementedException();


        IConnectionSettings _smsSettings = new ConnectionSettings()
        {
            Password = Environment.GetEnvironmentVariable("TWILLIO_API_KEY")
        };

        public IConnectionSettings SmsServer
        {
            get => _smsSettings;
        }

        IConnectionSettings _smtpSettings = new ConnectionSettings()
        {
            Password = Environment.GetEnvironmentVariable("SEND_GRID_API_KEY")
        };

        public IConnectionSettings SmtpServer
        {
            get => _smtpSettings;
        }
    }
}
