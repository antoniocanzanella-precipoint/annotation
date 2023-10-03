using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Scatterplot;

public interface IScatterplotLayerSerializer
{
    int SerializePathLayer(LayerHeaderDto header, DeckGlScatterplotLayer<AnnotationShape> layer, Span<byte> memory);
}