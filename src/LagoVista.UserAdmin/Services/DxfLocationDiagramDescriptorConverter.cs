using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Services
{
    public class DxfLocationDiagramDescriptorConverter : IDxfLocationDiagramDescriptorConverter
    {
        private readonly IDxfLocationDiagramConverter _locationDiagramConverter;

        public DxfLocationDiagramDescriptorConverter(IDxfLocationDiagramConverter locationDiagramConverter)
        {
            _locationDiagramConverter = locationDiagramConverter ?? throw new ArgumentNullException(nameof(locationDiagramConverter));
        }

        public async Task<InvokeResult<LocationDiagramDescriptor>> ConvertAsync(string dxfContents, DxfImportOptions options = null, CancellationToken cancellationToken = default)
        {
            var result = await _locationDiagramConverter.ConvertAsync(dxfContents, options, cancellationToken);
            if (!result.Successful)
            {
                result.ToInvokeResult<LocationDiagramDescriptor>();
            }

            return InvokeResult<LocationDiagramDescriptor>.Create(LocationDiagramDescriptorMapper.Map(result.Result));
        }

        public async Task<InvokeResult<LocationDiagramDescriptor>> ConvertAsync(Stream stream, DxfImportOptions options = null, CancellationToken cancellationToken = default)
        {
            var result = await _locationDiagramConverter.ConvertAsync(stream, options, cancellationToken);
            if (!result.Successful)
            {
                result.ToInvokeResult<LocationDiagramDescriptor>();
            }

            return InvokeResult<LocationDiagramDescriptor>.Create(LocationDiagramDescriptorMapper.Map(result.Result));
        }
    }
}
