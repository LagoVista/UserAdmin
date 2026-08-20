using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// Runtime execution projection of a canonical auth-model/scenarios-v2 definition.
    /// Git owns the authored definition; this object contains the subset needed to list,
    /// plan, execute, and verify a scenario.
    /// </summary>
    [EntityDescription(
        Domains.AuthTesting, UserAdminResources.Names.AuthDSL_Title, UserAdminResources.Names.AuthDSL_Help, UserAdminResources.Names.AuthDSL_Description,
        EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),
        SaveUrl: "/api/sys/testing/auth/scenario", GetListUrl: "/api/sys/testing/auth/scenarios", FactoryUrl: "/api/sys/testing/auth/scenario/factory",
        DeleteUrl: "/api/sys/testing/auth/scenario/{id}", GetUrl: "/api/sys/testing/auth/scenario/{id}",
        ListUIUrl: "/sysadmin/testing/scenarios", EditUIUrl: "/sysadmin/testing/scenarios/{id}", CreateUIUrl: "/sysadmin/testing/scenarios/add",
        PreviewUIUrl: "/sysadmin/testing/scenarios/{id}/preview",
        Icon: "icon-fo-information-computer", ClusterKey: "scenarios", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration,
        Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true,
        IndexTier: EntityDescriptionAttribute.IndexTiers.Primary, IndexPriority: 85, IndexTagsCsv: "authtesting,scenarios,configuration")]
    public class AppUserTestScenario : EntityBase, ISummaryFactory, IValidateable, IFormDescriptor, IFormConditionalFields, IFormDescriptorCol2
    {
        public string CanonicalKey { get; set; }
        public string SchemaVersion { get; set; }
        public int DefinitionVersion { get; set; }
        public string Maturity { get; set; }
        public string CategoryKey { get; set; }
        public string DefinitionHash { get; set; }

        public string ActionId { get; set; }
        public string ActionFinder { get; set; }

        public string PreconditionExpression { get; set; }
        public string PostconditionExpression { get; set; }

        public bool ServerInteractionRequired { get; set; }
        public string ServerInteractionIntent { get; set; }
        public List<string> TransitionKeys { get; set; } = new List<string>();
        public List<string> ExpectedVisibleFinders { get; set; } = new List<string>();
        public List<string> EvidenceRequirements { get; set; } = new List<string>();

        /// <summary>
        /// Inputs for the runner. Secret-reference values are symbolic and are resolved
        /// against TestUserCredentials while building the runner plan.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingDSL_Inputs, HelpResource: UserAdminResources.Names.AppUserTestingDSL_Inputs_Help, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<AppUserTestSettingsValue> Inputs { get; set; } = new List<AppUserTestSettingsValue>();

        /// <summary>
        /// Legacy runtime header retained for existing clients. Canonical action identity
        /// is carried explicitly by ActionId and ActionFinder.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingDSL_Action, HelpResource: UserAdminResources.Names.AppUserTestingDSL_Action_Help, FieldType: FieldTypes.Picker, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader Action { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUserTestScenario_AuthView, HelpResource: UserAdminResources.Names.AppUserTestScenario_AuthView_Help, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader AuthView { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingExpectedOutcome_ExpectedLanding, HelpResource: UserAdminResources.Names.AppUserTestingExpectedOutcome_ExpectedLanding_Help, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader ExpectedView { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingExpectedOutcome_ExpectedAuthLogEvents, HelpResource: UserAdminResources.Names.AppUserTestingExpectedOutcome_ExpectedAuthLogEvents_Help, FieldType: FieldTypes.StringList, ResourceType: typeof(UserAdminResources))]
        public List<string> ExpectedAuthLogEvents { get; set; } = new List<string>();

        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingDSL_Preconditions, HelpResource: UserAdminResources.Names.AppUserTestingDSL_Preconditions_Help, FieldType: FieldTypes.ChildItem, ResourceType: typeof(UserAdminResources))]
        public AuthTenantStateSnapshot PreConditions { get; set; } = new AuthTenantStateSnapshot();

        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingDSL_Expected, HelpResource: UserAdminResources.Names.AppUserTestingDSL_Expected_Help, FieldType: FieldTypes.ChildItem, ResourceType: typeof(UserAdminResources))]
        public AuthTenantStateSnapshot PostConditions { get; set; } = new AuthTenantStateSnapshot();

        /// <summary>
        /// Derived runtime status only. These values are populated from the latest persisted
        /// run receipts and are never written back to canonical Git definitions.
        /// </summary>
        public AppUserTestPlatformStatus WebStatus { get; set; } = AppUserTestPlatformStatus.Create(AppUserTestPlatform.Web);
        public AppUserTestPlatformStatus AndroidStatus { get; set; } = AppUserTestPlatformStatus.Create(AppUserTestPlatform.Android);
        public AppUserTestPlatformStatus IOSStatus { get; set; } = AppUserTestPlatformStatus.Create(AppUserTestPlatform.IOS);

        public AppUserTestScenarioSummary CreateSummary()
        {
            var summary = new AppUserTestScenarioSummary();
            summary.Populate(this);
            summary.CanonicalKey = CanonicalKey;
            summary.DefinitionVersion = DefinitionVersion;
            summary.CategoryKey = CategoryKey;
            summary.EvidenceRequirements = new List<string>(EvidenceRequirements ?? new List<string>());
            summary.WebStatus = WebStatus;
            summary.AndroidStatus = AndroidStatus;
            summary.IOSStatus = IOSStatus;
            return summary;
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals
            {
                ConditionalFields = new List<string> { nameof(Inputs), nameof(Action) },
                Conditionals = new List<FormConditional>
                {
                    new FormConditional
                    {
                        Field = nameof(AuthView),
                        Value = "*",
                        VisibleFields = new List<string> { nameof(Inputs), nameof(Action) }
                    }
                }
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string> { nameof(Name), nameof(Key), nameof(Category) };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string> { nameof(AuthView), nameof(Action), nameof(ExpectedView), nameof(ExpectedAuthLogEvents) };
        }

        ISummaryData ISummaryFactory.CreateSummary() => CreateSummary();
    }

    public class AppUserTestSettingsValue
    {
        public string Finder { get; set; }
        public string Name { get; set; }
        public string ValueType { get; set; }
        public bool Required { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }

    [EntityDescription(Domains.AuthTesting, UserAdminResources.Names.AuthDSL_Title, UserAdminResources.Names.AuthDSL_Help, UserAdminResources.Names.AuthDSL_Description,
        EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-fo-information-computer",
        ListUIUrl: "/sysadmin/testing/scenarios", EditUIUrl: "/sysadmin/testing/scenarios/{id}", CreateUIUrl: "/sysadmin/testing/scenarios/add", PreviewUIUrl: "/sysadmin/testing/scenarios/{id}/preview",
        SaveUrl: "/api/sys/testing/auth/scenario", GetListUrl: "/api/sys/testing/auth/scenarios", FactoryUrl: "/api/sys/testing/auth/scenario/factory", DeleteUrl: "/api/sys/testing/auth/scenario/{id}", GetUrl: "/api/sys/testing/auth/scenario/{id}")]
    public class AppUserTestScenarioSummary : SummaryData
    {
        public string CanonicalKey { get; set; }
        public int DefinitionVersion { get; set; }
        public List<string> EvidenceRequirements { get; set; } = new List<string>();
        public AppUserTestPlatformStatus WebStatus { get; set; } = AppUserTestPlatformStatus.Create(AppUserTestPlatform.Web);
        public AppUserTestPlatformStatus AndroidStatus { get; set; } = AppUserTestPlatformStatus.Create(AppUserTestPlatform.Android);
        public AppUserTestPlatformStatus IOSStatus { get; set; } = AppUserTestPlatformStatus.Create(AppUserTestPlatform.IOS);
    }

    public class AppUserTestPlatformStatus
    {
        public AppUserTestPlatform Platform { get; set; }
        public TestRunStatus Status { get; set; } = TestRunStatus.Created;
        public string LastRun { get; set; }
        public string LastRunId { get; set; }
        public string FinalViewId { get; set; }
        public string ErrorMessage { get; set; }

        public static AppUserTestPlatformStatus Create(AppUserTestPlatform platform)
        {
            return new AppUserTestPlatformStatus { Platform = platform };
        }
    }

    public static class TestUserSeed
    {
        public static string FirstName { get; } = "OAUTH";
        public static string LastName { get; } = "TESTING";
        public static string Email { get; } = "DEVTEST1@SOFTWARE-LOGISTICS.COM";
        public static string PhoneNumber { get; } = "6125551212";

        private const string TEST_USER_ID = "30458D0723764ACDBB10DA73AD98D088";
        private const string TEST_ORG1_ID = "5C00C94DB4D14B0E8E625F8FB47B9911";
        private const string TEST_ORG2_ID = "963F59BD3B0D43098212EB8EE26D3D3A";
        private const string TEST_ORG3_ID = "44A956C41AF0405AA5D7845FEB139B7B";

        public const string TEST_ORG_NS1 = "orgns1";
        public const string TEST_ORG_NS2 = "orgns2";
        public const string TEST_ORG_NS3 = "orgns3";

        public static EntityHeader User { get; } = new EntityHeader { Id = TEST_USER_ID, Text = "Fred Flintstone" };
        public static EntityHeader Org1 { get; } = new EntityHeader { Id = TEST_ORG1_ID, Text = "Test Org 1" };
        public static EntityHeader Org2 { get; } = new EntityHeader { Id = TEST_ORG2_ID, Text = "Test Org 2" };
        public static EntityHeader Org3 { get; } = new EntityHeader { Id = TEST_ORG3_ID, Text = "Test Org 3" };
        public static EntityHeader InvitingUser { get; } = new EntityHeader { Id = "A1B2C3D4E5F64718293A0B1C2D3E4F50", Text = "Barney Rubble" };
    }
}
