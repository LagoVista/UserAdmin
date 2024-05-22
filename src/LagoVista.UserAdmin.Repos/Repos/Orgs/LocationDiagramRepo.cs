using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class LocationDiagramRepo : DocumentDBRepoBase<LocationDiagram>, ILocationDiagramRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public LocationDiagramRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
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
