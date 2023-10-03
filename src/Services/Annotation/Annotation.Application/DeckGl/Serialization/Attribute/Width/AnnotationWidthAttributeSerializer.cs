using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Constants;
using PreciPoint.Ims.Services.Annotation.Application.Extensions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Width;

public class AnnotationWidthAttributeSerializer : IAttributeSerializer
{
    public int SerializeAttribute(DeckGlLayer<AnnotationShape> layer, LayerHeaderDto header, Span<byte> target)
    {
        header.ThrowIfNotAttributeHeaderPresent(DeckGlDataAccessor.GetWidth, out _);

        for (var i = 0; i < header.VertexCount; i++)
        {
            target[i] = SerializationConstants.DotsLineWidth;
        }

        return header.VertexCount;
    }
}