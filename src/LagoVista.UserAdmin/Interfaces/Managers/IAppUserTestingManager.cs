using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Testing;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    /// <summary>
    /// Aggregates all server-side operations needed by the WPF auth ceremony test runner.
    /// Intended to be exposed via /api/testadmin controllers.
    /// V1: keep methods small and atomic; consolidate domains here for speed.
    /// </summary>
    public interface IAppUserTestingManager
    {
        #region Preconditions / Setup
        /// <summary>
        /// Ensure the fixed test user does not exist.
        /// </summary>
        Task<InvokeResult> DeleteTestUserAsync(EntityHeader org, EntityHeader user);

        /// <summary>
        /// Apply a declarative setup intent (seeds) and/or precondition snapshot for the fixed test user.
        /// This should only support the subset of fields the server can enforce.
        /// </summary>
        Task<InvokeResult<TestUserCredentials>> ApplySetupAsync(string testScenarioId, EntityHeader org, EntityHeader user);

        /// <summary>
        /// For out-of-band verification flows: return the last email token/code sent to the fixed test user.
        /// </summary>
        Task<InvokeResult<string>> GetLastEmailTokenAsync(EntityHeader org, EntityHeader user);

        /// <summary>
        /// For out-of-band verification flows: return the last sms token/code sent to the fixed test user.
        /// </summary>
        Task<InvokeResult<string>> GetLastSmsTokenAsync(EntityHeader org, EntityHeader user);

        Task<AppUser> GetTestUserAsync(EntityHeader org, EntityHeader user);
        #endregion


        #region DSL Case Management

        /// <summary>
        /// Create or update a DSL spec. If testSceenario.Id is empty, the store should assign one (create).
        /// ScenarioName is display-only (may still be unique, but not used as the key).
        /// </summary>
        Task<InvokeResult> AddTestScenarioAsync(AppUserTestScenario testSceenario, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateTestScenarioAsync(AppUserTestScenario testSceenario, EntityHeader org, EntityHeader user);
        Task<AppUserTestScenario> GetTestScenarioAsync(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<AppUserTestScenarioSummary>> GetTestScenariosForOrganizationAsync(ListRequest request, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteTestScenarioAsync(string id, EntityHeader org, EntityHeader user);

        Task<InvokeResult> AddAuthViewAsync(AuthView testSceenario, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateAuthViewAsync(AuthView testSceenario, EntityHeader org, EntityHeader user);
        Task<AuthView> GetAuthViewAsync(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<AuthViewSummary>> GetAuthViewsForOrgAsync(ListRequest request, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteAuthViewAsync(string id, EntityHeader org, EntityHeader user);

        #endregion

        #region Run Persistence

        Task<InvokeResult> AddTestRunAsync(AppUserTestRun run, List<ArtifactFlie> files, EntityHeader org, EntityHeader user);
        
        Task<ListResponse<AppUserTestRunSummary>> GetTestRunsAsync(ListRequest request, EntityHeader org, EntityHeader user);
        Task<AppUserTestRun> GetTestRunAsync(string runId, EntityHeader org, EntityHeader user);

        #endregion

        #region Auth Log Review
        Task<ListResponse<AuthenticationLog>> GetAuthLogReviewAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);

        #endregion

        Task<InvokeResult<AuthRunnerPlan>> BuildRunnerPlanAsync(string scenarioId, bool headless, EntityHeader org, EntityHeader user);
    }

    public class ArtifactFlie
    {
        public byte[] Buffer { get; set; }
        public string FileName { get; set; }
    
        public string ContentType { get; set; }
    }
}
