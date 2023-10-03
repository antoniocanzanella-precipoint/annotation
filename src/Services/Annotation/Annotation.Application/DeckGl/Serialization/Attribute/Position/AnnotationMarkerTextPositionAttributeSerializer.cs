using NetTopologySuite.Geometries;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Position;

public class AnnotationMarkerTextPositionAttributeSerializer : IAttributeSerializer
{
    private readonly IByteSerializer _serializer;

    public AnnotationMarkerTextPositionAttributeSerializer(IByteSerializer serializer)
    {
        _serializer = serializer;
    }

    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetPosition, out AttributeHeaderDto attrHeaderDto);

        var written = 0;
        for (var index = 0; index < layer.Data.Count; index++)
        {
            AnnotationShape annota = layer.Data[index];
            Span<byte> buf = target.Slice(written);
            Coordinate cords = annota.Shape.Coordinates[1];
            int count = annota.Label.EnumerateRunes().Count();
            var coordsOffset = 0;
            for (var i = 0; i < count; i++)
            {
                coordsOffset += AnnotationAttributeSerializerHelper.SerializeCoordinates(attrHeaderDto, cords,
                    _serializer, buf.Slice(coordsOffset));
            }

            written += coordsOffset;
        }

        return written;
    }
}