using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Counter;

public class AnnotationCounterLayerSerializer : ADeckGlAnnotationSingleLayerSerializer
{
    private readonly IAttributeSerializer _colorSerializer;
    private readonly IAttributeSerializer _positionSerializer;

    public AnnotationCounterLayerSerializer(IByteSerializer serializer, IAttributeSerializer positionSerializer,
        IAttributeSerializer colorSerializer) : base(serializer)
    {
        _positionSerializer = positionSerializer;
        _colorSerializer = colorSerializer;
    }

    protected override int SerializePosition(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _positionSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }

    protected override int SerializeColor(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _colorSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }
}