using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Path;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Text;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Custom;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.CompositeLayer.Marker;

public class AnnotationMarkerLayerSerializer : IDeckGlAnnotationCompositeLayerSerializer
{
    private readonly IAnnotationPathLayerSerializer _markerPathSerializer;
    private readonly IAnnotationTextLayerSerializer _textLayerSerializer;

    public AnnotationMarkerLayerSerializer(IAnnotationTextLayerSerializer textLayerSerializer, IAnnotationPathLayerSerializer markerPathSerializer)
    {
        _textLayerSerializer = textLayerSerializer;
        _markerPathSerializer = markerPathSerializer;
    }

    public int SerializeLayer(CompositeLayerHeaderDto headerDto, DeckGlCompositeLayer<AnnotationShape> compositeLayer,
        Span<byte> memory)
    {
        var markerLayer = compositeLayer as DeckGlMarkerLayer<AnnotationShape>;
        if (markerLayer is null)
        {
            return 0;
        }

        var written = 0;
        for (var index = 0; index < headerDto.CompositeLayerHeaders.Count; index++)
        {
            BaseLayerHeaderDto subHeader = headerDto.CompositeLayerHeaders[index];
            Span<byte> mem = memory.Slice(subHeader.Offset, subHeader.TotalSizeInBytes);
            if (subHeader.Type == DeckGlLayerType.Text)
            {
                written += _textLayerSerializer.SerializeTextLayer((LayerHeaderDto) subHeader, markerLayer.TextLayer,
                    mem);
            }
            else if (subHeader.Type == DeckGlLayerType.Path)
            {
                written += _markerPathSerializer.SerializePathLayer((LayerHeaderDto) subHeader, markerLayer.PathLayer,
                    mem);
            }
        }

        return written;
    }
}