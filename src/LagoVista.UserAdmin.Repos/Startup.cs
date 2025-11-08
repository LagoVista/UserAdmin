// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7e2be60736308a5c0d6e0da8bfc2c227f42f0cb03ae9188024eb564c58c56791
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using LagoVista.UserAdmin.Interfaces.Repos.Commo;
using LagoVista.UserAdmin.Repos.Repos.Commo;

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
            services.AddSingleton<IRoleRepo, RoleRepo>();
            services.AddSingleton<IUserRoleManager, UserRoleManager>();
            services.AddSingleton<IModuleRepo, ModuleRepo>();
            services.AddSingleton<IAccessLogRepo, AccessLogRepo>();
            services.AddSingleton<IHolidaySetRepo, HolidaySetRepo>();
            services.AddSingleton<IScheduledDowntimeRepo, ScheduledDowntimeRepo>();
            services.AddSingleton<IScheduledDowntimeRepo, ScheduledDowntimeRepo>();
            services.AddSingleton<ITeamRepo, TeamRepo>();
            services.AddSingleton<ITeamUserRepo, TeamUserRepo>();
            services.AddSingleton<IAssetSetRepo, AssetSetRepo>();
            services.AddSingleton<IManagedAssetRepo, ManagedAssetRepo>();
            services.AddSingleton<IAppInstanceRepo, AppInstanceRepo>();
            services.AddSingleton<IUserRoleRepo, UserRoleRepo>();
            services.AddSingleton<IRoleAccessRepo, RoleAccessRepo>();
            services.AddTransient<ISingleUseTokenRepo, SingleUseTokenRepo>();
            services.AddSingleton<IDistributionListRepo, DistributionListRepo>();
            services.AddSingleton<ICalendarRepo, CalendarRepo>();
            services.AddSingleton<IMostRecentlyUsedRepo, MostRecentUsedRepo>();
            services.AddSingleton<IUserFavoritesRepo, UserFavoritesRepo>();
            services.AddSingleton<IAppUserInboxRepo, AppUserIndboxRepo>();
            services.AddSingleton<IAuthenticationLogRepo, AuthenticationLogRepo>();
            services.AddSingleton<ISystemNotificationsRepo, SystemNotificationsRepo>();
            services.AddSingleton<ISentEmailRepo, SentEmailRepo>();
            services.AddTransient<ILocationDiagramRepo, LocationDiagramRepo>();
            services.AddScoped<ISecureLinkRepo, SecureLinkRepo>();
            services.AddSingleton<IFunctionMapRepo, FunctionMapRepo>();

            ErrorCodes.Register(typeof(UserAdminErrorCodes));
        }
    }
}
