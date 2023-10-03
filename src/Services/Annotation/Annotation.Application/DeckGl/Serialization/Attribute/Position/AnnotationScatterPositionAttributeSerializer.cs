using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Position;

public class AnnotationScatterPositionAttributeSerializer : IAttributeSerializer
{
    private readonly IByteSerializer _serializer;

    public AnnotationScatterPositionAttributeSerializer(IByteSerializer serializer)
    {
        _serializer = serializer;
    }

    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetPosition, out AttributeHeaderDto attribute);

        var written = 0;
        for (var index = 0; index < layer.Data.Count; index++)
        {
            AnnotationShape annota = layer.Data[index];
            Span<byte> buf = target.Slice(written);
            written += AnnotationAttributeSerializerHelper.SerializeCoordinates(attribute, annota.Shape.Coordinates[0],
                _serializer, buf);
        }

        return written;
    }
}