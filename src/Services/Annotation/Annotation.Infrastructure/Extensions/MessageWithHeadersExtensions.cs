using MediatR;
using PreciPoint.Ims.Messaging.Core.DataTransfer;
using PreciPoint.Ims.Services.Annotation.Application.Events;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Infrastructure.Extensions;

internal static class MessageWithHeadersExtensions
{
    public static INotification ToNotification(this MessageWithHeaders<SlideImageDto> messageWithHeaders)
    {
        EventValue eventValue = messageWithHeaders.Headers
            .Where(keyValuePair => keyValuePair.Key == HeaderKey.Event)
            .Select(keyValuePair => Enum.Parse<EventValue>(keyValuePair.Value))
            .FirstOrDefault();

        return eventValue switch
        {
            EventValue.Deleted => new SlideImageDeleted(messageWithHeaders.Payload),
            EventValue.Created => new SlideImageCreated(messageWithHeaders.Payload),
            EventValue.Updated => new SlideImageUpdated(messageWithHeaders.Payload),
            _ => new EventUnspecified(HeaderKeysAndValues(messageWithHeaders.Headers))
        };
    }

    private static string HeaderKeysAndValues(IReadOnlyDictionary<HeaderKey, string> headers)
    {
        return string.Join(", ",
            headers.Select(keyValuePair => $"Key '{keyValuePair.Key}' with value '{keyValuePair.Value}'"));
    }
}