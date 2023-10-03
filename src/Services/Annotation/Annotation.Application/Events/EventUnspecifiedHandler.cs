using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciPoint.Ims.Services.Annotation.Application.Events;

public class EventUnspecified : INotification
{
    public EventUnspecified(string message)
    {
        Message = message;
    }

    public string Message { get; }
}

public class EventUnspecifiedHandler : NotificationHandler<EventUnspecified>
{
    private readonly ILogger<EventUnspecifiedHandler> _logger;

    public EventUnspecifiedHandler(ILogger<EventUnspecifiedHandler> logger)
    {
        _logger = logger;
    }

    protected override void Handle(EventUnspecified eventUnspecified)
    {
        _logger.LogWarning(
            $"{eventUnspecified.Message}. The parsed message could not get mapped to any well known event.");
    }
}