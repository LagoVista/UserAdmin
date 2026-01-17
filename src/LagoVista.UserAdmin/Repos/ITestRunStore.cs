using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models.UIMetaData;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// Persistence abstraction for auth ceremony test runs.
    /// Storage implementation is provided elsewhere.
    /// </summary>
    public interface IAppUserTestRunRepo
    {
        /* Runs */

        Task CreateRunAsync(AppUserTestRun run);

        Task<AppUserTestRun> GetRunAsync(string runId);

        Task<ListResponse<AppUserTestRunSummary>> GetRunsFoOrgAsync(string orgId, ListRequest request);

        Task UpdateRunAsync(AppUserTestRun run);

        /* Events */

        /// <summary>
        /// Append events to a run.
        /// Storage must assign Seq (monotonic per run) and persist in order.
        /// Caller may send Seq=0.
        /// </summary>
        Task AppendEventsAsync(string runId, IEnumerable<AppUserTestRunEvent> events);

        /* Convenience */

        Task FinishRunAsync(string runId, TestRunStatus status, DateTime finishedUtc, TestRunVerification verification);
    }
}
