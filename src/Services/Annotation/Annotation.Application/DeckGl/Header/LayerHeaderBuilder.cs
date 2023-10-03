using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Custom;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Helper;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Header;

public class LayerHeaderBuilder
{
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;

    public LayerHeaderBuilder(IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public BuildResult Build(IReadOnlyList<AnnotationShape> annotationList)
    {
        var deckGlLayers = new Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>>();
        var layerHeaders = new List<BaseLayerHeaderDto>();
        foreach (AnnotationShape annotationShape in annotationList)
        {
            MapAnnotationShapeToLayer(annotationShape, deckGlLayers);
        }

        var offset = 0;
        foreach ((DeckGlLayerId key, DeckGlLayer<AnnotationShape> value) in deckGlLayers)
        {
            offset = AlignmentHelper.AlignTo4ByteOffset(offset);
            BaseLayerHeaderDto headerDto = value.ToHeader(offset, _claimsPrincipalProvider);

            layerHeaders.Add(headerDto);
            offset += headerDto.TotalSizeInBytes;
        }

        Dictionary<string, DeckGlLayer<AnnotationShape>> resultDict = deckGlLayers.Values.ToDictionary(x => x.Id.ToString());

        return new BuildResult(layerHeaders, offset, resultDict);
    }

    private void MapAnnotationShapeToLayer(AnnotationShape annotationShape,
        Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>> layers)
    {
        DeckGlLayerType deckLayer = annotationShape.Type.ToDeckGlLayer();
        bool hasCounters = annotationShape.CounterGroups.Count > 0;
        if (hasCounters)
        {
            AddAnnotationToCounterLayer(annotationShape, layers);
        }

        switch (deckLayer)
        {
            case DeckGlLayerType.Scatterplot:
                AddAnnotationToScatterplotLayer(annotationShape, layers);
                break;
            case DeckGlLayerType.Path:
                AddAnnotationToPathLayer(annotationShape, layers);
                break;
            case DeckGlLayerType.Composite:
                AddAnnotationToCompositeLayer(annotationShape, layers);
                break;
            case DeckGlLayerType.Polygon:
                AddAnnotationToPolygonLayer(annotationShape, layers);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AddAnnotationToPolygonLayer(AnnotationShape shape,
        Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>> layers)
    {
        var layer = (DeckGlPolygonLayer<AnnotationShape>) GetLayerFromId(DeckGlLayerId.AnnotationsPolygonLayer, layers);

        layer.Data.Add(shape);
        layer.AppendIndex(shape.Shape.NumPoints);
    }

    private void AddAnnotationToCompositeLayer(AnnotationShape shape,
        Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>> layers)
    {
        AnnotationType type = shape.Type;

        if (type == AnnotationType.Marker)
        {
            AddAnnotationToMarkerLayer(shape, layers);
        }
    }

    private void AddAnnotationToCounterLayer(AnnotationShape shape,
        Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>> layers)
    {
        if (shape.CounterGroups.Count < 1)
        {
            return;
        }

        var layer = (DeckGlCounterLayer) GetLayerFromId(DeckGlLayerId.AnnotationsCounterLayer, layers);

        layer.Data.Add(shape);
    }

    private void AddAnnotationToScatterplotLayer(AnnotationShape shape,
        Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>> layers)
    {
        var layer = (DeckGlScatterplotLayer<AnnotationShape>) GetLayerFromId(DeckGlLayerId.AnnotationsCircleLayer,
            layers);

        layer.Data.Add(shape);
    }

    private void AddAnnotationToMarkerLayer(AnnotationShape shape,
        Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>> layers)
    {
        if (shape.Type != AnnotationType.Marker)
        {
            return;
        }

        var layer = (DeckGlMarkerLayer<AnnotationShape>) GetLayerFromId(DeckGlLayerId.AnnotationsMarkerLayer,
            layers);

        AddAnnotationToTextLayer(shape, layer.TextLayer);
        AddAnnotationToMarkerPathLayer(shape, layer.PathLayer);
        layer.Data.Add(shape);
    }

    private void AddAnnotationToTextLayer(AnnotationShape shape,
        DeckGlTextLayer<AnnotationShape> layer)
    {
        layer.Data.Add(shape);
        int count = shape.Label.EnumerateRunes().Count();
        layer.AppendIndex(count);
    }

    private void AddAnnotationToMarkerPathLayer(AnnotationShape shape, DeckGlPathLayer<AnnotationShape> layer)
    {
        layer.Data.Add(shape);
    }

    private void AddAnnotationToPathLayer(AnnotationShape shape,
        Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>> layers)
    {
        AnnotationType type = shape.Type;
        DeckGlLayerId id;
        if (type == AnnotationType.Polygon || type == AnnotationType.Rectangular)
        {
            id = DeckGlLayerId.AnnotationsPolygonLayer;
        }
        else
        {
            id = DeckGlLayerId.AnnotationsPolyLineLayer;
        }

        var layer = (DeckGlPathLayer<AnnotationShape>) GetLayerFromId(id, layers);


        layer.Data.Add(shape);
        layer.AppendIndex(shape.Shape.NumPoints);
    }

    private DeckGlLayer<AnnotationShape> GetLayerFromId(DeckGlLayerId id,
        Dictionary<DeckGlLayerId, DeckGlLayer<AnnotationShape>> deckGlLayers)
    {
        if (deckGlLayers.ContainsKey(id))
        {
            return deckGlLayers[id];
        }

        DeckGlLayer<AnnotationShape> layer = null;
        switch (id)
        {
            case DeckGlLayerId.AnnotationsCircleLayer:
                layer = new DeckGlScatterplotLayer<AnnotationShape>(id.ToString());
                break;
            case DeckGlLayerId.AnnotationsPolyLineLayer:
                layer = new DeckGlPathLayer<AnnotationShape>(id.ToString());
                break;
            case DeckGlLayerId.AnnotationsPolygonLayer:
                layer = new DeckGlPolygonLayer<AnnotationShape>(id.ToString());
                break;
            case DeckGlLayerId.AnnotationsMarkerLayer:
                layer = new DeckGlMarkerLayer<AnnotationShape>(id.ToString());
                break;
            case DeckGlLayerId.AnnotationsCounterLayer:
                layer = new DeckGlCounterLayer(id.ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }

        deckGlLayers[id] = layer;
        return layer;
    }
}