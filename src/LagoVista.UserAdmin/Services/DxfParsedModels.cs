using System.Collections.Generic;

namespace LagoVista.UserAdmin.Services
{
    internal sealed class DxfPair
    {
        public int Code { get; set; }
        public string Value { get; set; }
    }

    internal sealed class DxfIntermediateDocument
    {
        public List<DxfIntermediateLayer> Layers { get; } = new List<DxfIntermediateLayer>();
    }

    internal sealed class DxfIntermediateLayer
    {
        public string Name { get; set; }
        public List<DxfIntermediateEntity> Entities { get; } = new List<DxfIntermediateEntity>();
    }

    internal abstract class DxfIntermediateEntity
    {
        public string Layer { get; set; }
    }

    internal sealed class DxfPolylineEntity : DxfIntermediateEntity
    {
        public bool Closed { get; set; }
        public List<DxfPoint> Vertices { get; } = new List<DxfPoint>();
    }

    internal sealed class DxfCircleEntity : DxfIntermediateEntity
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Radius { get; set; }
    }

    internal sealed class DxfLineEntity : DxfIntermediateEntity
    {
        public DxfPoint Start { get; set; }
        public DxfPoint End { get; set; }
    }

    internal sealed class DxfPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
