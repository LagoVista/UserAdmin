using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Interfaces.Repos.Commo;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Testing;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Interfaces.REpos.Account;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Testing;
using LagoVista.UserAdmin.Repos.Account;
using LagoVista.UserAdmin.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Passkeys;
using LagoVista.UserAdmin.Repos.Repos.Account;
using LagoVista.UserAdmin.Repos.Repos.Apps;
using LagoVista.UserAdmin.Repos.Repos.Calendar;
using LagoVista.UserAdmin.Repos.Repos.Commo;
using LagoVista.UserAdmin.Repos.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Repos.Security;
using LagoVista.UserAdmin.Repos.Repos.Testing;
using LagoVista.UserAdmin.Repos.Repos.Users;
using LagoVista.UserAdmin.Repos.Security;
using LagoVista.UserAdmin.Repos.TableStorage.Passkeys;
using LagoVista.UserAdmin.Repos.Testing;
using LagoVista.UserAdmin.Repos.Users;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Repos
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDeviceOwnerRepo, DeviceOwnerRepo>();
            services.AddSingleton<IAppUserRepo, AppUserRepo>();
            services.AddSingleton<IAppUserLoaderRepo, AppUserLoadRepo>();
            services.AddSingleton<ILocationUserRepo, LocationUserRepo>();
            services.AddSingleton<IRefreshTokenRepo, RefreshTokenRepo>();
            services.AddSingleton<IInviteUserRepo, InviteUserRepo>();
            services.AddSingleton<IOrgUserRepo, OrgUserRepo>();
            services.AddSingleton<IOwnedObjectRepo, OwnedObjectRepo>();
            services.AddSingleton<IOrgLocationRepo, OrgLocationRepo>();
            services.AddSingleton<IOrganizationRepo, OrganizationRepo>();
            services.AddSingleton<IOrganizationLoaderRepo, OrganizationLoaderRepo>();
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
            services.AddSingleton<IAppUserTestingDslRepo, AppUserTestingDslRepo>();
            services.AddSingleton<IAppUserTestRunRepo, AppUserTestRunRepo>();
            services.AddSingleton<IAuthViewRepo, AuthViewRepo>();
            services.AddSingleton<IAppUserPasskeyCredentialRepo, PasskeyCredentialRepo>();
            services.AddSingleton<IPasskeyCredentialIndexRepo, PasskeyCredentialIndexRepo>();
            services.AddTransient<ITestArtifactStorage, TestArtifactStorage>();
            services.AddTransient<IPasskeyChallengeStore, RedisPasskeyChallengeStore>();
            services.AddTransient<IMagicLinkAttemptStore, MagicLinkAttemptCacheStore>();

            ErrorCodes.Register(typeof(UserAdminErrorCodes));
        }
    }
}
