using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LagoVista.UserAdmin
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAppUserManager, AppUserManager>();
            services.AddScoped<IAppUserManagerReadOnly, AppUserManagerReadOnly>();
            services.AddScoped<IAppUserManager, AppUserManager>();
            services.AddScoped<IOrganizationManager, OrgManager>();
            services.AddScoped<ITeamManager, TeamManager>();
            services.AddScoped<IAssetSetManager, AssetSetManager>();
            services.AddScoped<IScheduledDowntimeManager, ScheduledDowntimeManager>();
            services.AddScoped<IHolidaySetManager, HolidaySetManager>();
            services.AddScoped<ISubscriptionManager, SubscriptionManager>();
            services.AddScoped<IModuleManager, ModuleManager>();
            services.AddScoped<IAppInstanceManager, AppInstanceManager>();
            services.AddScoped<IUserVerficationManager, UserVerficationManager>();
            services.AddScoped<IDefaultRoleList, DefaultRoleList>();
            services.AddScoped<IPasswordManager, PasswordManager>();
            services.AddScoped<IIUserAccessManager, UserAccessManager>();
            services.AddScoped<IOrgUtils, OrgUtils>();
            services.AddScoped<IRoleManager, RoleManager>(); ;
            services.AddScoped<IDistributionManager, DistributionManager>();
            services.AddScoped<ISingleUseTokenManager, SingleUseTokenManager>();
            services.AddScoped<ICalendarManager, CalendarManager>();
            services.AddScoped<IMostRecentlyUsedManager, MostRecentlyUsedManager>();
            services.AddScoped<IUserFavoritesManager, UserFavoritesManager>();            
            services.AddScoped<ISystemNotificationManager, SystemNotificationManager>();
            services.AddScoped<IAppUserInboxManager, AppUserInboxManager>();
            services.AddScoped<ICallLogManager, RingCentralManager>();
            services.AddScoped<IAuthenticationLogManager, AuthenticationLogManager>();
            services.AddScoped<ILocationDiagramManager, LocationDiagramManager>();
            services.AddScoped<ISecureLinkManager, SecureLinkManager>();
            services.AddScoped<IUserRegistrationManager, UserRegistrationManager>();
            services.AddScoped<IFunctionMapManager, FunctionMapManager>();

            Services.Startup.ConfigureServices(services, configuration);

            services.AddScoped<IOrgInformationSource, OrgInformationSource>();
            services.AddScoped<IOrgIdentityData, OrgIdentityData>();

            services.AddScoped<IAppUserTestingManager, AppUserTestingManager>();
        }
    }
}