using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Services;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Services
{
    public class DxfLocationDiagramConverter : IDxfLocationDiagramConverter
    {
        public Task<InvokeResult<LocationDiagram>> ConvertAsync(Stream stream, DxfImportOptions options = null, CancellationToken cancellationToken = default)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                var dxfContents = reader.ReadToEnd();
                return ConvertAsync(dxfContents, options, cancellationToken);
            }
        }

        public Task<InvokeResult<LocationDiagram>> ConvertAsync(string dxfContents, DxfImportOptions options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dxfContents))
            {
                throw new ArgumentNullException(nameof(dxfContents));
            }

            options = options ?? new DxfImportOptions();

            try
            {
                var document = ParseDocument(dxfContents, options);
                var entityCount = document.Layers.Sum(layer => layer.Entities.Count);
                if (entityCount == 0)
                {
                    return Task.FromResult(InvokeResult<LocationDiagram>.FromError("The DXF file did not contain any supported entities to import."));
                }

                var diagram = DxfLocationDiagramMapper.Map(document, options);
                return Task.FromResult(InvokeResult<LocationDiagram>.Create(diagram));
            }
            catch (Exception ex)
            {
                return Task.FromResult(InvokeResult<LocationDiagram>.FromException("Could not convert DXF to LocationDiagram.", ex));
            }
        }

        private static DxfIntermediateDocument ParseDocument(string dxfContents, DxfImportOptions options)
        {
            var pairs = ReadPairs(dxfContents);
            var entities = ParseEntities(pairs, options);
            return BuildIntermediateDocument(entities, options);
        }

        private static List<DxfIntermediateEntity> ParseEntities(List<DxfPair> pairs, DxfImportOptions options)
        {
            var entities = new List<DxfIntermediateEntity>();
            var inEntitiesSection = false;
            var sectionPending = false;
            DxfPolylineEntity activePolyline = null;
            string currentEntityType = null;
            bool polylineSequenceMode = false;

            for (var index = 0; index < pairs.Count; index++)
            {
                var pair = pairs[index];

                if (pair.Code == 0 && pair.Value == "SECTION")
                {
                    sectionPending = true;
                    continue;
                }

                if (sectionPending && pair.Code == 2)
                {
                    inEntitiesSection = string.Equals(pair.Value, "ENTITIES", StringComparison.OrdinalIgnoreCase);
                    sectionPending = false;
                    continue;
                }

                if (pair.Code == 0 && pair.Value == "ENDSEC")
                {
                    inEntitiesSection = false;
                    activePolyline = null;
                    currentEntityType = null;
                    polylineSequenceMode = false;
                    continue;
                }

                if (!inEntitiesSection)
                {
                    continue;
                }

                if (pair.Code == 0)
                {
                    currentEntityType = pair.Value;

                    if (string.Equals(currentEntityType, "LWPOLYLINE", StringComparison.OrdinalIgnoreCase))
                    {
                        var entity = ParseLightweightPolyline(pairs, ref index);
                        if (entity.Closed || !options.IgnoreOpenPolylines)
                        {
                            entities.Add(entity);
                        }

                        currentEntityType = null;
                        continue;
                    }

                    if (string.Equals(currentEntityType, "CIRCLE", StringComparison.OrdinalIgnoreCase))
                    {
                        var entity = ParseCircle(pairs, ref index);
                        entities.Add(entity);
                        currentEntityType = null;
                        continue;
                    }

                    if (string.Equals(currentEntityType, "LINE", StringComparison.OrdinalIgnoreCase))
                    {
                        var entity = ParseLine(pairs, ref index);
                        if (entity.Start != null && entity.End != null)
                        {
                            entities.Add(entity);
                        }

                        currentEntityType = null;
                        continue;
                    }

                    if (string.Equals(currentEntityType, "POLYLINE", StringComparison.OrdinalIgnoreCase))
                    {
                        activePolyline = new DxfPolylineEntity();
                        polylineSequenceMode = true;
                        continue;
                    }

                    if (polylineSequenceMode && string.Equals(currentEntityType, "VERTEX", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseVertex(pairs, ref index, activePolyline);
                        continue;
                    }

                    if (polylineSequenceMode && string.Equals(currentEntityType, "SEQEND", StringComparison.OrdinalIgnoreCase))
                    {
                        if (activePolyline != null && activePolyline.Vertices.Count > 0 && (activePolyline.Closed || !options.IgnoreOpenPolylines))
                        {
                            entities.Add(activePolyline);
                        }

                        activePolyline = null;
                        polylineSequenceMode = false;
                        continue;
                    }
                }

                if (polylineSequenceMode && activePolyline != null)
                {
                    switch (pair.Code)
                    {
                        case 8:
                            activePolyline.Layer = pair.Value;
                            break;
                        case 70:
                            activePolyline.Closed = (ParseInt(pair.Value) & 1) == 1;
                            break;
                    }
                }
            }

            if (activePolyline != null && activePolyline.Vertices.Count > 0 && (activePolyline.Closed || !options.IgnoreOpenPolylines))
            {
                entities.Add(activePolyline);
            }

            return entities;
        }

        private static DxfIntermediateDocument BuildIntermediateDocument(List<DxfIntermediateEntity> entities, DxfImportOptions options)
        {
            var document = new DxfIntermediateDocument();
            var layerLookup = new Dictionary<string, DxfIntermediateLayer>(StringComparer.OrdinalIgnoreCase);

            foreach (var entity in entities)
            {
                var layerName = string.IsNullOrWhiteSpace(entity.Layer) ? options.LayerNamePrefix : entity.Layer.Trim();
                if (!layerLookup.TryGetValue(layerName, out var layer))
                {
                    layer = new DxfIntermediateLayer { Name = layerName };
                    layerLookup[layerName] = layer;
                    document.Layers.Add(layer);
                }

                entity.Layer = layerName;
                layer.Entities.Add(entity);
            }

            return document;
        }

        private static DxfPolylineEntity ParseLightweightPolyline(List<DxfPair> pairs, ref int index)
        {
            var entity = new DxfPolylineEntity();
            double? pendingX = null;

            for (var i = index + 1; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                if (pair.Code == 0)
                {
                    index = i - 1;
                    return entity;
                }

                switch (pair.Code)
                {
                    case 8:
                        entity.Layer = pair.Value;
                        break;
                    case 10:
                        pendingX = ParseDouble(pair.Value);
                        break;
                    case 20:
                        if (pendingX.HasValue)
                        {
                            entity.Vertices.Add(new DxfPoint { X = pendingX.Value, Y = ParseDouble(pair.Value) });
                            pendingX = null;
                        }
                        break;
                    case 70:
                        entity.Closed = (ParseInt(pair.Value) & 1) == 1;
                        break;
                }
            }

            index = pairs.Count - 1;
            return entity;
        }

        private static DxfCircleEntity ParseCircle(List<DxfPair> pairs, ref int index)
        {
            var entity = new DxfCircleEntity();

            for (var i = index + 1; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                if (pair.Code == 0)
                {
                    index = i - 1;
                    return entity;
                }

                switch (pair.Code)
                {
                    case 8:
                        entity.Layer = pair.Value;
                        break;
                    case 10:
                        entity.CenterX = ParseDouble(pair.Value);
                        break;
                    case 20:
                        entity.CenterY = ParseDouble(pair.Value);
                        break;
                    case 40:
                        entity.Radius = ParseDouble(pair.Value);
                        break;
                }
            }

            index = pairs.Count - 1;
            return entity;
        }

        private static DxfLineEntity ParseLine(List<DxfPair> pairs, ref int index)
        {
            var entity = new DxfLineEntity();
            double? x1 = null;
            double? y1 = null;
            double? x2 = null;
            double? y2 = null;

            for (var i = index + 1; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                if (pair.Code == 0)
                {
                    if (x1.HasValue && y1.HasValue && x2.HasValue && y2.HasValue)
                    {
                        entity.Start = new DxfPoint { X = x1.Value, Y = y1.Value };
                        entity.End = new DxfPoint { X = x2.Value, Y = y2.Value };
                    }

                    index = i - 1;
                    return entity;
                }

                switch (pair.Code)
                {
                    case 8:
                        entity.Layer = pair.Value;
                        break;
                    case 10:
                        x1 = ParseDouble(pair.Value);
                        break;
                    case 20:
                        y1 = ParseDouble(pair.Value);
                        break;
                    case 11:
                        x2 = ParseDouble(pair.Value);
                        break;
                    case 21:
                        y2 = ParseDouble(pair.Value);
                        break;
                }
            }

            if (x1.HasValue && y1.HasValue && x2.HasValue && y2.HasValue)
            {
                entity.Start = new DxfPoint { X = x1.Value, Y = y1.Value };
                entity.End = new DxfPoint { X = x2.Value, Y = y2.Value };
            }

            index = pairs.Count - 1;
            return entity;
        }

        private static void ParseVertex(List<DxfPair> pairs, ref int index, DxfPolylineEntity polyline)
        {
            double? x = null;
            double? y = null;

            for (var i = index + 1; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                if (pair.Code == 0)
                {
                    if (x.HasValue && y.HasValue)
                    {
                        polyline.Vertices.Add(new DxfPoint { X = x.Value, Y = y.Value });
                    }

                    index = i - 1;
                    return;
                }

                switch (pair.Code)
                {
                    case 10:
                        x = ParseDouble(pair.Value);
                        break;
                    case 20:
                        y = ParseDouble(pair.Value);
                        break;
                }
            }

            if (x.HasValue && y.HasValue)
            {
                polyline.Vertices.Add(new DxfPoint { X = x.Value, Y = y.Value });
            }

            index = pairs.Count - 1;
        }

        private static List<DxfPair> ReadPairs(string dxfContents)
        {
            var pairs = new List<DxfPair>();
            using (var reader = new StringReader(dxfContents))
            {
                while (true)
                {
                    var codeLine = reader.ReadLine();
                    if (codeLine == null)
                    {
                        break;
                    }

                    var valueLine = reader.ReadLine();
                    if (valueLine == null)
                    {
                        break;
                    }

                    if (!int.TryParse(codeLine.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
                    {
                        throw new InvalidDataException($"Invalid DXF group code '{codeLine}'.");
                    }

                    pairs.Add(new DxfPair
                    {
                        Code = code,
                        Value = valueLine.Trim()
                    });
                }
            }

            return pairs;
        }

        private static double ParseDouble(string value)
        {
            return double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        }

        private static int ParseInt(string value)
        {
            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
    }
}
