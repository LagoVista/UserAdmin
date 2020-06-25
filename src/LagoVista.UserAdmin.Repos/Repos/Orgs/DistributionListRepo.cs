using LagoVista.CloudStorage.DocumentDB;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System.Linq;
using System.Threading.Tasks;
using LagoVista.Core.Models.UIMetaData;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class DistributionListRepo : DocumentDBRepoBase<DistroList>, IDistributionListRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public DistributionListRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public Task AddDistroListAsync(DistroList team)
        {
            return base.CreateDocumentAsync(team);
        }

        public Task DeleteDistroListAsync(string id)
        {
            return base.DeleteDocumentAsync(id);
        }

        public Task<DistroList> GetDistroListAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<ListResponse<DistroListSummary>> GetDistroListsForOrgAsync(string orgId, ListRequest listRequest)
        {
            var lists = await QueryAsync(rec => rec.OwnerOrganization.Id == orgId, listRequest);

            return new ListResponse<DistroListSummary>()
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

        public Task UpdateDistroListAsync(DistroList distroList)
        {
            return base.UpsertDocumentAsync(distroList);
        }
    }
}
