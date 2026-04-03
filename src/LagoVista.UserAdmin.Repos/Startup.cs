using LagoVista.Core.Interfaces;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Repos;
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
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Testing;
using LagoVista.UserAdmin.Repos.Account;
using LagoVista.UserAdmin.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Passkeys;
using LagoVista.UserAdmin.Repos.Relational;
using LagoVista.UserAdmin.Repos.Repos.Account;
using LagoVista.UserAdmin.Repos.Repos.Apps;
using LagoVista.UserAdmin.Repos.Repos.Calendar;
using LagoVista.UserAdmin.Repos.Repos.Commo;
using LagoVista.UserAdmin.Repos.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Repos.Relational;
using LagoVista.UserAdmin.Repos.Repos.Security;
using LagoVista.UserAdmin.Repos.Repos.Testing;
using LagoVista.UserAdmin.Repos.Repos.Users;
using LagoVista.UserAdmin.Repos.Security;
using LagoVista.UserAdmin.Repos.TableStorage.Passkeys;
using LagoVista.UserAdmin.Repos.Testing;
using LagoVista.UserAdmin.Repos.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LagoVista.UserAdmin.Repos
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAppUserLoaderRepo, AppUserLoadRepo>();
            services.AddScoped<ILocationUserRepo, LocationUserRepo>();
            services.AddScoped<IRefreshTokenRepo, RefreshTokenRepo>();
            services.AddScoped<IInviteUserRepo, InviteUserRepo>();
            services.AddScoped<IOrgUserRepo, OrgUserRepo>();
            services.AddScoped<IOwnedObjectRepo, OwnedObjectRepo>();
            services.AddScoped<IOrgLocationRepo, OrgLocationRepo>();
            services.AddScoped<IOrganizationRepo, OrganizationRepo>();
            services.AddScoped<IOrganizationLoaderRepo, OrganizationLoaderRepo>();
            services.AddScoped<ISubscriptionResourceRepo, SubscriptionResourceRepo>();
            services.AddScoped<ILocationRoleRepo, LocationRoleRepo>();
            services.AddScoped<IRoleRepo, RoleRepo>();
            services.AddScoped<IUserRoleManager, UserRoleManager>();
            services.AddScoped<IModuleRepo, ModuleRepo>();
            services.AddScoped<IAccessLogRepo, AccessLogRepo>();
            services.AddScoped<IHolidaySetRepo, HolidaySetRepo>();
            services.AddScoped<IScheduledDowntimeRepo, ScheduledDowntimeRepo>();
            services.AddScoped<IScheduledDowntimeRepo, ScheduledDowntimeRepo>();
            services.AddScoped<ITeamRepo, TeamRepo>();
            services.AddScoped<ITeamUserRepo, TeamUserRepo>();
            services.AddScoped<IAssetSetRepo, AssetSetRepo>();
            services.AddScoped<IManagedAssetRepo, ManagedAssetRepo>();
            services.AddScoped<IAppInstanceRepo, AppInstanceRepo>();
            services.AddScoped<IUserRoleRepo, UserRoleRepo>();
            services.AddScoped<IRoleAccessRepo, RoleAccessRepo>();
            services.AddTransient<ISingleUseTokenRepo, SingleUseTokenRepo>();
            services.AddScoped<IDistributionListRepo, DistributionListRepo>();
            services.AddScoped<ICalendarRepo, CalendarRepo>();
            services.AddScoped<IMostRecentlyUsedRepo, MostRecentUsedRepo>();
            services.AddScoped<IUserFavoritesRepo, UserFavoritesRepo>();
            services.AddScoped<IAppUserInboxRepo, AppUserIndboxRepo>();
            services.AddScoped<IAuthenticationLogRepo, AuthenticationLogRepo>();
            services.AddScoped<ISystemNotificationsRepo, SystemNotificationsRepo>();
            services.AddScoped<ISentEmailRepo, SentEmailRepo>();
            services.AddTransient<ILocationDiagramRepo, LocationDiagramRepo>();
            services.AddScoped<ISecureLinkRepo, SecureLinkRepo>();
            services.AddScoped<IFunctionMapRepo, FunctionMapRepo>();
            services.AddScoped<IAppUserTestingDslRepo, AppUserTestingDslRepo>();
            services.AddScoped<IAppUserTestRunRepo, AppUserTestRunRepo>();
            services.AddScoped<IAuthViewRepo, AuthViewRepo>();
            services.AddScoped<IPendingIdentityRepo, PendingIdentityRepo>();
            services.AddScoped<IAppUserPasskeyCredentialRepo, PasskeyCredentialRepo>();
            services.AddScoped<IPasskeyCredentialIndexRepo, PasskeyCredentialIndexRepo>();
            services.AddTransient<ITestArtifactStorage, TestArtifactStorage>();
            services.AddTransient<IPasskeyChallengeStore, RedisPasskeyChallengeStore>();
            services.AddTransient<IMagicLinkAttemptStore, MagicLinkAttemptCacheStore>();

            services.AddScoped<IAppUserRepo, AppUserRepo>();
            services.AddScoped<IDeviceOwnerRepo, DeviceOwnerRepo>();
            services.AddScoped<IAppUserRelationalRepo, AppUserRelationalRepo>();
            services.AddScoped<IOrganizationRelationalRepo, OrganizationRelationalRepo>();
            services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();
            services.AddScoped<IDeviceOwnerRelationalRepo, DeviceOwnerRelationalRepo>();

            services.AddScoped<IOAuthSettings, OAuthSettings>();
            services.AddScoped<IUserAdminSettings, UserAdminSettings>();
            services.AddScoped<IRingCentralCredentials, RingCentralCredentials>();
           
            ErrorCodes.Register(typeof(UserAdminErrorCodes));
        }
    }
}

namespace LagoVista.DependencyInjection
{
    public static class UserAdminModule
    {
        public static void AddUserAdminModule(this IServiceCollection services, IConfigurationRoot configurationRoot, ILogger logger)
        {
            LagoVista.UserAdmin.Startup.ConfigureServices(services, configurationRoot);
            LagoVista.UserAdmin.Repos.Startup.ConfigureServices(services);
            services.AddMetaDataHelper<DistroList>();

        }
    }
}
