using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Path;

public class AnnotationPathAttributeSerializer : IAttributeSerializer
{
    private readonly IByteSerializer _serializer;

    public AnnotationPathAttributeSerializer(IByteSerializer serializer)
    {
        _serializer = serializer;
    }

    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetPath, out AttributeHeaderDto attribute);

        return AnnotationAttributeSerializerHelper.SerializeLayerCoordinates(layer, attribute, _serializer, target);
    }
}