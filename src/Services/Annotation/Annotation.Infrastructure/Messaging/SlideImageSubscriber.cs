using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Messaging.Core.Config;
using PreciPoint.Ims.Messaging.Core.DataTransfer;
using PreciPoint.Ims.Messaging.Rabbit.Subscribers;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Infrastructure.Extensions;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Infrastructure.Messaging;

public class SlideImageSubscriber : AHeadersEventLoopSubscriber<SlideImageDto>
{
    private readonly ApplicationConfig _applicationConfig;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SlideImageSubscriber(ILogger<SlideImageSubscriber> logger, ApplicationConfig applicationConfig,
        IServiceScopeFactory serviceScopeFactory)
        : base(
            logger,
            applicationConfig.MessagingConfig.ConnectionConfig,
            applicationConfig.MessagingConfig.SlideImageSubscriberConfig.Bindings,
            applicationConfig.MessagingConfig.SlideImageSubscriberConfig.ThreadPriority,
            applicationConfig.MessagingConfig.SlideImageSubscriberConfig.SchedulerThreadName)
    {
        _applicationConfig = applicationConfig;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task Process(CancellationToken cancellationToken)
    {
        // The routing key is ignored by queues that consider routing based on header values.
        const string routingKey = "";
        EventLoopSubscriberConfig slideImageSubscriberConfig =
            _applicationConfig.MessagingConfig.SlideImageSubscriberConfig;

        Channel.QueueDeclare(slideImageSubscriberConfig.QueueName, true, false, false, null);

        Logger.LogInformation($"Queue '{slideImageSubscriberConfig.QueueName}' was declared.");

        Channel.BasicQos(0, 1, false);

        foreach (IReadOnlyDictionary<HeaderKey, string> binding in slideImageSubscriberConfig.Bindings)
        {
            Dictionary<string, object> headerBinding = binding.ToDictionary(
                header => header.Key.ToString(),
                header => string.IsNullOrEmpty(header.Value) ? null : (object) header.Value);

            string bindings = string.Join(", ",
                headerBinding.Select(keyValuePair => $"{keyValuePair.Key}: {keyValuePair.Value}"));
            Logger.LogInformation(
                $"Bind queue '{slideImageSubscriberConfig.QueueName}' to exchange '{slideImageSubscriberConfig.ExchangeName}' for " +
                $"headers '{bindings}'.");

            Channel.QueueBind(slideImageSubscriberConfig.QueueName, slideImageSubscriberConfig.ExchangeName,
                routingKey, headerBinding);
        }

        CreateMessageStream(slideImageSubscriberConfig.QueueName, true)
            .Select(messageWithHeaders => messageWithHeaders.ToNotification())
            .Subscribe(
                notification =>
                {
                    using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
                    var mediator = serviceScope.ServiceProvider.GetService<IMediator>();
                    mediator.Publish(notification, cancellationToken);
                },
                error => Logger.LogError(error, "Failure during message consumption."),
                () => Logger.LogInformation("Message consumption was stopped."));

        Logger.LogDebug($"Subscription for queue '{slideImageSubscriberConfig.QueueName}' was started.");

        return Task.CompletedTask;
    }

    protected override bool HasValidProperties(BasicDeliverEventArgs basicDeliverEventArgs)
    {
        bool hasValidProperties = basicDeliverEventArgs.BasicProperties.IsHeadersPresent() &&
                                  basicDeliverEventArgs.BasicProperties.IsTypePresent();

        if (!hasValidProperties)
        {
            Logger.LogWarning("Received message with properties that were not expected for this subscriber. " +
                              $"Headers present: {basicDeliverEventArgs.BasicProperties.IsHeadersPresent()}, " +
                              $"Type present: {basicDeliverEventArgs.BasicProperties.IsTypePresent()}");
        }

        return hasValidProperties;
    }
}