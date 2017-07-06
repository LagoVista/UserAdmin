using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
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
            services.AddTransient<IUserManager, UserManager>();
            services.AddTransient<ISignInManager, SignInManager>();
            services.AddTransient<IEmailSender, SendGridEmailService>();
            services.AddTransient<ISmsSender, TwilioSMSSender>();
            services.AddTransient<IAuthTokenManager, AuthTokenManager>();
            services.AddSingleton<IClaimsFactory, ClaimsFactory>();
            services.AddSingleton<IAuthTokenManager, AuthTokenManager>();
            services.AddSingleton<IRefreshTokenManager, RefreshTokenManager>();
        }
    }
}
