using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.Authorization.Providers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure;

public class RequestLogger<TRequest> : IRequestPreProcessor<TRequest>
{
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ILogger _logger;

    public RequestLogger(ILogger<TRequest> logger, IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _logger = logger;
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        //some notification will trigger commands without user. this is usually the case of hosted services
        try
        {
            Guid? aClaimsPrincipal = _claimsPrincipalProvider?.Current.UserId;
        }
        catch (NullReferenceException)
        {
            return Task.CompletedTask;
        }

        string requestName = typeof(TRequest).Name;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("User with id {UserId} made request: {RequestName} {@Request}", _claimsPrincipalProvider.Current.UserId, requestName, request);
        }
        else
        {
            _logger.LogInformation("User with id {UserId} made request: {RequestName}", _claimsPrincipalProvider.Current.UserId, requestName);
        }

        return Task.CompletedTask;
    }
}