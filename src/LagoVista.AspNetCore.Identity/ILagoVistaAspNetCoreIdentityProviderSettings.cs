using System;
using System.Collections.Generic;
using System.Text;
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
