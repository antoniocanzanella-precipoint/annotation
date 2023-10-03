using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.MessagePack.Resolver;

internal static class BasicCustomResolverGetFormatterHelper
{
    private static readonly Dictionary<Type, object> FormatterMap = new() { { typeof(BaseLayerHeaderDto), new BaseLayerHeaderDtoMessagePackFormatter() } };

    internal static object GetFormatter(Type t)
    {
        if (FormatterMap.TryGetValue(t, out object formatter))
        {
            return formatter;
        }

        // If type can not get, must return null for fallback mechanism.
        return null;
    }
}