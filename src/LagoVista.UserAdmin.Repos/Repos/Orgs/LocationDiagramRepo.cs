// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fa173bff8055e93371d7463bec8bcd96de988cf6bacc318b62f5e486d4a12b76
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class LocationDiagramRepo : DocumentDBRepoBase<LocationDiagram>, ILocationDiagramRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public LocationDiagramRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyManager) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider, dependencyManager)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public Task AddLocationDiagramAsync(LocationDiagram diagramLocation)
        {
            return CreateDocumentAsync(diagramLocation);
        }

        public Task DeleteLocationDiagramAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<LocationDiagram> GetLocationDiagramAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsAsync(string orgId, ListRequest listRequest)
        {
            return QuerySummaryAsync<LocationDiagramSummary, LocationDiagram>(dig => dig.OwnerOrganization.Id == orgId, dig => dig.Name, listRequest);
        }

        public Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsForCustomerAsync(string orgId, string customerId, ListRequest listRequest)
        {
            return QuerySummaryAsync<LocationDiagramSummary, LocationDiagram>(dig => dig.OwnerOrganization.Id == orgId && dig.Customer.Id == customerId, dig => dig.Name, listRequest);
        }

        public Task UpdateLocationDiagramAsync(LocationDiagram diagramLocation)
        {
            return UpsertDocumentAsync(diagramLocation);
        }
    }
}
