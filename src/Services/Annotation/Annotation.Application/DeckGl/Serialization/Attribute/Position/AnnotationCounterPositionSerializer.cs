using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Position;

public class AnnotationCounterPositionSerializer : IAttributeSerializer
{
    private readonly IByteSerializer _serializer;

    public AnnotationCounterPositionSerializer(IByteSerializer serializer)
    {
        _serializer = serializer;
    }

    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetPosition, out AttributeHeaderDto attrHeader);

        var written = 0;
        foreach (AnnotationShape annota in layer.Data)
        {
            foreach (CounterGroup group in annota.CounterGroups)
            {
                foreach (Counter counter in group.Counters)
                {
                    Span<byte> buf = target.Slice(written);
                    written += AnnotationAttributeSerializerHelper.SerializeCoordinates(attrHeader, counter.Shape.Coordinates[0], _serializer, buf);
                }
            }
        }

        return written;
    }
}