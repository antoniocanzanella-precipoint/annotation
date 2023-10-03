using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation;

public interface IDeckGlAnnotationSerializer
{
    public void Serialize(IReadOnlyList<BaseLayerHeaderDto> headers,
        IReadOnlyDictionary<string, DeckGlLayer<AnnotationShape>> deckGlLayers,
        Memory<byte> mem);
}