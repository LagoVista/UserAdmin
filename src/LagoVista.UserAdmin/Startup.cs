using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Repos;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddSingleton<ITeamManager, TeamManager>();
            services.AddSingleton<IAssetSetManager, AssetSetManager>();
            services.AddSingleton<IScheduledDowntimeManager, ScheduledDowntimeManager>();
            services.AddSingleton<IHolidaySetManager, HolidaySetManager>();
            services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
            services.AddSingleton<IModuleManager, ModuleManager>();
            services.AddSingleton<IAppInstanceManager, AppInstanceManager>();
            services.AddSingleton<IUserVerficationManager, UserVerficationManager>();
            services.AddSingleton<IDefaultRoleList, DefaultRoleList>();
            services.AddSingleton<IPasswordManager, PasswordManager>();
            services.AddSingleton<IIUserAccessManager, UserAccessManager>();
            services.AddSingleton<IOrgUtils, OrgUtils>();
            services.AddSingleton<IRoleManager, RoleManager>(); ;
            services.AddSingleton<IDistributionManager, DistributionManager>();
            services.AddSingleton<ISingleUseTokenManager, SingleUseTokenManager>();
            services.AddSingleton<ICalendarManager, CalendarManager>();
            services.AddSingleton<IMostRecentlyUsedManager, MostRecentlyUsedManager>();
            services.AddScoped<IAppUserTestingManager, AppUserTestingManager>();
            services.AddSingleton<IUserFavoritesManager, UserFavoritesManager>();            
            services.AddSingleton<ISystemNotificationManager, SystemNotificationManager>();
            services.AddSingleton<IAppUserInboxManager, AppUserInboxManager>();
            services.AddSingleton<ICallLogManager, RingCentralManager>();
            services.AddSingleton<IAuthenticationLogManager, AuthenticationLogManager>();
            services.AddSingleton<ILocationDiagramManager, LocationDiagramManager>();
            services.AddScoped<ISecureLinkManager, SecureLinkManager>();
            services.AddScoped<IUserRegistrationManager, UserRegistrationManager>();
            services.AddScoped<IFunctionMapManager, FunctionMapManager>();

            services.AddScoped<IOrgInformationSource, OrgInformationSource>();
            services.AddScoped<IOrgIdentityData, OrgIdentityData>();
        }
    }
}