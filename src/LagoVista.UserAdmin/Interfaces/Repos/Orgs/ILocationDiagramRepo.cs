using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ILocationDiagramRepo
    {
        Task AddLocationDiagramAsync(LocationDiagram diagramLocation);
        Task<LocationDiagram> GetLocationDiagramAsync(string id);
        Task UpdateLocationDiagramAsync(LocationDiagram diagramLocation);
        Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsAsync(String orgId, ListRequest listRequest);
        Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsForCustomerAsync(String orgId, string customerId, ListRequest listRequest);
        Task DeleteLocationDiagramAsync(String id);
    }
}
