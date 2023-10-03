using PreciPoint.Ims.Messaging.Core.Config;

namespace PreciPoint.Ims.Services.Annotation.Application.Configuration;

public class MessagingConfig
{
    public ConnectionConfig ConnectionConfig { get; set; }
    public EventLoopSubscriberConfig SlideImageSubscriberConfig { get; set; }
}