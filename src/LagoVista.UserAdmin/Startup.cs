using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;

namespace LagoVista.UserAdmin
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAppUserManager, AppUserManager>();
            services.AddSingleton<IAppUserManagerReadOnly, AppUserManagerReadOnly>();
            services.AddSingleton<IAppUserManager, AppUserManager>();
            services.AddSingleton<IOrganizationManager, OrgManager>();
            services.AddTransient<ITeamManager, TeamManager>();
            services.AddTransient<IAssetSetManager, AssetSetManager>();
            services.AddTransient<IScheduledDowntimeManager, ScheduledDowntimeManager>();
            services.AddTransient<IHolidaySetManager, HolidaySetManager>();
            services.AddTransient<ISubscriptionManager, SubscriptionManager>();
            services.AddSingleton<IModuleManager, ModuleManager>();
            services.AddTransient<IAppInstanceManager, AppInstanceManager>();
            services.AddTransient<IUserVerficationManager, UserVerficationManager>();
            services.AddTransient<IDefaultRoleList, DefaultRoleList>();
            services.AddTransient<IPasswordManager, PasswordManager>();
            services.AddSingleton<IIUserAccessManager, UserAccessManager>();
            services.AddTransient<IOrgUtils, OrgUtils>();
            services.AddTransient<IRoleManager, RoleManager>(); ;
            services.AddTransient<IDistributionManager, DistributionManager>();
            services.AddTransient<ISingleUseTokenManager, SingleUseTokenManager>();
            services.AddTransient<ICalendarManager, CalendarManager>();
            services.AddSingleton<IMostRecentlyUsedManager, MostRecentlyUsedManager>();
            services.AddSingleton<IUserFavoritesManager, UserFavoritesManager>();            
            services.AddTransient<ISystemNotificationManager, SystemNotificationManager>();
            services.AddSingleton<IAppUserInboxManager, AppUserInboxManager>();
            services.AddTransient<ICallLogManager, RingCentralManager>();
            services.AddSingleton<IAuthenticationLogManager, AuthenticationLogManager>();
            services.AddTransient<ILocationDiagramManager, LocationDiagramManager>();
            services.AddScoped<ISecureLinkManager, SecureLinkManager>();
        }
    }
}