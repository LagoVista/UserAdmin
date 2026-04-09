using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Services
{
    public class LocationDiagramDescriptor
    {
        public double ViewPortX { get; set; } = 20;
        public double ViewPortY { get; set; } = 20;
        public EntityHeader<DiagramUnits> Units { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        public double Scale { get; set; }
        public double Rotation { get; set; }

        public string TextColor { get; set; }

        public string Stroke { get; set; }

        public string Fill { get; set; }

        public List<LocationDiagramLayer> Layers { get; set; } = new List<LocationDiagramLayer>();
    }
}
