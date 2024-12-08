using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.AspNetCore.Identity.Utils;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace LagoVista.AspNetCore.Identity
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUserManager, UserManager>();
            services.AddSingleton<ISignInManager, SignInManager>();

            //TODO: These don't belong here.
            services.AddSingleton<IEmailSender, SendGridEmailService>();
            services.AddSingleton<ISmsSender, TwilioSMSSender>();

            services.AddSingleton<IClaimsFactory, ClaimsFactory>();

            services.AddSingleton<ITokenHelper, TokenHelper>();
            services.AddSingleton<IOrgHelper, OrgHelper>();
            services.AddSingleton<IAuthRequestValidators, AuthRequestValidators>();

            services.AddScoped<ITwitterAuthService, TwitterAuthServices>();

            services.AddSingleton<IAuthTokenManager, AuthTokenManager>();
            services.AddSingleton<IRefreshTokenManager, RefreshTokenManager>();
            services.AddScoped<IExternalLoginManager, ExternalLoginManager>();
        }
    }
}
