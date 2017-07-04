using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace LagoVista.UserAdmin
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAppUserManager, AppUserManager>();
            services.AddTransient<IOrganizationManager, OrgManager>();
            services.AddTransient<ITeamManager, TeamManager>();
            services.AddTransient<IAssetSetManager, AssetSetManager>();
            services.AddTransient<ISubscriptionManager, SubscriptionManager>();
        }
    }
}