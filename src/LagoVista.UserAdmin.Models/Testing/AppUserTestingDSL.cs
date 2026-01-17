using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// Declarative, self-contained specification for a single auth ceremony test.
    /// Intended to be JSON serializable/deserializable.
    /// V1: 1 DSL spec => 1 ceremony => 1 run.
    /// </summary>
    public class AppUserTestingDSL : EntityBase, ISummaryFactory
    {
        /// <summary>
        /// Human-friendly name for reporting.
        /// </summary>
        public string ScenarioName { get; set; }

        /// <summary>
        /// The single ceremony to execute (semantic string id).
        /// Preferred over CeremonyType because segments are meaningful for grouping/filtering.
        /// Example: CeremonyIds.LoginWithPassword ("auth.login.password").
        /// </summary>
        public string CeremonyId { get; set; }

        public string StartUIPath { get; set; }

        /// <summary>
        /// Back-compat / convenience for existing code paths.
        /// Prefer CeremonyId for new work.
        /// </summary>
        public CeremonyTypes CeremonyType { get; set; }

        /// <summary>
        /// Inputs for the runner (e.g., username/password, invite token, returnTo).
        /// Keep secrets out of this object if you plan to persist it.
        /// </summary>
        public Dictionary<string, string> Inputs { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Setup intent / seeds to create baseline data before applying Preconditions.
        /// This is declarative; the runner (or server-side setup helper) decides how to enforce.
        /// </summary>
        public TestSetupSeeds Setup { get; set; } = new TestSetupSeeds();

        /// <summary>
        /// Preconditions to apply before executing the ceremony.
        /// This is declarative; the runner (or server-side setup helper) decides how to enforce.
        /// </summary>
        public AuthTenantStateSnapshot Preconditions { get; set; } = new AuthTenantStateSnapshot();

        /// <summary>
        /// Expected outcome after the ceremony completes.
        /// </summary>
        public ExpectedOutcome Expected { get; set; } = new ExpectedOutcome();
    
        public AppUserTestingDSLSummary CreateSummary()
        {
            var summary = new AppUserTestingDSLSummary();
            summary.Populate(this);
            return summary;
        }
        
        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    public class AppUserTestingDSLSummary : SummaryData
    {
        
    }

    public enum OrgFlowTypes
    {
        /// <summary>
        /// User was invited to an existing org/tenant and should join it as part of the ceremony flow.
        /// </summary>
        InvitedToExistingOrg,

        /// <summary>
        /// Brand-new user must complete setup screen to create an org before using the system.
        /// </summary>
        MustCreateOrgViaSetupScreen,
    }

    public static class TestUserSeed
    {
        public static string FirstName { get; } = "Fred";
        public static string LastName { get; } = "Flintstone";
        public static string Email { get; } = "fred@BedRock.com";
        public static string PhoneNumber {get; } = "6125551212";

        private const string TEST_USER_ID = "30458D0723764ACDBB10DA73AD98D088";
        private const string TEST_ORG1_ID = "5C00C94DB4D14B0E8E625F8FB47B9911";
        private const string TEST_ORG2_ID = "963F59BD3B0D43098212EB8EE26D3D3A";
        private const string TEST_ORG3_ID = "44A956C41AF0405AA5D7845FEB139B7B";

        public static EntityHeader User {get;} = new EntityHeader()
        {
            Id = TEST_USER_ID,
            Text = "Fred Flintstone"
        };

        public static  EntityHeader Org1 {get;} = new EntityHeader()
        {
            Id = TEST_ORG1_ID,
            Text = "Test Org 1"
        };

        public static EntityHeader Org2 {get;} = new EntityHeader()
        {
            Id = TEST_ORG2_ID,
            Text = "Test Org 2"
        };

        public static EntityHeader Org3    {get;} = new EntityHeader()
        {
            Id = TEST_ORG3_ID,
            Text = "Test Org 3"
        };

        public static EntityHeader InvitingUser = new EntityHeader()
        {
            Id = "A1B2C3D4E5F64718293A0B1C2D3E4F50",
            Text = "Barney Rubble"
        };
    }

    public class TestSetupSeeds
    {
        /// <summary>
        /// How org/tenant context should be established for this run.
        /// </summary>
        public OrgFlowTypes? OrgFlow { get; set; }

        /// <summary>
        /// For InvitedToExistingOrg flows, the runner may need an invite token or org id.
        /// (Exact usage depends on the ceremony.)
        /// </summary>
        public string InviteToken { get; set; }
        public string TargetOrgId { get; set; }

        /// <summary>
        /// For MustCreateOrgViaSetupScreen flows, the runner may need an org name to create.
        /// </summary>
        public string NewOrgName { get; set; }
    }

    public class ExpectedOutcome
    {
        /// <summary>
        /// Expected computed landing route after ceremony for fully configured users.
        /// Examples: /home, /home/welcome, /{redirect}
        /// </summary>
        public string ExpectedLanding { get; set; }

        /// <summary>
        /// Expected server-computed flags.
        /// </summary>
        public bool? IsFullyConfigured { get; set; }

        /// <summary>
        /// Expected postconditions on the user (auth + tenant/org state).
        /// Only fields that are non-null should be asserted by the verifier.
        /// </summary>
        public AuthTenantStateSnapshot Postconditions { get; set; } = new AuthTenantStateSnapshot();

        /// <summary>
        /// Expected auth log event presence (names/keys are system-defined).
        /// </summary>
        public List<string> ExpectedAuthLogEvents { get; set; } = new List<string>();
    }
}
