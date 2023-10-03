using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Polygon;

public class AnnotationPolygonAttributeSerializer : IAttributeSerializer
{
    private readonly IByteSerializer _serializer;

    public AnnotationPolygonAttributeSerializer(IByteSerializer serializer)
    {
        _serializer = serializer;
    }

    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetPolygon, out AttributeHeaderDto attribute);

        return AnnotationAttributeSerializerHelper.SerializeLayerCoordinates(layer, attribute, _serializer, target);
    }
}