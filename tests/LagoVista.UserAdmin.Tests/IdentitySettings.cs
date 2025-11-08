// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 588b01f0fa1d1005d4277dda320749e4da8e385401175db64c901918276adaa4
// IndexVersion: 2
// --- END CODE INDEX META ---
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
