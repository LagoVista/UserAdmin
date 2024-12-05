using LagoVista.CloudStorage.DocumentDB;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System.Linq;
using System.Threading.Tasks;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Interfaces;
using LagoVista.Core;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class DistributionListRepo : DocumentDBRepoBase<DistroList>, IDistributionListRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public DistributionListRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, IDependencyManager dependencyMgr, ICacheProvider cacheProvider) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider, dependencyManager: dependencyMgr)
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

        public async Task<ListResponse<DistroListSummary>> GetDistroListsForCustomerAsync(string customerId, string orgId, ListRequest listRequest)
        {
            return await QuerySummaryAsync<DistroListSummary, DistroList>(rec => rec.OwnerOrganization.Id == orgId && rec.Customer.Id == customerId, rec => rec.Name, listRequest);
        }

        public async Task<ListResponse<DistroListSummary>> GetDistroListsForOrgAsync(string orgId, ListRequest listRequest)
        {
            return await QuerySummaryAsync<DistroListSummary, DistroList>(rec => rec.OwnerOrganization.Id == orgId, rec => rec.Name, listRequest);
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
