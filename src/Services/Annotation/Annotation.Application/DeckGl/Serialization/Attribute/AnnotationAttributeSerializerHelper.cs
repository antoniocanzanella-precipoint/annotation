using NetTopologySuite.Geometries;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute;

public static class AnnotationAttributeSerializerHelper
{
    public static int SerializeLayerCoordinates(DeckGlLayer<AnnotationShape> layer, AttributeHeaderDto attributeHeader,
        IByteSerializer serializer,
        Span<byte> target)
    {
        var written = 0;
        for (var index = 0; index < layer.Data.Count; index++)
        {
            AnnotationShape annota = layer.Data[index];
            Coordinate[] cords = annota.Shape.Coordinates;
            int len = cords.Length;
            for (var i = 0; i < len; i++)
            {
                Coordinate c = cords[i];
                written += SerializeCoordinates(attributeHeader, c, serializer, target.Slice(written));
            }
        }

        return written;
    }

    public static int SerializeCoordinates(AttributeHeaderDto header, Coordinate c, IByteSerializer serializer,
        Span<byte> target)
    {
        var written = 0;
        if (header.DataType == PrimitiveDataType.Double)
        {
            written += serializer.Serialize(c[0], target);
        }
        else if (header.DataType == PrimitiveDataType.Float)
        {
            written += serializer.Serialize((float) c.X, target);
            written += serializer.Serialize((float) c.Y, target.Slice(written));
        }

        return written;
    }

    public static int SerializeColor(AttributeHeaderDto attributeHeader,
        DeckGlLayer<AnnotationShape> layer, Span<byte> buffer, Func<AnnotationShape, int> vertexCountFn,
        Span<byte> backupColor, float opacity = 1)
    {
        Span<byte> colorBuffer = new byte[attributeHeader.SizeOfDataType * attributeHeader.Size];
        var written = 0;
        for (var index = 0; index < layer.Data.Count; index++)
        {
            AnnotationShape annota = layer.Data[index];
            Span<byte> buf = buffer.Slice(written);
            int vertexCount = vertexCountFn(annota);
            if (annota.Color is not null)
            {
                colorBuffer[0] = (byte) annota.Color[0];
                colorBuffer[1] = (byte) annota.Color[1];
                colorBuffer[2] = (byte) annota.Color[2];
                if (annota.Color.Length == 4)
                {
                    colorBuffer[3] = (byte) Math.Round(annota.Color[3] * opacity);
                }
                else
                {
                    colorBuffer[3] = (byte) Math.Round(255 * opacity);
                }

                written += CopySequenceNTimesToBuffer(buf, colorBuffer, vertexCount);
            }
            else
            {
                written += CopySequenceNTimesToBuffer(buf, backupColor, vertexCount);
            }
        }

        return written;
    }

    public static int CopySequenceNTimesToBuffer(Span<byte> buffer, Span<byte> toCopy, int n)
    {
        var written = 0;

        for (var index = 0; index < n; index++)
        {
            Span<byte> buf = buffer.Slice(written);
            toCopy.CopyTo(buf);
            written += toCopy.Length;
        }

        return written;
    }
}