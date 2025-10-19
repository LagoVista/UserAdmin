// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cc14fc85c0c16222a445b23b5370a4a0d674a1b3b21ddfc129c6f763c1c1f0a3
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ILocationDiagramManager
    {
        Task<InvokeResult> AddLocationDiagramAsync(LocationDiagram diagramLocation, EntityHeader org, EntityHeader user);
        Task<LocationDiagram> GetLocationDiagramAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateLocationDiagramAsync(LocationDiagram diagramLocation, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteLocationDiagramAsync(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsForCustomerAsync(string customerId, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);        
    }
}
