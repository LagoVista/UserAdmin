using LagoVista.UserAdmin.Services;
using Npgsql.Internal;
using NUnit.Framework;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.Location
{
    [TestFixture]
    public class DxfImportTests
    {

        [Test]
        public async Task ImportAsLocation()
        {
            var converter = new DxfLocationDiagramConverter();
            using (var strm = System.IO.File.OpenRead("Testing.dxf"))
            {
                var output = await converter.ConvertAsync(strm);
            }
        }

        [Test]
        public async Task ImportAsLocationDescriptor()
        {
            var converter = new DxfLocationDiagramDescriptorConverter(new DxfLocationDiagramConverter());
            using (var strm = System.IO.File.OpenRead("Testing.dxf"))
            {
                var output = await converter.ConvertAsync(strm);
                foreach(var layer in output.Result.Layers)
                {
                    Console.WriteLine($"Layer: {layer.Name}");
                    foreach(var polyLine in layer.Polylines)
                    {
                        Console.WriteLine($"  Polyline with {polyLine.Count} points");
                    }
                }
            }
        }
    }
}
