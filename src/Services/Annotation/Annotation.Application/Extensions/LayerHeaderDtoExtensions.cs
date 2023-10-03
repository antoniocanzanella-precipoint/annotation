using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Extensions;

public static class LayerHeaderDtoExtensions
{
    public static void ThrowIfNotAttributeHeaderPresent(this LayerHeaderDto header, DeckGlDataAccessor accessor, out AttributeHeaderDto attributeHeaderDto)
    {
        if (!header.AttributeHeaders.TryGetValue(accessor, out attributeHeaderDto))
        {
            throw new InvalidOperationException($"Header does not contain attributeHeader for {accessor}");
        }
    }
}