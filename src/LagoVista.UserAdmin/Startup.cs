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
            services.AddTransient<IAppUserManager, AppUserManager>();
            services.AddTransient<IAppUserManagerReadOnly, AppUserManagerReadOnly>();
            services.AddTransient<IAppUserManager, AppUserManager>();
            services.AddTransient<IOrganizationManager, OrgManager>();
            services.AddTransient<ITeamManager, TeamManager>();
            services.AddTransient<IAssetSetManager, AssetSetManager>();
            services.AddTransient<IScheduledDowntimeManager, ScheduledDowntimeManager>();
            services.AddTransient<IHolidaySetManager, HolidaySetManager>();
            services.AddTransient<ISubscriptionManager, SubscriptionManager>();
            services.AddTransient<IModuleManager, ModuleManager>();
            services.AddTransient<IAppInstanceManager, AppInstanceManager>();
            services.AddTransient<IUserVerficationManager, UserVerficationManager>();
            services.AddTransient<IDefaultRoleList, DefaultRoleList>();
            services.AddTransient<IPasswordManager, PasswordManager>();
            services.AddTransient<IIUserAccessManager, UserAccessManager>();
            services.AddTransient<IOrgUtils, OrgUtils>();
            services.AddTransient<IRoleManager, RoleManager>(); ;
            services.AddTransient<IDistributionManager, DistributionManager>();
            services.AddTransient<ISingleUseTokenManager, SingleUseTokenManager>();
            services.AddTransient<ICalendarManager, CalendarManager>();
            services.AddTransient<IMostRecentlyUsedManager, MostRecentlyUsedManager>();
            services.AddTransient<IUserFavoritesManager, UserFavoritesManager>();            
            services.AddTransient<ISystemNotificationManager, SystemNotificationManager>();
            services.AddTransient<IAppUserInboxManager, AppUserInboxManager>();
            services.AddTransient<ICallLogManager, RingCentralManager>();
        }
    }
}