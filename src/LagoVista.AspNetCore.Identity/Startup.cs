// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d410eeeb2dfed85f79ddabc76572888380a2e8892c2645518913f01e8b2cce6e
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Models;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.AspNetCore.Identity.Utils;
using LagoVista.Core.Interfaces;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.REpos.Account;
using LagoVista.UserAdmin.Managers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Security.Models;

namespace LagoVista.AspNetCore.Identity
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<ISignInManager, SignInManager>();

            services.AddScoped<ICoreEmailServices, SendGridEmailService>();
            services.AddScoped<IEmailSender, SendGridEmailService>();
            services.AddScoped<ISmsSender, TwilioSMSSender>();

            services.AddScoped<IClaimsFactory, ClaimsFactory>();

            services.AddScoped<ITokenHelper, TokenHelper>();
            services.AddScoped<IOrgHelper, OrgHelper>();
            services.AddScoped<IAuthRequestValidators, AuthRequestValidators>();

            services.AddScoped<ITwitterAuthService, TwitterAuthServices>();

            services.AddScoped<IAuthTokenManager, AuthTokenManager>();
            services.AddScoped<IRefreshTokenManager, RefreshTokenManager>();
            services.AddScoped<IPendingIdentityManager, PendingIdentityManager>();

            services.AddScoped<IExternalLoginManager, ExternalLoginManager>();
            services.AddScoped<IAppUserMfaManager, AppUserMfaManager>();
            services.AddScoped<IAppUserPasskeyManager, AppUserPasskeyManager>();
            services.AddTransient<IMagicLinkManager, MagicLinkManager>();
           
            services.AddTransient<IPasswordHasher<PendingIdentity>, PasswordHasher<PendingIdentity>>();

            services.AddScoped<IUserRedirectServices, UserRedirectServices>();

            services.AddSingleton<IDataProtectionSettings, DataProtectionSettings>();
            services.AddSingleton<ITokenAuthOptions, TokenAuthOptions>();
            services.AddScoped<ILagoVistaAspNetCoreIdentityProviderSettings, LagoVistaAspNetCoreIdentityProviderSettings>();

            IdentityModelEventSource.ShowPII = true;

        }
    }
}


namespace LagoVista.DependencyInjection
{
    public static class LagoVistaIdentityModule
    {
        public static void AddLagoVistaIdentityModule(this IServiceCollection services, IConfigurationRoot configurationRoot, ILogger logger)
        {
            LagoVista.AspNetCore.Identity.Startup.ConfigureServices(services, configurationRoot);
        }
    }
}