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

        public Task UpdateLocationDiagramAsync(LocationDiagram diagramLocation)
        {
            return UpsertDocumentAsync(diagramLocation);
        }
    }
}
