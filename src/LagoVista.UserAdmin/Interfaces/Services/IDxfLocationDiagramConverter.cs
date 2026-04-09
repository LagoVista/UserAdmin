using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Services
{
    public interface IDxfLocationDiagramConverter
    {
        Task<InvokeResult<LocationDiagram>> ConvertAsync(string dxfContents, DxfImportOptions options = null, CancellationToken cancellationToken = default);
        Task<InvokeResult<LocationDiagram>> ConvertAsync(Stream stream, DxfImportOptions options = null, CancellationToken cancellationToken = default);
    }
}
