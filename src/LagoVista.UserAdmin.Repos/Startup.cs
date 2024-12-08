using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Repos.Users;
using LagoVista.UserAdmin.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Repos.Users;
using LagoVista.UserAdmin.Repos.Security;
using LagoVista.IoT.Logging;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Repos.Repos.Apps;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Repos.Repos.Security;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Repos.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Repos.Repos.Calendar;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Repos.Repos.Account;

namespace LagoVista.UserAdmin.Repos
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDeviceOwnerRepo, DeviceOwnerRepo>();
            services.AddSingleton<IAppUserRepo, AppUserRepo>();
            services.AddSingleton<ILocationUserRepo, LocationUserRepo>();
            services.AddSingleton<IRefreshTokenRepo, RefreshTokenRepo>();
            services.AddSingleton<IInviteUserRepo, InviteUserRepo>();
            services.AddSingleton<IOrgUserRepo, OrgUserRepo>();
            services.AddSingleton<IOwnedObjectRepo, OwnedObjectRepo>();
            services.AddSingleton<IOrgLocationRepo, OrgLocationRepo>();
            services.AddSingleton<IOrganizationRepo, OrganizationRepo>();
            services.AddSingleton<ISubscriptionResourceRepo, SubscriptionResourceRepo>();
            services.AddSingleton<ILocationRoleRepo, LocationRoleRepo>();
            services.AddTransient<IRoleRepo, RoleRepo>();
            services.AddSingleton<IUserRoleManager, UserRoleManager>();
            services.AddSingleton<IModuleRepo, ModuleRepo>();
            services.AddSingleton<IAccessLogRepo, AccessLogRepo>();
            services.AddSingleton<IHolidaySetRepo, HolidaySetRepo>();
            services.AddSingleton<IScheduledDowntimeRepo, ScheduledDowntimeRepo>();
            services.AddSingleton<ITeamRepo, TeamRepo>();
            services.AddTransient<ITeamUserRepo, TeamUserRepo>();
            services.AddSingleton<IAssetSetRepo, AssetSetRepo>();
            services.AddSingleton<IManagedAssetRepo, ManagedAssetRepo>();
            services.AddSingleton<IAppInstanceRepo, AppInstanceRepo>();
            services.AddTransient<IUserRoleRepo, UserRoleRepo>();
            services.AddTransient<IRoleAccessRepo, RoleAccessRepo>();
            services.AddTransient<ISingleUseTokenRepo, SingleUseTokenRepo>();
            services.AddTransient<IDistributionListRepo, DistributionListRepo>();
            services.AddTransient<ICalendarRepo, CalendarRepo>();
            services.AddTransient<IMostRecentlyUsedRepo, MostRecentUsedRepo>();
            services.AddTransient<IUserFavoritesRepo, UserFavoritesRepo>();
            services.AddTransient<IAppUserInboxRepo, AppUserIndboxRepo>();
            services.AddTransient<IAuthenticationLogRepo, AuthenticationLogRepo>();
            services.AddTransient<ISystemNotificationsRepo, SystemNotificationsRepo>();
            services.AddTransient<ILocationDiagramRepo, LocationDiagramRepo>();
            services.AddScoped<ISecureLinkRepo, SecureLinkRepo>();

            ErrorCodes.Register(typeof(UserAdminErrorCodes));
        }
    }
}
