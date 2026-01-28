using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Testing;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// Declarative, self-contained specification for a single auth ceremony test.
    /// Intended to be JSON serializable/deserializable.
    /// V1: 1 DSL spec => 1 ceremony => 1 run.
    /// </summary>
    [EntityDescription(Domains.AuthTesting, UserAdminResources.Names.AuthDSL_Title, UserAdminResources.Names.AuthDSL_Help, UserAdminResources.Names.AuthDSL_Description,
     EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-fo-information-computer",
     ListUIUrl: "/sysadmin/testing/scenarios", EditUIUrl: "/sysadmin/testing/scenarios/{id}", CreateUIUrl: "/sysadmin/testing/scenarios/add", PreviewUIUrl: "/sysadmin/testing/scenarios/{id}/preview",
     SaveUrl: "/api/sys/testing/auth/scenario", GetListUrl: "/api/sys/testing/auth/scenarios", FactoryUrl: "/api/sys/testing/auth/scenario/factory", DeleteUrl: "/api/sys/testing/auth/scenario/{id}", GetUrl: "/api/sys/testing/auth/scenario/{id}")]
    public class AppUserTestScenario : EntityBase, ISummaryFactory, IValidateable, IFormDescriptor, IFormConditionalFields, IFormDescriptorCol2
    {
        /// <summary>
        /// Inputs for the runner (e.g., username/password, invite token, returnTo).
        /// Keep secrets out of this object if you plan to persist it.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingDSL_Inputs, HelpResource: UserAdminResources.Names.AppUserTestingDSL_Inputs_Help, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<AppUserTestSettingsValue> Inputs { get; set; } = new List<AppUserTestSettingsValue>();

        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingDSL_Action, HelpResource: UserAdminResources.Names.AppUserTestingDSL_Action_Help, FieldType: FieldTypes.Picker, IsRequired:true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader Action { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUserTestScenario_AuthView, HelpResource: UserAdminResources.Names.AppUserTestScenario_AuthView_Help, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader AuthView { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingExpectedOutcome_ExpectedLanding, HelpResource: UserAdminResources.Names.AppUserTestingExpectedOutcome_ExpectedLanding_Help, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader ExpectedView { get; set; }


        [FormField(
    LabelResource: UserAdminResources.Names.AppUserTestingExpectedOutcome_ExpectedAuthLogEvents,
    HelpResource: UserAdminResources.Names.AppUserTestingExpectedOutcome_ExpectedAuthLogEvents_Help,
    FieldType: FieldTypes.StringList,
    ResourceType: typeof(UserAdminResources))]
        public List<string> ExpectedAuthLogEvents { get; set; } = new List<string>();

        /// Preconditions to apply before executing the ceremony.
        /// This is declarative; the runner (or server-side setup helper) decides how to enforce.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingDSL_Preconditions, HelpResource: UserAdminResources.Names.AppUserTestingDSL_Preconditions_Help, FieldType: FieldTypes.ChildItem, ResourceType: typeof(UserAdminResources))]
        public AuthTenantStateSnapshot PreConditions { get; set; } = new AuthTenantStateSnapshot();

        /// <summary>
        /// Expected outcome after the ceremony completes.
        /// </summary>
        [FormField(LabelResource: UserAdminResources.Names.AppUserTestingDSL_Expected, HelpResource: UserAdminResources.Names.AppUserTestingDSL_Expected_Help, FieldType: FieldTypes.ChildItem, ResourceType: typeof(UserAdminResources))]
        public AuthTenantStateSnapshot PostConditions { get; set; } = new AuthTenantStateSnapshot();

        public string LastStatus { get; set; }

        public string LastRun { get; set; }

        public string LastError { get; set; }

        public AppUserTestScenarioSummary CreateSummary()
        {
            var summary = new AppUserTestScenarioSummary();
            summary.Populate(this);
            summary.LastError = LastError;
            summary.LastRun = LastRun;
            summary.LastStatus = LastStatus;
            return summary;
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string> { nameof(Inputs), nameof(Action) },
                Conditionals = new List<FormConditional>()
                 {
                     new FormConditional()
                     {
                         Field = nameof(AuthView),
                         Value = "*",
                         VisibleFields = new List<string>{ nameof(Inputs), nameof(Action) }
                     }
                 }
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Category),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(AuthView),
                nameof(Action),
                nameof(ExpectedView),
                nameof(ExpectedAuthLogEvents),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    public class AppUserTestSettingsValue
    {
        public string Finder { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }


    [EntityDescription(Domains.AuthTesting, UserAdminResources.Names.AuthDSL_Title, UserAdminResources.Names.AuthDSL_Help, UserAdminResources.Names.AuthDSL_Description,
     EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-fo-information-computer",
     ListUIUrl: "/sysadmin/testing/scenarios", EditUIUrl: "/sysadmin/testing/scenarios/{id}", CreateUIUrl: "/sysadmin/testing/scenarios/add", PreviewUIUrl: "/sysadmin/testing/scenarios/{id}/preview",
     SaveUrl: "/api/sys/testing/auth/scenario", GetListUrl: "/api/sys/testing/auth/scenarios", FactoryUrl: "/api/sys/testing/auth/scenario/factory", DeleteUrl: "/api/sys/testing/auth/scenario/{id}", GetUrl: "/api/sys/testing/auth/scenario/{id}")]
    public class AppUserTestScenarioSummary : SummaryData
    {
        public string LastError { get; set; }
        public string LastStatus { get; set; }
        public string LastRun { get; set; }
    }

    public static class TestUserSeed
    {
        public static string FirstName { get; } = "OAUTH";
        public static string LastName { get; } = "TESTING";
        public static string Email { get; } = "DEVTEST1@SOFTWARE-LOGISTICS.COM";
        public static string PhoneNumber {get; } = "6125551212";

        private const string TEST_USER_ID = "30458D0723764ACDBB10DA73AD98D088";
        private const string TEST_ORG1_ID = "5C00C94DB4D14B0E8E625F8FB47B9911";
        private const string TEST_ORG2_ID = "963F59BD3B0D43098212EB8EE26D3D3A";
        private const string TEST_ORG3_ID = "44A956C41AF0405AA5D7845FEB139B7B";

        public const string TEST_ORG_NS1 = "orgns1";
        public const string TEST_ORG_NS2 = "orgns2";
        public const string TEST_ORG_NS3 = "orgns3";

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
}