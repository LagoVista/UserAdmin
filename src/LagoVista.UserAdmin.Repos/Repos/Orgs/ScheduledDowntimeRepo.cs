using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class ScheduledDowntimeRepo : DocumentDBRepoBase<ScheduledDowntime>, IScheduledDowntimeRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public ScheduledDowntimeRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }
        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public Task AddScheduledDowntimeAsync(ScheduledDowntime scheduledDowntime)
        {
            return CreateDocumentAsync(scheduledDowntime);
        }

        public Task DeleteScheduledDowntimeAsync(string id)
        {
            return DeleteDocumentAsync(id)   ;
        }

        public Task<ScheduledDowntime> GetScheduledDowntimeAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<ListResponse<ScheduledDowntimeSummary>> GetScheduledDowntimesAsync(string orgId, ListRequest listRequest)
        {
            var lists = await QueryAsync(rec => rec.OwnerOrganization.Id == orgId, listRequest);

            return new ListResponse<ScheduledDowntimeSummary>()
            {
                Model = lists.Model.Select(dls => dls.CreateSummary()),
                NextPartitionKey = lists.NextPartitionKey,
                NextRowKey = lists.NextRowKey,
                PageCount = lists.PageCount,
                PageIndex = lists.PageIndex,
                HasMoreRecords = lists.HasMoreRecords,
                PageSize = lists.PageSize,
                ResultId = lists.ResultId
            };
        }

        public async Task<bool> QueryKeyInUseAsync(string key, string orgId)
        {
            return (await base.QueryAsync(attr => (attr.OwnerOrganization.Id == orgId || attr.IsPublic == true) && attr.Key == key)).Any();
        }

        public Task UpdateScheudledDowntimeAsync(ScheduledDowntime scheduledDowntime)
        {
            return UpsertDocumentAsync(scheduledDowntime);
        }
    }
}
