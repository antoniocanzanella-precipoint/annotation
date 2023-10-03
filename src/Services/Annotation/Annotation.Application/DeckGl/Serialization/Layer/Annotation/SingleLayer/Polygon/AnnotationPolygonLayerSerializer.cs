using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Polygon;

public class AnnotationPolygonLayerSerializer : ADeckGlAnnotationSingleLayerSerializer,
                                                IAnnotationPolygonLayerSerializer
{
    private readonly IAttributeSerializer _colorSerializer;
    private readonly IAttributeSerializer _polygonSerializer;
    private readonly IAttributeSerializer _widthSerializer;

    public AnnotationPolygonLayerSerializer(IByteSerializer serializer, IAttributeSerializer polygonSerializer, IAttributeSerializer colorSerializer,
        IAttributeSerializer widthSerializer) :
        base(serializer)
    {
        _polygonSerializer = polygonSerializer;
        _colorSerializer = colorSerializer;
        _widthSerializer = widthSerializer;
    }

    public int SerializePolygonLayer(LayerHeaderDto layerHeaderDto, DeckGlPolygonLayer<AnnotationShape> layer,
        Span<byte> memory)
    {
        return SerializeLayer(layerHeaderDto, layer, memory);
    }

    protected override int SerializePolygon(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _polygonSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }

    protected override int SerializeWidth(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _widthSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }

    protected override int SerializeColor(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _colorSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }
}