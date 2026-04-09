using LagoVista.UserAdmin.Models.Orgs;
using System.Linq;

namespace LagoVista.UserAdmin.Services
{
    internal static class LocationDiagramDescriptorMapper
    {
        public static LocationDiagramDescriptor Map(LocationDiagram source)
        {
            if (source == null)
            {
                return null;
            }

            return new LocationDiagramDescriptor
            {
                ViewPortX = source.ViewPortX,
                ViewPortY = source.ViewPortY,
                Units = source.Units,
                Width = source.Width,
                Height = source.Height,
                Scale = source.Scale,
                Rotation = source.Rotation,
                TextColor = source.TextColor,
                Stroke = source.Stroke,
                Fill = source.Fill,
                Layers = source.Layers.Select(CloneLayer).ToList()
            };
        }

        public static void ApplyTo(LocationDiagram target, LocationDiagramDescriptor source)
        {
            target.ViewPortX = source.ViewPortX;
            target.ViewPortY = source.ViewPortY;
            target.Units = source.Units;
            target.Width = source.Width;
            target.Height = source.Height;
            target.Scale = source.Scale;
            target.Rotation = source.Rotation;
            target.TextColor = source.TextColor;
            target.Stroke = source.Stroke;
            target.Fill = source.Fill;
            target.Layers = source.Layers.Select(CloneLayer).ToList();
        }

        private static LocationDiagramLayer CloneLayer(LocationDiagramLayer source)
        {
            return new LocationDiagramLayer
            {
                Id = source.Id,
                Name = source.Name,
                Key = source.Key,
                Icon = source.Icon,
                Description = source.Description,
                Locked = source.Locked,
                Visible = source.Visible,
                Polylines = source.Polylines.Select(polyline => polyline.Select(point => new ShapePoint { X = point.X, Y = point.Y }).ToList()).ToList(),   
                Shapes = source.Shapes.Select(CloneShape).ToList(),
                Groups = source.Groups.Select(CloneGroup).ToList()
            };
        }

        private static LocationDiagramShape CloneShape(LocationDiagramShape source)
        {
            return new LocationDiagramShape
            {
                Id = source.Id,
                Name = source.Name,
                Key = source.Key,
                Icon = source.Icon,
                Notes = source.Notes,
                X = source.X,
                Y = source.Y,
                Width = source.Width,
                Height = source.Height,
                FlipX = source.FlipX,
                FlipY = source.FlipY,
                ShapeType = source.ShapeType,
                Rotation = source.Rotation,
                TextRotation = source.TextRotation,
                Scale = source.Scale,
                FontSize = source.FontSize,
                TextOffsetX = source.TextOffsetX,
                TextOffsetY = source.TextOffsetY,
                TextColor = source.TextColor,
                Stroke = source.Stroke,
                Fill = source.Fill,
                Locked = source.Locked,
                GeoLocationCenter = source.GeoLocationCenter,
                GeoPoints = source.GeoPoints?.ToList() ?? new System.Collections.Generic.List<LagoVista.Core.Models.Geo.GeoLocation>(),
                Devices = source.Devices?.ToList() ?? new System.Collections.Generic.List<DeviceReference>(),
                Points = source.Points.Select(point => new ShapePoint { X = point.X, Y = point.Y }).ToList(),
                VerticalAlign = source.VerticalAlign,
                HorizontalAlign = source.HorizontalAlign
            };
        }

        private static LocationDiagramShapeGroup CloneGroup(LocationDiagramShapeGroup source)
        {
            return new LocationDiagramShapeGroup
            {
                Id = source.Id,
                Name = source.Name,
                Key = source.Key,
                Icon = source.Icon,
                Description = source.Description,
                Shapes = source.Shapes.Select(shape => new LagoVista.Core.Models.EntityHeader
                {
                    Id = shape.Id,
                    Key = shape.Key,
                    Text = shape.Text
                }).ToList()
            };
        }
    }
}
