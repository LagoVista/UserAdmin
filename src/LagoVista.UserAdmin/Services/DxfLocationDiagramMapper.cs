using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LagoVista.UserAdmin.Services
{
    internal static class DxfLocationDiagramMapper
    {
        private const double PointTolerance = 0.01d;
        private const double MinimumDrawableWidthOrHeight = 0.01d;
        private const double MaxImportX = 15000d;
        private const double ContentMarginPercent = 0.10d;

        public static LocationDiagram Map(DxfIntermediateDocument document, DxfImportOptions options)
        {
            var diagram = new LocationDiagram
            {
                Name = options.DiagramName,
                Key = Slugify(options.DiagramKey),
                Stroke = options.DefaultStroke,
                Fill = options.DefaultFill,
                TextColor = options.DefaultTextColor,
                Units = EntityHeader<DiagramUnits>.Create(options.Units)
            };

            var layerArtifacts = new List<LayerArtifacts>();
            foreach (var sourceLayer in document.Layers)
            {
                var layerName = string.IsNullOrWhiteSpace(sourceLayer.Name) ? options.LayerNamePrefix : sourceLayer.Name.Trim();
                var targetLayer = new LocationDiagramLayer
                {
                    Name = layerName,
                    Key = Slugify("layer" + layerName),
                    Icon = options.DefaultLayerIcon,
                    Visible = true,
                    Locked = false,
                    Polylines = new List<List<ShapePoint>>()
                };

                var artifacts = new LayerArtifacts
                {
                    Layer = targetLayer
                };

                var keptLines = sourceLayer.Entities
                    .OfType<DxfLineEntity>()
                    .Where(ShouldKeepLine)
                    .ToList();

                var mergedLinePolylines = MergeLinesToPolylines(keptLines);
                foreach (var polyline in mergedLinePolylines)
                {
                    var points = NormalizePolylineVertices(polyline.Vertices, false);
                    if (points.Count < 2)
                    {
                        continue;
                    }

                    if (!HasDrawableGeometry(points))
                    {
                        continue;
                    }

                    artifacts.WorldPolylines.Add(points);
                }

                var shapeIndex = 1;
                foreach (var entity in sourceLayer.Entities.Where(entity => !(entity is DxfLineEntity)))
                {
                    var rawShape = CreateRawShape(entity, shapeIndex, options);
                    if (!ShouldInclude(rawShape))
                    {
                        continue;
                    }

                    if (!ShouldKeepShape(rawShape))
                    {
                        continue;
                    }

                    shapeIndex++;
                    artifacts.Shapes.Add(rawShape);
                    targetLayer.Shapes.Add(rawShape.Shape);
                }

                if (artifacts.WorldPolylines.Count > 0 || targetLayer.Shapes.Count > 0)
                {
                    diagram.Layers.Add(targetLayer);
                    layerArtifacts.Add(artifacts);
                }
            }

            if (layerArtifacts.Count == 0)
            {
                return diagram;
            }

            NormalizeArtifacts(layerArtifacts, diagram, options);
            return diagram;
        }

        private static RawImportedShape CreateRawShape(DxfIntermediateEntity entity, int shapeIndex, DxfImportOptions options)
        {
            if (entity is DxfPolylineEntity polyline)
            {
                return CreateRawPolylineShape(polyline, shapeIndex, options);
            }

            if (entity is DxfCircleEntity circle)
            {
                return CreateRawCircleShape(circle, shapeIndex, options);
            }

            return null;
        }

        private static RawImportedShape CreateRawPolylineShape(DxfPolylineEntity polyline, int shapeIndex, DxfImportOptions options)
        {
            if (polyline.Vertices.Count < 2)
            {
                return null;
            }

            var points = NormalizePolylineVertices(polyline.Vertices, polyline.Closed);
            if (points.Count < 2)
            {
                return null;
            }

            var name = $"{options.ShapeNamePrefix}{shapeIndex}";
            var key = Slugify(name);
            var shapeType = polyline.Closed ? ShapeTypes.Polygon : ShapeTypes.Polyline;

            var shape = new LocationDiagramShape
            {
                Name = name,
                Key = key,
                Icon = options.DefaultShapeIcon,
                ShapeType = EntityHeader<ShapeTypes>.Create(shapeType),
                Stroke = options.DefaultStroke,
                Fill = options.DefaultFill,
                TextColor = options.DefaultTextColor,
                Scale = 1,
                Rotation = 0,
                TextRotation = 0,
                Locked = false
            };

            return new RawImportedShape
            {
                Shape = shape,
                Points = points
            };
        }

        private static RawImportedShape CreateRawCircleShape(DxfCircleEntity circle, int shapeIndex, DxfImportOptions options)
        {
            var name = $"{options.ShapeNamePrefix}{shapeIndex}";
            var key = Slugify(name);

            if (options.ApproximateCirclesAsPolygons)
            {
                var segmentCount = options.CircleSegmentCount < 8 ? 8 : options.CircleSegmentCount;
                var points = new List<DxfPoint>(segmentCount);
                for (var index = 0; index < segmentCount; index++)
                {
                    var angle = (Math.PI * 2d * index) / segmentCount;
                    points.Add(new DxfPoint
                    {
                        X = circle.CenterX + (circle.Radius * Math.Cos(angle)),
                        Y = circle.CenterY + (circle.Radius * Math.Sin(angle))
                    });
                }

                var polygonShape = new LocationDiagramShape
                {
                    Name = name,
                    Key = key,
                    Icon = options.DefaultShapeIcon,
                    ShapeType = EntityHeader<ShapeTypes>.Create(ShapeTypes.Polygon),
                    Stroke = options.DefaultStroke,
                    Fill = options.DefaultFill,
                    TextColor = options.DefaultTextColor,
                    Scale = 1,
                    Rotation = 0,
                    TextRotation = 0,
                    Locked = false
                };

                return new RawImportedShape
                {
                    Shape = polygonShape,
                    Points = points
                };
            }

            var circleShape = new LocationDiagramShape
            {
                Name = name,
                Key = key,
                Icon = options.DefaultShapeIcon,
                ShapeType = EntityHeader<ShapeTypes>.Create(ShapeTypes.Circle),
                Stroke = options.DefaultStroke,
                Fill = options.DefaultFill,
                TextColor = options.DefaultTextColor,
                Scale = 1,
                Rotation = 0,
                TextRotation = 0,
                Locked = false,
                X = circle.CenterX - circle.Radius,
                Y = circle.CenterY - circle.Radius,
                Width = circle.Radius * 2d,
                Height = circle.Radius * 2d
            };

            return new RawImportedShape
            {
                Shape = circleShape,
                Points = new List<DxfPoint>()
            };
        }

        private static List<DxfPolylineEntity> MergeLinesToPolylines(List<DxfLineEntity> lines)
        {
            var usableLines = lines
                .Where(line => line.Start != null && line.End != null && !AreSamePoint(line.Start, line.End))
                .ToList();

            if (usableLines.Count == 0)
            {
                return new List<DxfPolylineEntity>();
            }

            var nodes = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
            var edges = new List<GraphEdge>();

            foreach (var line in usableLines)
            {
                var startNode = GetOrCreateNode(nodes, line.Start);
                var endNode = GetOrCreateNode(nodes, line.End);
                if (startNode.Key == endNode.Key)
                {
                    continue;
                }

                var edge = new GraphEdge
                {
                    Start = startNode,
                    End = endNode,
                    Layer = line.Layer
                };

                startNode.Edges.Add(edge);
                endNode.Edges.Add(edge);
                edges.Add(edge);
            }

            var polylines = new List<DxfPolylineEntity>();

            foreach (var node in nodes.Values.Where(node => node.UnvisitedDegree > 0 && node.UnvisitedDegree != 2))
            {
                while (node.UnvisitedDegree > 0)
                {
                    var polyline = WalkPath(node, node.GetFirstUnvisitedEdge());
                    if (polyline != null)
                    {
                        polylines.Add(polyline);
                    }
                }
            }

            foreach (var edge in edges.Where(edge => !edge.Visited))
            {
                var polyline = WalkPath(edge.Start, edge);
                if (polyline != null)
                {
                    polylines.Add(polyline);
                }
            }

            return polylines;
        }

        private static DxfPolylineEntity WalkPath(GraphNode startNode, GraphEdge startEdge)
        {
            if (startEdge == null || startEdge.Visited)
            {
                return null;
            }

            var points = new List<DxfPoint> { ClonePoint(startNode.Point) };
            var currentNode = startNode;
            var currentEdge = startEdge;
            string layer = null;

            while (currentEdge != null && !currentEdge.Visited)
            {
                currentEdge.Visited = true;
                layer ??= currentEdge.Layer;

                var nextNode = currentEdge.GetOther(currentNode);
                if (nextNode == null)
                {
                    break;
                }

                if (!AreSamePoint(points[points.Count - 1], nextNode.Point))
                {
                    points.Add(ClonePoint(nextNode.Point));
                }

                currentNode = nextNode;

                if (currentNode.UnvisitedDegree == 0)
                {
                    break;
                }

                if (currentNode.UnvisitedDegree > 1)
                {
                    break;
                }

                currentEdge = currentNode.GetFirstUnvisitedEdge();
            }

            points = NormalizePolylineVertices(points, false);
            if (points.Count < 2)
            {
                return null;
            }

            return new DxfPolylineEntity().WithLayer(layer).WithClosed(false).WithVertices(points);
        }

        private static GraphNode GetOrCreateNode(Dictionary<string, GraphNode> nodes, DxfPoint point)
        {
            var key = CreatePointKey(point);
            if (!nodes.TryGetValue(key, out var node))
            {
                node = new GraphNode
                {
                    Key = key,
                    Point = new DxfPoint
                    {
                        X = Math.Round(point.X / PointTolerance) * PointTolerance,
                        Y = Math.Round(point.Y / PointTolerance) * PointTolerance
                    }
                };

                nodes[key] = node;
            }

            return node;
        }

        private static List<DxfPoint> NormalizePolylineVertices(List<DxfPoint> vertices, bool closed)
        {
            var normalized = new List<DxfPoint>();
            foreach (var vertex in vertices)
            {
                if (normalized.Count == 0 || !AreSamePoint(normalized[normalized.Count - 1], vertex))
                {
                    normalized.Add(ClonePoint(vertex));
                }
            }

            if (closed && normalized.Count > 2 && !AreSamePoint(normalized[0], normalized[normalized.Count - 1]))
            {
                normalized.Add(ClonePoint(normalized[0]));
            }

            return normalized;
        }

        private static bool ShouldInclude(RawImportedShape rawShape)
        {
            if (rawShape == null)
            {
                return false;
            }

            if (rawShape.Shape.ShapeType?.Key == LocationDiagramShape.ShapeType_Circle)
            {
                return rawShape.Shape.Width > MinimumDrawableWidthOrHeight || rawShape.Shape.Height > MinimumDrawableWidthOrHeight;
            }

            if (rawShape.Points == null || rawShape.Points.Count < 2)
            {
                return false;
            }

            return HasDrawableGeometry(rawShape.Points);
        }

        private static bool HasDrawableGeometry(List<DxfPoint> points)
        {
            if (points == null || points.Count < 2)
            {
                return false;
            }

            for (var index = 1; index < points.Count; index++)
            {
                if (!AreSamePoint(points[index - 1], points[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldKeepLine(DxfLineEntity line)
        {
            Console.WriteLine(line.Start.X + " - " + line.Start.Y);

            return line != null &&
                   line.Start != null &&
                   line.End != null &&
                   (line.Start.X <= MaxImportX || line.End.X <= MaxImportX);
        }

        private static bool ShouldKeepShape(RawImportedShape rawShape)
        {
            var points = GetWorldPoints(rawShape);
            return points.Any(point => point.X <= MaxImportX);
        }

        private static void NormalizeArtifacts(List<LayerArtifacts> layerArtifacts, LocationDiagram diagram, DxfImportOptions options)
        {
            var allPoints = new List<DxfPoint>();
            foreach (var artifact in layerArtifacts)
            {
                foreach (var polyline in artifact.WorldPolylines)
                {
                    allPoints.AddRange(polyline.Select(ClonePoint));
                }

                foreach (var shape in artifact.Shapes)
                {
                    allPoints.AddRange(GetWorldPoints(shape));
                }
            }

            if (allPoints.Count == 0)
            {
                return;
            }

            var contentMinX = allPoints.Min(point => point.X);
            var contentMinY = allPoints.Min(point => point.Y);
            var contentMaxX = allPoints.Max(point => point.X);
            var contentMaxY = allPoints.Max(point => point.Y);

            var contentWidth = Math.Max(1d, contentMaxX - contentMinX);
            var contentHeight = Math.Max(1d, contentMaxY - contentMinY);

            var marginX = contentWidth * ContentMarginPercent;
            var marginY = contentHeight * ContentMarginPercent;

            var cropMinX = contentMinX - marginX;
            var cropMinY = contentMinY - marginY;
            var cropMaxX = contentMaxX + marginX;
            var cropMaxY = contentMaxY + marginY;

            foreach (var artifact in layerArtifacts)
            {
                foreach (var worldPolyline in artifact.WorldPolylines)
                {
                    var transformed = worldPolyline.Select(point => new ShapePoint
                    {
                        X = point.X - cropMinX,
                        Y = options.FlipY ? (cropMaxY - point.Y) : (point.Y - cropMinY)
                    }).ToList();

                    if (transformed.Count >= 2)
                    {
                        artifact.Layer.Polylines.Add(transformed);
                    }
                }

                foreach (var rawShape in artifact.Shapes)
                {
                    NormalizeShape(rawShape, cropMinX, cropMinY, cropMaxY, options);
                }
            }

            var normalizedPoints = new List<DxfPoint>();
            foreach (var artifact in layerArtifacts)
            {
                foreach (var polyline in artifact.Layer.Polylines)
                {
                    normalizedPoints.AddRange(polyline.Select(point => new DxfPoint { X = point.X, Y = point.Y }));
                }

                foreach (var shape in artifact.Layer.Shapes)
                {
                    if (shape.ShapeType?.Key == LocationDiagramShape.ShapeType_Circle)
                    {
                        normalizedPoints.Add(new DxfPoint { X = shape.X, Y = shape.Y });
                        normalizedPoints.Add(new DxfPoint { X = shape.X + shape.Width, Y = shape.Y + shape.Height });
                    }
                    else
                    {
                        foreach (var point in shape.Points)
                        {
                            normalizedPoints.Add(new DxfPoint { X = shape.X + point.X, Y = shape.Y + point.Y });
                        }
                    }
                }
            }

            if (normalizedPoints.Count == 0)
            {
                return;
            }

            var diagramMinX = normalizedPoints.Min(point => point.X);
            var diagramMinY = normalizedPoints.Min(point => point.Y);
            var diagramMaxX = normalizedPoints.Max(point => point.X);
            var diagramMaxY = normalizedPoints.Max(point => point.Y);

            diagram.ViewPortX = diagramMinX;
            diagram.ViewPortY = diagramMinY;
            diagram.Width = Math.Max(0, diagramMaxX - diagramMinX);
            diagram.Height = Math.Max(0, diagramMaxY - diagramMinY);
            diagram.Scale = 1;
            diagram.Rotation = 0;
            diagram.DefaultShapeZoomLevel = 1;
        }

        private static void NormalizeShape(RawImportedShape rawShape, double cropMinX, double cropMinY, double cropMaxY, DxfImportOptions options)
        {
            if (rawShape.Shape.ShapeType?.Key == LocationDiagramShape.ShapeType_Circle)
            {
                var normalizedX = rawShape.Shape.X - cropMinX;
                var normalizedY = options.FlipY ? (cropMaxY - (rawShape.Shape.Y + rawShape.Shape.Height)) : (rawShape.Shape.Y - cropMinY);
                rawShape.Shape.X = normalizedX;
                rawShape.Shape.Y = normalizedY;
                rawShape.Shape.Points = new List<ShapePoint>();
                return;
            }

            var transformed = rawShape.Points.Select(point => new DxfPoint
            {
                X = point.X - cropMinX,
                Y = options.FlipY ? (cropMaxY - point.Y) : (point.Y - cropMinY)
            }).ToList();

            if (transformed.Count < 2)
            {
                rawShape.Shape.Points = new List<ShapePoint>();
                return;
            }

            var anchor = transformed[0];
            var shapeMinX = transformed.Min(point => point.X);
            var shapeMinY = transformed.Min(point => point.Y);
            var shapeMaxX = transformed.Max(point => point.X);
            var shapeMaxY = transformed.Max(point => point.Y);

            rawShape.Shape.X = anchor.X;
            rawShape.Shape.Y = anchor.Y;
            rawShape.Shape.Width = shapeMaxX - shapeMinX;
            rawShape.Shape.Height = shapeMaxY - shapeMinY;
            rawShape.Shape.Points = transformed.Select(point => new ShapePoint { X = point.X - anchor.X, Y = point.Y - anchor.Y }).ToList();
        }

        private static List<DxfPoint> GetWorldPoints(RawImportedShape shape)
        {
            if (shape.Shape.ShapeType?.Key == LocationDiagramShape.ShapeType_Circle)
            {
                return new List<DxfPoint>
                {
                    new DxfPoint { X = shape.Shape.X, Y = shape.Shape.Y },
                    new DxfPoint { X = shape.Shape.X + shape.Shape.Width, Y = shape.Shape.Y + shape.Shape.Height }
                };
            }

            return shape.Points.Select(ClonePoint).ToList();
        }

        private static bool AreSamePoint(DxfPoint first, DxfPoint second)
        {
            return Math.Abs(first.X - second.X) <= PointTolerance && Math.Abs(first.Y - second.Y) <= PointTolerance;
        }

        private static string CreatePointKey(DxfPoint point)
        {
            var x = Math.Round(point.X / PointTolerance);
            var y = Math.Round(point.Y / PointTolerance);
            return $"{x}:{y}";
        }

        private static DxfPoint ClonePoint(DxfPoint point)
        {
            return new DxfPoint { X = point.X, Y = point.Y };
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "importedshape";
            }

            var builder = new StringBuilder();
            foreach (var character in value.Trim().ToLowerInvariant())
            {
                if ((character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'))
                {
                    builder.Append(character);
                }
            }

            var result = builder.ToString();
            return string.IsNullOrWhiteSpace(result) ? "importedshape" : result;
        }

        private sealed class RawImportedShape
        {
            public LocationDiagramShape Shape { get; set; }
            public List<DxfPoint> Points { get; set; }
        }

        private sealed class LayerArtifacts
        {
            public LocationDiagramLayer Layer { get; set; }
            public List<List<DxfPoint>> WorldPolylines { get; } = new List<List<DxfPoint>>();
            public List<RawImportedShape> Shapes { get; } = new List<RawImportedShape>();
        }

        private sealed class GraphNode
        {
            public string Key { get; set; }
            public DxfPoint Point { get; set; }
            public List<GraphEdge> Edges { get; } = new List<GraphEdge>();
            public int UnvisitedDegree => Edges.Count(edge => !edge.Visited);

            public GraphEdge GetFirstUnvisitedEdge()
            {
                return Edges.FirstOrDefault(edge => !edge.Visited);
            }
        }

        private sealed class GraphEdge
        {
            public GraphNode Start { get; set; }
            public GraphNode End { get; set; }
            public string Layer { get; set; }
            public bool Visited { get; set; }

            public GraphNode GetOther(GraphNode node)
            {
                if (node == null)
                {
                    return null;
                }

                if (ReferenceEquals(node, Start))
                {
                    return End;
                }

                if (ReferenceEquals(node, End))
                {
                    return Start;
                }

                return null;
            }
        }
    }

    internal static class DxfPolylineEntityExtensions
    {
        public static DxfPolylineEntity WithVertices(this DxfPolylineEntity entity, List<DxfPoint> points)
        {
            foreach (var point in points)
            {
                entity.Vertices.Add(point);
            }

            return entity;
        }

        public static DxfPolylineEntity WithLayer(this DxfPolylineEntity entity, string layer)
        {
            entity.Layer = layer;
            return entity;
        }

        public static DxfPolylineEntity WithClosed(this DxfPolylineEntity entity, bool closed)
        {
            entity.Closed = closed;
            return entity;
        }
    }
}