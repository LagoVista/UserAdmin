
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Testing;
using RingCentral;

namespace LagoVista.UserAdmin.Repos.Testing
{
    public class AppUserTestRunRepo : DocumentDBRepoBase<AppUserTestRun>, IAppUserTestRunRepo
    {
        bool _shouldConsolidateCollections;
        public AppUserTestRunRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) : 
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;


        public async Task AppendEventsAsync(string runId, IEnumerable<AppUserTestRunEvent> events)
        {
            var run = await GetDocumentAsync(runId);

            var idx = run.Events.Count;

            foreach(var evt in events)
            {
                evt.Seq = idx++;
                run.Events.Add(evt);
            }
            await UpsertDocumentAsync(run); 
        }

        public Task CreateRunAsync(AppUserTestRun run)
        {
            return CreateDocumentAsync(run);
        }

        public async Task FinishRunAsync(string runId, TestRunStatus status, DateTime finishedUtc, TestRunVerification verification)
        {
            var run = await GetDocumentAsync(runId);
            run.Status = status;
            run.FinishedUtc = finishedUtc;
            run.Verification = verification;
            await UpsertDocumentAsync(run); }

        public Task<AppUserTestRun> GetRunAsync(string runId)
        {
            return GetDocumentAsync(runId);
        }

        public Task<ListResponse<AppUserTestRunSummary>> GetRunsFoOrgAsync(string orgId, ListRequest request)
        {
            return QuerySummaryDescendingAsync< AppUserTestRunSummary, AppUserTestRun>(qry => qry.OwnerOrganization.Id == orgId, qry=>qry.LastUpdatedDate, request);
        }

        public Task UpdateRunAsync(AppUserTestRun run)
        {
            return UpsertDocumentAsync(run);
        }
    }
}