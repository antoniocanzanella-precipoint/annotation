using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute;

public interface IAttributeSerializer
{
    int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target);
}