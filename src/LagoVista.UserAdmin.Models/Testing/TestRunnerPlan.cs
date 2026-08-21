using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// Prepared runtime execution plan projected from one canonical auth scenario.
    /// UserAdmin owns state setup and value resolution; clients only navigate, populate,
    /// invoke one action, and verify the expected destination.
    /// </summary>
    [EntityDescription(
        Domains.AuthTesting, UserAdminResources.Names.AuthRunnerPlan_Name, UserAdminResources.Names.AuthRunnerPlan_Help,
        UserAdminResources.Names.AuthRunnerPlan_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),
        ClusterKey: "runner", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Primary,
        IndexPriority: 85, IndexTagsCsv: "authtesting,runner,configuration")]
    public class AuthRunnerPlan
    {
        public string RunId { get; set; }

        /// <summary>Compatibility header retained while DevTools migrates to the explicit scenario fields.</summary>
        public EntityHeader Scenario { get; set; }
        public string ScenarioCanonicalKey { get; set; }
        public string ScenarioName { get; set; }
        public int ScenarioDefinitionVersion { get; set; }
        public string ScenarioDefinitionHash { get; set; }

        /// <summary>Resolved start target for both supported UI runners.</summary>
        public AuthRunnerViewTarget Start { get; set; } = new AuthRunnerViewTarget();

        /// <summary>Expected destination after the single configured action is invoked.</summary>
        public AuthRunnerViewTarget Expected { get; set; } = new AuthRunnerViewTarget();

        public string BaseUrl { get; set; }

        /// <summary>Compatibility fields retained until DevTools consumes Start directly.</summary>
        public string StartRoute { get; set; }
        public string StartViewId { get; set; }

        public string EmailConfirmToken { get; set; }
        public string OrgInvitationId { get; set; }
        public List<AuthRunnerInput> Inputs { get; set; } = new List<AuthRunnerInput>();
        public AuthRunnerAction Action { get; set; } = new AuthRunnerAction();
        public AuthRunnerObservations Observations { get; set; } = new AuthRunnerObservations();
        public AuthRunnerOptions Options { get; set; } = new AuthRunnerOptions();

        /// <summary>
        /// Retained for provider/OAuth automation and compatibility. Normal scenario input
        /// values are resolved by UserAdmin before the plan is returned.
        /// </summary>
        public TestUserCredentials UserCredentials { get; set; }
    }

    public class AuthRunnerViewTarget
    {
        public string ViewId { get; set; }
        public string WebRoute { get; set; }
        public string MobileRoute { get; set; }
    }

    [EntityDescription(
        Domains.AuthTesting,
        UserAdminResources.Names.TestUserCredentials_Name,
        UserAdminResources.Names.TestUserCredentials_Help,
        UserAdminResources.Names.TestUserCredentials_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class TestUserCredentials
    {
        public string UserId { get; set; }
        public string InviteId { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string InvalidPassword { get; set; }
        public string PreloginLink { get; set; }
        public string MagicLinkToken { get; set; }
        public string PasskeyCredentialsId { get; set; }
        public string EmailConfirmationToken { get; set; }
        public string PasswordRecoveryCode { get; set; }
        public string EmailVerificationCode { get; set; }
        public string PasswordResetToken { get; set; }

        /// <summary>
        /// Optional provider simulator facts prepared by UserAdmin for external OAuth scenarios.
        /// DevTools uses these only to configure its local test IdP before invoking the normal UI action.
        /// </summary>
        public string OAuthProvider { get; set; }
        public string OAuthSubject { get; set; }
        public string OAuthEmail { get; set; }
        public bool? OAuthEmailVerified { get; set; }
        public string OAuthFirstName { get; set; }
        public string OAuthLastName { get; set; }
        public string OAuthOutcome { get; set; }
    }

    [EntityDescription(
        Domains.AuthTesting,
        UserAdminResources.Names.AuthRunnerInput_Name,
        UserAdminResources.Names.AuthRunnerInput_Help,
        UserAdminResources.Names.AuthRunnerInput_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class AuthRunnerInput
    {
        public string Name { get; set; }
        public string Finder { get; set; }

        /// <summary>Concrete value prepared by UserAdmin. DevTools should not resolve symbolic auth-model values.</summary>
        public string Value { get; set; }
        public string ValueType { get; set; }
        public bool Required { get; set; }
        public string Kind { get; set; }
    }

    [EntityDescription(
        Domains.AuthTesting,
        UserAdminResources.Names.AuthRunnerAction_Name,
        UserAdminResources.Names.AuthRunnerAction_Help,
        UserAdminResources.Names.AuthRunnerAction_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class AuthRunnerAction
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Finder { get; set; }
        public string ActionType { get; set; }
    }

    [EntityDescription(
        Domains.AuthTesting,
        UserAdminResources.Names.AuthRunnerObservations_Name,
        UserAdminResources.Names.AuthRunnerObservations_Help,
        UserAdminResources.Names.AuthRunnerObservations_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class AuthRunnerObservations
    {
        public string ScreenRootFinder { get; set; } = "[data-testid=\"auth-screen\"]";
        public string ScreenIdAttribute { get; set; } = "data-screen-id";

        /// <summary>Compatibility fields retained until DevTools consumes Expected directly.</summary>
        public string ExpectedEndViewId { get; set; }
        public string ExpectedEndRoute { get; set; }

        public List<string> ExpectedVisibleFinders { get; set; } = new List<string>();
        public string BusyStateFinder { get; set; }
    }

    [EntityDescription(
        Domains.AuthTesting,
        UserAdminResources.Names.AuthRunnerOptions_Name,
        UserAdminResources.Names.AuthRunnerOptions_Help,
        UserAdminResources.Names.AuthRunnerOptions_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class AuthRunnerOptions
    {
        public bool Headless { get; set; } = true;
        public int SlowMoMs { get; set; }
        public int TimeoutMs { get; set; } = 30000;
        public bool EnableTracing { get; set; }
    }

    [EntityDescription(
        Domains.AuthTesting,
        UserAdminResources.Names.AuthRunnerResult_Name,
        UserAdminResources.Names.AuthRunnerResult_Help,
        UserAdminResources.Names.AuthRunnerResult_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class AuthRunnerResult
    {
        public string RunId { get; set; }
        public string RunStarted { get; set; }
        public string RunEnded { get; set; }
        public AuthRunnerStatus Status { get; set; } = AuthRunnerStatus.Completed;
        public string FinalUrl { get; set; }
        public string FinalViewId { get; set; }
        public List<string> Notes { get; set; } = new List<string>();
        public List<AuthRunnerArtifact> Artifacts { get; set; } = new List<AuthRunnerArtifact>();
    }

    public enum AuthRunnerStatus
    {
        Completed = 0,
        Aborted = 1,
        FailedToExecute = 2
    }

    [EntityDescription(
        Domains.AuthTesting,
        UserAdminResources.Names.AuthRunnerArtifact_Name,
        UserAdminResources.Names.AuthRunnerArtifact_Help,
        UserAdminResources.Names.AuthRunnerArtifact_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class AuthRunnerArtifact
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string Ref { get; set; }
    }
}
