using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.BackgroundProcessing.Processors;
using PreciPoint.Ims.Services.Annotation.Application.Notification;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Infrastructure.Services;

/// <summary>
/// As the whole slide image service heavily depends on data owned by the image management service, we provide a scoped
/// processor for the connection.
/// </summary>
public class ImageManagementConnector : ScopedProcessor
{
    /// <summary>
    /// Provide a factory for scoped access and a corresponding logger.
    /// </summary>
    /// <param name="serviceScopeFactory">The factory allows to create services within a scope on demand.</param>
    /// <param name="logger">Provides a concrete implementation to a logger.</param>
    public ImageManagementConnector(IServiceScopeFactory serviceScopeFactory, ILogger<ImageManagementConnector> logger)
        : base(serviceScopeFactory, logger) { }

    /// <summary>
    /// We check if this whole slide image is already registered as slide storage and fill the cache with corresponding
    /// slide images.
    /// </summary>
    /// <param name="serviceProvider">Allows the creation of scopes.</param>
    /// <param name="cancellationToken">Could be used to cancel ongoing processing.</param>
    /// <returns>A processing task that handles this service's slide storage relation and fills the whole slide image cache.</returns>
    protected override async Task ProcessInScope(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // We can check back with .NET6, but it seems for now that we need to stay synchronous in order to get exceptions that make the application explode
        // on startup. In this case we want to explode so the Docker container gets restarted.
        mediator.Publish(new FillSlideImages(), cancellationToken).GetAwaiter().GetResult();

        try
        {
            await StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Unable stop scoped background service.");
        }
    }

    /// <summary>
    /// We don't configure any further execution constraints.
    /// </summary>
    /// <param name="cancellationToken">Stops ongoing background processing.</param>
    /// <returns>The executing task.</returns>
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Process(cancellationToken);
    }
}