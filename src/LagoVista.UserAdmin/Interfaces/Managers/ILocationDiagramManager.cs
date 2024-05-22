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
        Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);        
    }
}
