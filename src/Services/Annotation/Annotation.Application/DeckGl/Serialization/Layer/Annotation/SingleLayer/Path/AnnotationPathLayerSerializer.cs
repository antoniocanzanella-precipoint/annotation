using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Path;

public class AnnotationPathLayerSerializer : ADeckGlAnnotationSingleLayerSerializer,
                                             IAnnotationPathLayerSerializer
{
    private readonly IAttributeSerializer _colorSerializer;
    private readonly IAttributeSerializer _pathSerializer;
    private readonly IAttributeSerializer _widthSerializer;

    public AnnotationPathLayerSerializer(IByteSerializer serializer, IAttributeSerializer colorSerializer,
        IAttributeSerializer widthSerializer, IAttributeSerializer pathSerializer) : base(serializer)
    {
        _colorSerializer = colorSerializer;
        _widthSerializer = widthSerializer;
        _pathSerializer = pathSerializer;
    }

    public int SerializePathLayer(LayerHeaderDto header, DeckGlPathLayer<AnnotationShape> layer, Span<byte> memory)
    {
        return SerializeLayer(header, layer, memory);
    }

    protected override int SerializePath(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _pathSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
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