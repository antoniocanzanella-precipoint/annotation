using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Polygon;

public interface IAnnotationPolygonLayerSerializer
{
    int SerializePolygonLayer(LayerHeaderDto layerHeaderDto, DeckGlPolygonLayer<AnnotationShape> layer, Span<byte> memory);
}