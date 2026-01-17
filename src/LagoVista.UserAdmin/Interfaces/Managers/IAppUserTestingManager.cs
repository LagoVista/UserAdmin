using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Testing;
using System;
using System.Collections.Generic;
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

        Task<InvokeResult> SignInTestUser();

        Task<InvokeResult> SignOutTestUser();

        /// <summary>
        /// Ensure the fixed test user does not exist.
        /// </summary>
        Task<InvokeResult> DeleteTestUserAsync();

        /// <summary>
        /// Apply a declarative setup intent (seeds) and/or precondition snapshot for the fixed test user.
        /// This should only support the subset of fields the server can enforce.
        /// </summary>
        Task<InvokeResult> ApplySetupAsync(AuthTenantStateSnapshot preconditions, bool loginUser);

        /// <summary>
        /// For out-of-band verification flows: return the last email token/code sent to the fixed test user.
        /// </summary>
        Task<InvokeResult<string>> GetLastEmailTokenAsync();

        /// <summary>
        /// For out-of-band verification flows: return the last sms token/code sent to the fixed test user.
        /// </summary>
        Task<InvokeResult<string>> GetLastSmsTokenAsync();

        #endregion

        #region Snapshot Getter

        /// <summary>
        /// Get the current auth+tenant/org snapshot for the fixed test user.
        /// CeremonyId may influence computed fields (e.g., landing).
        /// </summary>
        Task<InvokeResult<AuthTenantStateSnapshot>> GetUserSnapshotAsync(string ceremonyId = null);

        /// <summary>
        /// Get computed values used for verification (e.g., default landing, fully configured) for the fixed test user.
        /// </summary>
        Task<InvokeResult<TestRunVerification>> GetVerificationAsync(string ceremonyId = null);

        #endregion

        #region DSL Case Management

        /// <summary>
        /// Create or update a DSL spec. If dsl.Id is empty, the store should assign one (create).
        /// ScenarioName is display-only (may still be unique, but not used as the key).
        /// </summary>
        Task<InvokeResult> CreateDslAsync(AppUserTestingDSL dsl);
        Task<InvokeResult> UpdateDslAsync(AppUserTestingDSL dsl);

        Task<InvokeResult<AppUserTestingDSL>> GetDslAsync(string id);
        Task<ListResponse<AppUserTestingDSLSummary>> ListDslAsync(ListRequest request);
        Task<InvokeResult> DeleteDslAsync(string id);

        #endregion

        #region Run Persistence

        Task<InvokeResult<AppUserTestRun>> CreateRunAsync(AppUserTestRun run);
        Task<InvokeResult> AppendRunEventAsync(string runId, AppUserTestRunEvent evt);
        Task<InvokeResult<AppUserTestRun>> FinishRunAsync(string runId, TestRunStatus status, TestRunVerification verification = null);

        Task<ListResponse<AppUserTestRunSummary>> GetTestRunsAsync(ListRequest request);
        Task<InvokeResult<AppUserTestRun>> GetRunAsync(string runId);

        #endregion

        #region Auth Log Review

        Task<InvokeResult<AuthLogReviewSummary>> GetAuthLogReviewAsync(DateTime fromUtc, DateTime toUtc);

        #endregion
    }
}
