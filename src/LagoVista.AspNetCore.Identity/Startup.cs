using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.AspNetCore.Identity.Utils;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using Microsoft.Extensions.DependencyInjection;

namespace LagoVista.AspNetCore.Identity
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IUserManager, UserManager>();
            services.AddTransient<ISignInManager, SignInManager>();

            //TODO: These don't belong here.
            services.AddTransient<IEmailSender, SendGridEmailService>();
            services.AddTransient<ISmsSender, TwilioSMSSender>();

            services.AddSingleton<IClaimsFactory, ClaimsFactory>();

            services.AddSingleton<ITokenHelper, TokenHelper>();
            services.AddSingleton<IOrgHelper, OrgHelper>();
            services.AddSingleton<IAuthRequestValidators, AuthRequestValidators>();

            services.AddSingleton<IAuthTokenManager, AuthTokenManager>();
            services.AddSingleton<IRefreshTokenManager, RefreshTokenManager>();
        }
    }
}
