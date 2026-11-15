using LagoVista.CloudStorage.Storage;
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
using LagoVista.UserAdmin.Models.Security;
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
using LagoVista.UserAdmin.Repos.ApplicationData.AnonymousVisitors;
using LagoVista.UserAdmin.Repos.ApplicationData.ProvisionalEnvironments;
using LagoVista.UserAdmin.Repos.ApplicationData.AuthCodes;
using LagoVista.UserAdmin.Repos.ApplicationData.Notifications;
using LagoVista.UserAdmin.Repos.OperationalData.SecureLinks;
using LagoVista.UserAdmin.Repos.ApplicationData.Invitations;
using LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments;
using LagoVista.UserAdmin.Repos.TableStorage.ContinuityConversations;
using LagoVista.UserAdmin.Repos.Testing;
using LagoVista.UserAdmin.Repos.Users;
using LagoVista.UserAdmin.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LagoVista.CloudStorage.Storage.StorageProviders.Cassandra;

namespace LagoVista.UserAdmin.Repos
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddActivityRecordStore<AccessLog, CassandraActivityRecordStore<AccessLog>>(AccessLogRepo.ConfigureStorage);
            services.AddActivityRecordStore<AuthenticationLog, CassandraActivityRecordStore<AuthenticationLog>>(AuthenticationLogRepo.ConfigureStorage);
            services.AddOperationalDataStore<SentEmailXRefOperationalRecord, CassandraOperationalDataStore<SentEmailXRefOperationalRecord>>();
            services.AddScoped<SentEmailXRefRepo>();

            services.AddScoped<IAppUserLoaderRepo, AppUserLoadRepo>();
            services.AddScoped<ILocationUserRepo, LocationUserRepo>();
            services.AddConditionalOperationalDataStore<RefreshTokenOperationalRecord>(RefreshTokenRepo.ConfigureStorage);
            services.AddScoped<ILegacyRefreshTokenReader, LegacyRefreshTokenReader>();
            services.AddScoped<IRefreshTokenRepo, RefreshTokenRepo>();
            services.ConfigureApplicationData<InvitationRecord>(storage => storage
                .Index(r => r.OrgId)
                .Index(r => r.EmailKey)
                .Index(r => r.StatusKey)
                .Index(r => r.VisibleAsActive)
                .Index(r => r.IssuedUtc));
            services.AddScoped<LegacyInvitationReader>();
            services.AddScoped<IInviteUserRepo, InviteUserRepo>();
            services.AddScoped<IOrgUserRepo, OrgUserRepo>();
            services.AddScoped<IOwnedObjectRepo, OwnedObjectRepo>();
            services.AddScoped<IOrgLocationRepo, OrgLocationRepo>();
            services.AddScoped<IBusinessUnitRepo, BusinessUnitRepo>();
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
            services.AddScoped<IOAuthClientApplicationRepo, OAuthClientApplicationRepo>();
            services.AddScoped<ITeamRepo, TeamRepo>();
            services.AddScoped<ITeamUserRepo, TeamUserRepo>();
            services.AddScoped<IAssetSetRepo, AssetSetRepo>();
            services.AddScoped<IManagedAssetRepo, ManagedAssetRepo>();
            services.AddScoped<IAppInstanceRepo, AppInstanceRepo>();
            services.AddScoped<IUserRoleRepo, UserRoleRepo>();
            services.AddScoped<IRoleAccessRepo, RoleAccessRepo>();
            // Dedicated short-lived authentication records in Mongo ApplicationData.
            services.ConfigureApplicationData<SingleUseTokenRecord>(storage => storage
                .RetainFor(SingleUseTokenRecord.Retention));
            services.ConfigureApplicationData<VerificationCodeRecord>(storage => storage
                .Index(r => r.UserId)
                .Index(r => r.Kind)
                .Index(r => r.IssuedUtc)
                .RetainFor(VerificationCodeRecord.Retention));
            services.AddTransient<ISingleUseTokenRepo, SingleUseTokenRepo>();
            services.AddTransient<IPasswordResetCodeRepo, PasswordResetCodeRepo>();
            services.AddTransient<IEmailVerificationCodeRepo, EmailVerificationCodeRepo>();
            services.AddScoped<IDistributionListRepo, DistributionListRepo>();
            services.AddScoped<ICalendarRepo, CalendarRepo>();
            services.AddScoped<IMostRecentlyUsedRepo, MostRecentUsedRepo>();
            services.AddScoped<IUserFavoritesRepo, UserFavoritesRepo>();
            services.ConfigureApplicationData<AppUserInboxRecord>(storage => storage
                .Index(r => r.PartitionKey)
                .Index(r => r.RowKey)
                .Index(r => r.Viewed)
                .RetainFor(AppUserInboxRecord.Retention));
            services.ConfigureApplicationData<SystemNotificationRecord>(storage => storage
                .Index(r => r.PartitionKey)
                .Index(r => r.RowKey)
                .Index(r => r.Index)
                .RetainFor(SystemNotificationRecord.Retention));
            services.AddScoped<IAppUserInboxRepo, AppUserIndboxRepo>();
            services.AddScoped<IAuthenticationLogRepo, AuthenticationLogRepo>();
            services.AddScoped<ISystemNotificationsRepo, SystemNotificationsRepo>();
            services.AddScoped<ISentEmailRepo, SentEmailRepo>();
            services.AddTransient<ILocationDiagramRepo, LocationDiagramRepo>();
            services.AddConditionalOperationalDataStore<SecureLinkOperationalRecord>(SecureLinkRepo.ConfigureStorage);
            services.AddScoped<LegacySecureLinkReader>();
            services.AddScoped<ISecureLinkRepo, SecureLinkRepo>();
            services.AddScoped<IFunctionMapRepo, FunctionMapRepo>();
            services.AddScoped<IAppUserTestingDslRepo, AppUserTestingDslRepo>();
            services.AddScoped<IAppUserTestRunRepo, AppUserTestRunRepo>();
            services.AddScoped<IAuthViewRepo, AuthViewRepo>();
            services.AddScoped<IPendingIdentityRepo, PendingIdentityRepo>();
            services.AddScoped<IAppUserPasskeyCredentialRepo, PasskeyCredentialRepo>();
            services.AddScoped<IPasskeyCredentialIndexRepo, PasskeyCredentialIndexRepo>();
            services.ConfigureApplicationData<AnonymousVisitorRecord>(storage => storage
                .Index(r => r.Visitor.ContinuityTokenHash)
                .Index(r => r.Visitor.InstallationIdHash)
                .Index(r => r.StateKey)
                .Index(r => r.LifecycleDueUtc)
                .RetainFor(AnonymousVisitorRepo.Retention));
            services.AddScoped<IAnonymousVisitorRepo, AnonymousVisitorRepo>();
            services.AddScoped<IContinuityConversationRepo, ContinuityConversationRepo>();
            // Prefer manager archive/accounting/purge. A 180-day TTL is a
            // tracking-record cleanup backstop, not a linked-resource purge.
            services.ConfigureApplicationData<ProvisionalEnvironmentRecord>(storage => storage
                .Index(r => r.Environment.RecoveryTokenHash)
                .Index(r => r.Environment.InstallationIdHash)
                .Index(r => r.StateKey)
                .Index(r => r.LifecycleDueUtc)
                .RetainFor(ProvisionalEnvironmentRepo.Retention));
            services.AddScoped<IProvisionalEnvironmentRepo, ProvisionalEnvironmentRepo>();
            services.AddScoped<IProvisionalEnvironmentArchiveStore, ProvisionalEnvironmentArchiveStore>();
            services.AddScoped<IProvisionalEnvironmentBillingArchiveRepo, ProvisionalEnvironmentBillingArchiveRepo>();
            services.AddScoped<IProvisionalEnvironmentArchiveAccountingService, ProvisionalEnvironmentArchiveAccountingService>();
            services.AddTransient<ITestArtifactStorage, TestArtifactStorage>();
            services.AddTransient<IPasskeyChallengeStore, RedisPasskeyChallengeStore>();
            services.AddTransient<IMfaChallengeStore, RedisMfaChallengeStore>();
            services.AddTransient<IMagicLinkAttemptStore, MagicLinkAttemptCacheStore>();

            services.AddScoped<IAppUserRepo, AppUserRepo>();
            services.AddScoped<IDeviceOwnerRepo, DeviceOwnerRepo>();
            services.AddScoped<IAppUserRelationalRepo, AppUserRelationalRepo>();
            services.AddScoped<IOrganizationRelationalRepo, OrganizationRelationalRepo>();
            services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();
            services.AddScoped<ISubscriptionLevelRepo, SubscriptionLevelRepo>();
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
