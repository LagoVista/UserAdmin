using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Services
{
    public interface IDxfLocationDiagramDescriptorConverter
    {
        Task<InvokeResult<LocationDiagramDescriptor>> ConvertAsync(string dxfContents, DxfImportOptions options = null, CancellationToken cancellationToken = default);
        Task<InvokeResult<LocationDiagramDescriptor>> ConvertAsync(Stream stream, DxfImportOptions options = null, CancellationToken cancellationToken = default);
    }
}
