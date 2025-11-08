// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2b5860fd2de0a1b7f7c51049f4385ad68f80b2bb4419da8599a1f3f9adf1fdbe
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using LagoVista.Core.Interfaces;

namespace LagoVista.AspNetCore.Identity
{
    public interface ILagoVistaAspNetCoreIdentityProviderSettings
    {
        String SmtpFrom { get; }
        string FromPhoneNumber { get; }

        IConnectionSettings SmsServer { get; }

        IConnectionSettings SmtpServer { get; }
    }
}
