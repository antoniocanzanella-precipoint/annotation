using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Radius;

public class AnnotationRadiusAttributeSerializer : IAttributeSerializer
{
    private readonly IByteSerializer _serializer;

    public AnnotationRadiusAttributeSerializer(IByteSerializer serializer)
    {
        _serializer = serializer;
    }

    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetRadius, out AttributeHeaderDto attrHeaderDto);

        var written = 0;
        for (var index = 0; index < layer.Data.Count; index++)
        {
            AnnotationShape annota = layer.Data[index];
            double radius = annota.GetRadius();
            Span<byte> buf = target.Slice(written);
            if (attrHeaderDto.DataType == PrimitiveDataType.Double)
            {
                written += _serializer.Serialize(radius, buf);
            }
            else if (attrHeaderDto.DataType == PrimitiveDataType.Float)
            {
                written += _serializer.Serialize((float) radius, buf);
            }
        }

        return written;
    }
}