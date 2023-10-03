using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer;

public abstract class ADeckGlAnnotationSingleLayerSerializer : IDeckGlAnnotationSingleLayerSerializer
{
    protected readonly IByteSerializer Serializer;

    public ADeckGlAnnotationSingleLayerSerializer(IByteSerializer serializer)
    {
        Serializer = serializer;
    }

    public int SerializeLayer(LayerHeaderDto header, DeckGlLayer<AnnotationShape> layer, Span<byte> memory)
    {
        var totalWritten = 0;
        var writtenBytes = new int[header.AttributeHeaders.Count];

        List<AttributeHeaderDto> headers = header.AttributeHeaders.Values.ToList();
        for (var index = 0; index < header.AttributeHeaders.Count; index++)
        {
            AttributeHeaderDto attrHeaderDto = headers[index];
            int written = Serialize(header, attrHeaderDto, layer, memory.Slice(attrHeaderDto.Offset,
                attrHeaderDto.TotalSizeInBytes));
            totalWritten += written;
            writtenBytes[index] += written;
        }

        return totalWritten;
    }

    private int Serialize(LayerHeaderDto headerDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        switch (attrHeaderDto.DataAccessor)
        {
            case DeckGlDataAccessor.GetPosition:
                return SerializePosition(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetRadius:
                return SerializeRadius(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetFillColor:
                return SerializeFillColor(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetColor:
                return SerializeColor(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetLineColor:
                return SerializeLineColor(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetLineWidth:
                return SerializeLineWidth(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetPath:
                return SerializePath(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetWidth:
                return SerializeWidth(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetPolygon:
                return SerializePolygon(headerDto, attrHeaderDto, annotations, buffer);
            case DeckGlDataAccessor.GetText:
                return SerializeText(headerDto, attrHeaderDto, annotations, buffer);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected virtual int SerializeLineWidth(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializeLineWidth)}");
    }

    protected virtual int SerializePath(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializePath)}");
    }

    protected virtual int SerializeWidth(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializeWidth)}");
    }

    protected virtual int SerializePolygon(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializePolygon)}");
    }

    protected virtual int SerializeText(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializeText)}");
    }

    protected virtual int SerializeLineColor(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializeLineColor)}");
    }

    protected virtual int SerializeColor(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializeColor)}");
    }

    protected virtual int SerializeFillColor(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializeFillColor)}");
    }

    protected virtual int SerializeRadius(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializeRadius)}");
    }

    protected virtual int SerializePosition(LayerHeaderDto layerHeaderDto, AttributeHeaderDto attrHeaderDto,
        DeckGlLayer<AnnotationShape> annotations, Span<byte> buffer)
    {
        throw new NotSupportedException($"This LayerSerializer does not support {nameof(SerializePosition)}");
    }
}