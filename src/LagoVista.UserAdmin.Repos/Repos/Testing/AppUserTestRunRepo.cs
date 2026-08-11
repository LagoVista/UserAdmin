using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Testing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Testing
{
    public class AppUserTestRunRepo : DocumentDBRepoBase<AppUserTestRun>, IAppUserTestRunRepo
    {
        public AppUserTestRunRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
        }

        public Task CreateRunAsync(AppUserTestRun run)
        {
            CompactForPersistence(run);
            return CreateDocumentAsync(run);
        }

        public Task<AppUserTestRun> GetRunAsync(string runId)
        {
            return GetDocumentAsync(runId);
        }

        public Task<ListResponse<AppUserTestRunSummary>> GetRunsFoOrgAsync(string orgId, ListRequest request)
        {
            return QuerySummaryAsync<AppUserTestRunSummary, AppUserTestRun>(qry => qry.OwnerOrganization.Id == orgId, qry => qry.LastUpdatedDate, request);
        }

        public Task<ListResponse<AppUserTestRunSummary>> GetRunsAsync(ListRequest request)
        {
            return QuerySummaryAsync<AppUserTestRunSummary, AppUserTestRun>(qry => true, qry => qry.LastUpdatedDate, request);
        }

        public Task UpdateRunAsync(AppUserTestRun run)
        {
            CompactForPersistence(run);
            return UpsertDocumentAsync(run);
        }

        /// <summary>
        /// The runner payload is intentionally richer than the durable execution receipt.
        /// Git owns the scenario definition, so Cosmos only retains the information needed
        /// to establish latest platform status, inspect failures, and review observed evidence.
        /// </summary>
        private static void CompactForPersistence(AppUserTestRun run)
        {
            if (run == null) return;

            run.RunCode = null;
            run.DeviceId = null;
            run.DeviceName = null;
            run.PlatformVersion = null;
            run.BaseUrl = null;
            run.StartPath = null;
            run.StartViewId = null;
            run.EndPath = null;
            run.Tags = new Dictionary<string, string>();
            run.Events = new List<AppUserTestRunEvent>();

            if (run.Verification == null) return;

            run.Verification.FinalSnapshot = null;
            run.Verification.ComputedDefaultLanding = null;
            run.Verification.ComputedIsFullyConfigured = null;
            run.Verification.Errors = new List<string>();

            if (run.Verification.AuthLogReview == null) return;

            run.Verification.AuthLogReview.FromUtc = null;
            run.Verification.AuthLogReview.ToUtc = null;

            if (run.Status == TestRunStatus.Passed)
                run.Verification.AuthLogReview.ExpectedEventsMissing = new List<string>();
        }
    }
}