using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation;

public class DeckGlAnnotationSerializerOptions
{
    public Dictionary<string, IDeckGlAnnotationSingleLayerSerializer> SingleLayerSerializer { get; set; }

    public Dictionary<string, IDeckGlAnnotationCompositeLayerSerializer> CompositeLayerSerializer { get; set; }

    public Dictionary<DeckGlLayerType, IDeckGlAnnotationSingleLayerSerializer> DefaultSerializer { get; set; }
}

public class DeckGlAnnotationSerializer : IDeckGlAnnotationSerializer
{
    private readonly Dictionary<string, IDeckGlAnnotationCompositeLayerSerializer> _compositeLayerSerializers;

    private readonly Dictionary<DeckGlLayerType, IDeckGlAnnotationSingleLayerSerializer> _defaultLayerSerializers;
    private readonly Dictionary<string, IDeckGlAnnotationSingleLayerSerializer> _serializers;

    public DeckGlAnnotationSerializer(DeckGlAnnotationSerializerOptions options)
    {
        _serializers = options.SingleLayerSerializer ?? new Dictionary<string, IDeckGlAnnotationSingleLayerSerializer>();
        _compositeLayerSerializers = options.CompositeLayerSerializer ?? new Dictionary<string, IDeckGlAnnotationCompositeLayerSerializer>();
        _defaultLayerSerializers = options.DefaultSerializer;
    }

    public void Serialize(IReadOnlyList<BaseLayerHeaderDto> headers,
        IReadOnlyDictionary<string, DeckGlLayer<AnnotationShape>> deckGlLayers,
        Memory<byte> mem)
    {
        foreach (BaseLayerHeaderDto header in headers)
        {
            DeckGlLayer<AnnotationShape> layer = deckGlLayers[header.Id];
            int offset = header.Offset;
            if (header.Type == DeckGlLayerType.Composite)
            {
                SerializeCompositeLayer(layer, header, mem, offset);
            }
            else
            {
                SerializeSingleLayer(layer, header, mem, offset);
            }
        }
    }

    private void SerializeSingleLayer(DeckGlLayer<AnnotationShape> layer, BaseLayerHeaderDto header, Memory<byte> mem, int offset)
    {
        Span<byte> memSlice = mem.Span.Slice(offset, header.TotalSizeInBytes);
        if (_serializers.TryGetValue(header.Id, out IDeckGlAnnotationSingleLayerSerializer serializer))
        {
            serializer.SerializeLayer((LayerHeaderDto) header, layer, memSlice);
        }
        else
        {
            if (_defaultLayerSerializers.TryGetValue(header.Type, out IDeckGlAnnotationSingleLayerSerializer defaultSerializer))
            {
                defaultSerializer.SerializeLayer((LayerHeaderDto) header, layer, memSlice);
            }
            else
            {
                throw new InvalidOperationException($"Unable to find serializer for type {header.Type}");
            }
        }
    }

    private void SerializeCompositeLayer(DeckGlLayer<AnnotationShape> layer, BaseLayerHeaderDto header,
        Memory<byte> mem, int offset)
    {
        bool ok = _compositeLayerSerializers.TryGetValue(header.Id, out IDeckGlAnnotationCompositeLayerSerializer compSerializer);
        if (ok)
        {
            compSerializer.SerializeLayer((CompositeLayerHeaderDto) header,
                (DeckGlCompositeLayer<AnnotationShape>) layer, mem.Span.Slice(offset, header.TotalSizeInBytes));
        }
    }
}