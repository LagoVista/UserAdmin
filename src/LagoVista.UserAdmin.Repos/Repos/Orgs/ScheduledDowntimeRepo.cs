// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d9079d4a25d986efe45a511d2d7ed178e6227158a611ba8fc26bcc205c5147c3
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
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

        public ScheduledDowntimeRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
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

        public Task<ListResponse<ScheduledDowntimeSummary>> GetScheduledDowntimesAsync(string orgId, ListRequest listRequest)
        {
            return  QuerySummaryAsync<ScheduledDowntimeSummary, ScheduledDowntime>(rec => rec.OwnerOrganization.Id == orgId, rec=>rec.Name, listRequest);
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
