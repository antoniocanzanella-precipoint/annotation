using MediatR;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure;

public class RequestPerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ApplicationConfig _applicationConfig;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ILogger<TRequest> _logger;
    private readonly Stopwatch _timer;

    public RequestPerformanceBehaviour(ILogger<TRequest> logger, ApplicationConfig applicationConfig,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _applicationConfig = applicationConfig;
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();
        TResponse response = await next();
        _timer.Stop();

        //some notification will trigger commands without user. this is usually the case of hosted services
        try
        {
            Guid? aClaimsPrincipal = _claimsPrincipalProvider?.Current.UserId;
        }
        catch (NullReferenceException)
        {
            return response;
        }

        if (_timer.ElapsedMilliseconds >= _applicationConfig.PerformanceBehaviour.LongRunningTriggerMilliseconds)
        {
            string requestName = typeof(TRequest).Name;
            _logger.LogWarning(
                "User {UserId} facing {RequestWithWarnings}: {RequestName} ({ElapsedMilliseconds} milliseconds) {@Request}",
                _claimsPrincipalProvider.Current.UserId, "Long Running Request", requestName,
                _timer.ElapsedMilliseconds, request);
        }

        return response;
    }
}