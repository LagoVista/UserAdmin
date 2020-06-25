using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Managers;


namespace LagoVista.UserAdmin
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAppUserManager, AppUserManager>();
            services.AddTransient<IAppUserManagerReadOnly, AppUserManagerReadOnly>();
            services.AddTransient<IAppUserManager, AppUserManager>();
            services.AddTransient<IOrganizationManager, OrgManager>();
            services.AddTransient<ITeamManager, TeamManager>();
            services.AddTransient<IAssetSetManager, AssetSetManager>();
            services.AddTransient<ISubscriptionManager, SubscriptionManager>();
            services.AddTransient<IAppInstanceManager, AppInstanceManager>();
            services.AddTransient<IUserVerficationManager, UserVerficationManager>();
            services.AddTransient<IPasswordManager, PasswordManager>();
            services.AddTransient<IDistributionManager, DistributionManager>();
        }
    }
}