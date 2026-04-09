using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Services
{
    public class DxfImportOptions
    {
        public string DiagramName { get; set; } = "Imported Diagram";
        public string DiagramKey { get; set; } = "importeddiagram";
        public bool FlipY { get; set; } = true;
        public bool IgnoreOpenPolylines { get; set; } = true;
        public bool ApproximateCirclesAsPolygons { get; set; } = true;
        public int CircleSegmentCount { get; set; } = 32;
        public double Margin { get; set; } = 20;
        public DiagramUnits Units { get; set; } = DiagramUnits.Feet;
        public string DefaultStroke { get; set; } = "#000000";
        public string DefaultFill { get; set; } = "#f8f8f8";
        public string DefaultTextColor { get; set; } = "#000000";
        public string DefaultLayerIcon { get; set; } = "icon-fo-layer-3";
        public string DefaultShapeIcon { get; set; } = "icon-ae-ecommerce-1";
        public ShapeTypes DefaultPolylineShapeType { get; set; } = ShapeTypes.Polygon;
        public string LayerNamePrefix { get; set; } = "layer";
        public string ShapeNamePrefix { get; set; } = "shape";
    }
}
