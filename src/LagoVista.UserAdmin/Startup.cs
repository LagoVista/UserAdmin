using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

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
            services.AddScoped<IOAuthClientApplicationManager, OAuthClientApplicationManager>();
            services.AddScoped<IHolidaySetManager, HolidaySetManager>();
            services.AddScoped<ISubscriptionManager, SubscriptionManager>();
            services.AddScoped<ISubscriptionLevelManager, SubscriptionLevelManager>();
            services.AddScoped<IProvisionalEnvironmentManager, ProvisionalEnvironmentManager>();
            services.AddSingleton<IAnonymousVisitorPromotionOptions, AnonymousVisitorPromotionOptions>();
            services.AddScoped<IAnonymousVisitorPromotionManager, AnonymousVisitorPromotionManager>();
            services.AddScoped<IContinuityConversationManager, ContinuityConversationManager>();
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
            services.AddScoped<IPendingIdentityResolutionService, PendingIdentityResolutionService>();
            services.AddScoped<ICustomerUserManager, CustomerUserManager>();
            services.AddScoped<ICustomerAuthManager, CustomerAuthManager>();
            services.AddScoped<IFunctionMapManager, FunctionMapManager>();
            services.AddScoped<IPasswordLoginFlowHandler, PasswordLoginFlowHandler>();
            services.AddScoped<ITotpAuthenticationFlowHandler, TotpAuthenticationFlowHandler>();
            services.AddScoped<IRecoveryCodeAuthenticationFlowHandler, RecoveryCodeAuthenticationFlowHandler>();
            services.AddScoped<ITotpAdministrativeResetService, TotpAdministrativeResetService>();
            services.AddScoped<ISignOutFlowHandler, SignOutFlowHandler>();
            services.AddScoped<IAuthenticationFlowHandler<PasswordChangeFlowRequest>, PasswordChangeFlowHandler>();
            services.AddScoped<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>, PasswordRecoveryRequestFlowHandler>();
            services.AddScoped<IAuthenticationFlowHandler<PasswordRecoveryVerificationFlowRequest, string>, PasswordRecoveryVerificationFlowHandler>();
            services.AddScoped<IAuthenticationFlowHandler<PasswordRecoveryCompletionFlowRequest>, PasswordRecoveryCompletionFlowHandler>();
            services.AddScoped<IInvitationAcceptanceService, InvitationAcceptanceService>();
            services.AddScoped<IAuthenticationFlowHandler<InvitationAcceptanceFlowRequest, AcceptInviteResponse>, InvitationAcceptanceFlowHandler>();
            services.AddScoped<IAuthenticationFlowHandler<EmailVerificationFlowRequest>, EmailVerificationFlowHandler>();
            services.AddScoped<IAuthenticationFlowHandler<EmailVerificationSendFlowRequest, EmailVerificationSendResult>, EmailVerificationSendFlowHandler>();
            services.AddScoped<IAuthenticationFlowHandler<TotpEnrollmentBeginFlowRequest, AppUserTotpEnrollmentInfo>, TotpEnrollmentBeginFlowHandler>();
            services.AddScoped<IAuthenticationFlowHandler<TotpEnrollmentConfirmFlowRequest, List<string>>, TotpEnrollmentConfirmFlowHandler>();
            services.AddScoped<ITotpTurnOffFlowHandler, TotpTurnOffFlowHandler>();
            services.AddScoped<ITotpRecoveryCodeRotationFlowHandler, TotpRecoveryCodeRotationFlowHandler>();
            services.AddScoped<IAuthenticationFlowService, AuthenticationFlowService>();
            services.AddScoped<IMfaChallengeFlowService, MfaChallengeFlowService>();

            Services.Startup.ConfigureServices(services, configuration);

            services.AddScoped<IOrgInformationSource, OrgInformationSource>();
            services.AddScoped<IOrgIdentityData, OrgIdentityData>();

            services.AddScoped<IAppUserTestingManager, AppUserTestingManager>();
        }
    }
}
