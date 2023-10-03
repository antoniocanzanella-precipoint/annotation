using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Constants;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Color;

public class AnnotationVertexLenColorAttributeSerializer : IAttributeSerializer
{
    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetColor, out AttributeHeaderDto attribute);

        return AnnotationAttributeSerializerHelper.SerializeColor(attribute, layer, target,
            annota => annota.Shape.NumPoints, SerializationConstants.DefaultColor);
    }
}