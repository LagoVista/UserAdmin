// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 79b495aa02e0d1aabf74f3ee0bf963e1ca82df4f40acf18c9efb95f9c7880ad4
// IndexVersion: 0
// --- END CODE INDEX META ---
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
