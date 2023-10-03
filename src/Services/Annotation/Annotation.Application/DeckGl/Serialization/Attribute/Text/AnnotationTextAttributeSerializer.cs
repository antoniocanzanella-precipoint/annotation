using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Text;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Text;

public class AnnotationTextAttributeSerializer : IAttributeSerializer
{
    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetText, out AttributeHeaderDto attrHeaderDto);

        var written = 0;
        Encoding encoding = GetEncodingFromPrimDataType(attrHeaderDto.DataType);

        foreach (AnnotationShape annota in layer.Data)
        {
            Span<byte> buf = target.Slice(written);
            written += encoding.GetBytes(annota.Label, buf);
        }

        return written;
    }

    private Encoding GetEncodingFromPrimDataType(PrimitiveDataType primitiveDataType)
    {
        if (primitiveDataType == PrimitiveDataType.UInt8 || primitiveDataType == PrimitiveDataType.UInt16)
        {
            return Encoding.UTF8;
        }

        if (primitiveDataType == PrimitiveDataType.UInt32)
        {
            return Encoding.UTF32;
        }

        throw new ArgumentException($"Cannot get encoding from primitive datatype {primitiveDataType}",
            nameof(primitiveDataType));
    }
}