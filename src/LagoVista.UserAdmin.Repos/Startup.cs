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
            services.AddTransient<IAppUserRepo, AppUserRepo>();
            services.AddTransient<ILocationUserRepo, LocationUserRepo>();
            services.AddTransient<IRefreshTokenRepo, RefreshTokenRepo>();
            services.AddTransient<IInviteUserRepo, InviteUserRepo>();
            services.AddTransient<IOrgUserRepo, OrgUserRepo>();
            services.AddTransient<IOwnedObjectRepo, OwnedObjectRepo>();
            services.AddTransient<IOrgLocationRepo, OrgLocationRepo>();
            services.AddTransient<IOrganizationRepo, OrganizationRepo>();
            services.AddTransient<ISubscriptionResourceRepo, SubscriptionResourceRepo>();
            services.AddTransient<ILocationRoleRepo, LocationRoleRepo>();
            services.AddTransient<IRoleRepo, RoleRepo>();
            services.AddTransient<IUserRoleManager, UserRoleManager>();
            services.AddTransient<IModuleRepo, ModuleRepo>();
            services.AddTransient<IAccessLogRepo, AccessLogRepo>();
            services.AddTransient<IHolidaySetRepo, HolidaySetRepo>();
            services.AddTransient<IScheduledDowntimeRepo, ScheduledDowntimeRepo>();
            services.AddTransient<ITeamRepo, TeamRepo>();
            services.AddTransient<ITeamUserRepo, TeamUserRepo>();
            services.AddTransient<IAssetSetRepo, AssetSetRepo>();
            services.AddTransient<IManagedAssetRepo, ManagedAssetRepo>();
            services.AddTransient<IAppInstanceRepo, AppInstanceRepo>();
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

            ErrorCodes.Register(typeof(UserAdminErrorCodes));
        }
    }
}
