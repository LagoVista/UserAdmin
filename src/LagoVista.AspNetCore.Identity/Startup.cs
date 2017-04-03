using LagoVista.AspNetCore.Identity.Services;
using LagoVista.UserAdmin.Interfaces.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.AspNetCore.Identity
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEmailSender, SendGridEmailService>();
            services.AddTransient<ISmsSender, TwilioSMSSender>();
        }
    }
}
