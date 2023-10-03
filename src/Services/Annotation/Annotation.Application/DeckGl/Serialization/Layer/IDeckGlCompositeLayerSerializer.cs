using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer;

public interface IDeckGlCompositeLayerSerializer<T>
{
    int SerializeLayer(CompositeLayerHeaderDto headerDto, DeckGlCompositeLayer<T> compositeLayer, Span<byte> memory);
}

public interface IDeckGlAnnotationCompositeLayerSerializer : IDeckGlCompositeLayerSerializer<AnnotationShape> { }