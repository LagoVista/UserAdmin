// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d410eeeb2dfed85f79ddabc76572888380a2e8892c2645518913f01e8b2cce6e
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.AspNetCore.Identity.Utils;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Managers.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.REpos.Account;
using LagoVista.UserAdmin.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

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

            services.AddSingleton<IAppUserMfaManager, AppUserMfaManager>();
            services.AddSingleton<IAppUserPasskeyManager, AppUserPasskeyManager>();
            services.AddTransient<IMagicLinkManager, MagicLinkManager>();

            services.AddSingleton<IPendingIdentityManager, PendingIdentityManager>();

            IdentityModelEventSource.ShowPII = true;

        }
    }
}
