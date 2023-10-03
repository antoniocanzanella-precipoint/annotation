using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Text;

public class AnnotationTextLayerSerializer : ADeckGlAnnotationSingleLayerSerializer,
                                             IAnnotationTextLayerSerializer
{
    private readonly IAttributeSerializer _colorSerializer;
    private readonly IAttributeSerializer _positonSerializer;
    private readonly IAttributeSerializer _textSerializer;

    public AnnotationTextLayerSerializer(IByteSerializer serializer, IAttributeSerializer textSerializer,
        IAttributeSerializer positonSerializer, IAttributeSerializer colorSerializer) : base(serializer)
    {
        _textSerializer = textSerializer;
        _positonSerializer = positonSerializer;
        _colorSerializer = colorSerializer;
    }

    public int SerializeTextLayer(LayerHeaderDto header, DeckGlTextLayer<AnnotationShape> layer, Span<byte> memory)
    {
        return SerializeLayer(header, layer, memory);
    }

    protected override int SerializeText(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _textSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }

    protected override int SerializeColor(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _colorSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }

    protected override int SerializePosition(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer)
    {
        return _positonSerializer.SerializeAttribute(layer, layerHeaderDto, buffer);
    }
}