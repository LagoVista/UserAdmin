using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    public static class BrowserInstallationServiceCollectionExtensions
    {
        // Host opt-in only. The resolver must verify the actual canonical
        // app and origin from trusted host configuration and storage.
        public static IServiceCollection AddBrowserInstallationBinding(this IServiceCollection services,
            Func<IServiceProvider, HttpContext, Task<(string AppId, Uri Origin)>> resolveApplication)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (resolveApplication == null) throw new ArgumentNullException(nameof(resolveApplication));
            services.AddScoped<BrowserInstallationCookie>();
            services.AddScoped<BrowserInstallationSession>();
            services.AddScoped(sp => new BrowserInstallationSignIn(
                sp.GetRequiredService<BrowserInstallationSession>(), context => resolveApplication(sp, context), sp.GetRequiredService<BrowserInstallationCookie>()));

            // Compose rather than replace any existing application-cookie event.
            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
            {
                var previous = options.Events.OnSigningIn;
                options.Events.OnSigningIn = async context =>
                {
                    await previous(context);
                    var sp = context.HttpContext.RequestServices;
                    var coordinator = sp.GetRequiredService<BrowserInstallationSignIn>();
                    var users = sp.GetRequiredService<UserManager<AppUser>>();
                    await coordinator.ApplyAsync(context, users.FindByIdAsync);
                };
            });
            return services;
        }
    }
}
