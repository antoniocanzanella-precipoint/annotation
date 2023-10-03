using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Constants;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Color;

public class AnnotationMarkerPathColorAttributeSerializer : IAttributeSerializer
{
    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetColor, out AttributeHeaderDto attribute);

        // we cannot take the regular color attr serializer, because markers have 2 coordinates that we represent as 1 (x1, y1), (x2, y2) => (x1, y1, x2, y2)
        return AnnotationAttributeSerializerHelper.SerializeColor(attribute, layer, target,
            _ => 1, SerializationConstants.DefaultColor);
    }
}