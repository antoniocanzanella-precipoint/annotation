using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Scatterplot;

public class AnnotationScatterplotLayerSerializer : ADeckGlAnnotationSingleLayerSerializer,
                                                    IScatterplotLayerSerializer
{
    private readonly IAttributeSerializer _fillColorSerializer;
    private readonly IAttributeSerializer _lineColorSerializer;
    private readonly IAttributeSerializer _positonSerializer;
    private readonly IAttributeSerializer _radiusSerializer;

    public AnnotationScatterplotLayerSerializer(IByteSerializer serializer, IAttributeSerializer positonSerializer,
        IAttributeSerializer radiusSerializer, IAttributeSerializer fillColorSerializer,
        IAttributeSerializer lineColorSerializer) : base(serializer)
    {
        _positonSerializer = positonSerializer;
        _radiusSerializer = radiusSerializer;
        _fillColorSerializer = fillColorSerializer;
        _lineColorSerializer = lineColorSerializer;
    }

    public int SerializePathLayer(LayerHeaderDto header, DeckGlScatterplotLayer<AnnotationShape> layer,
        Span<byte> memory)
    {
        return SerializeLayer(header, layer, memory);
    }

    protected override int SerializePosition(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _positonSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }

    protected override int SerializeRadius(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _radiusSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }

    protected override int SerializeFillColor(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _fillColorSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }

    protected override int SerializeLineColor(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _lineColorSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }
}