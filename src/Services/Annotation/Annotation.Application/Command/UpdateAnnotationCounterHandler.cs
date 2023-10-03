using MediatR;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class UpdateAnnotationCounter : IRequest<GenericCudOperationDto>
{
    public UpdateAnnotationCounter(Guid counterId, double x, double y)
    {
        CounterId = counterId;
        X = x;
        Y = y;
    }

    public Guid CounterId { get; }
    public double X { get; }
    public double Y { get; }
}

public class UpdateAnnotationCounterHandler : IRequestHandler<UpdateAnnotationCounter, GenericCudOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly ApplicationConfig _appConfig;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ICountingQueries _countingQueries;
    private readonly GeometryFactory _geometryFactory;
    private readonly IStringLocalizer _stringLocalizer;

    public UpdateAnnotationCounterHandler(ICountingQueries countingQueries, IStringLocalizer stringLocalizer,
        IDbContext annotationDbContext, IClaimsPrincipalProvider claimsPrincipalProvider,
        GeometryFactory geometryFactory, ApplicationConfig appConfig)
    {
        _countingQueries = countingQueries;
        _stringLocalizer = stringLocalizer;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _geometryFactory = geometryFactory;
        _appConfig = appConfig;
    }

    public async Task<GenericCudOperationDto> Handle(UpdateAnnotationCounter request,
        CancellationToken cancellationToken = default)
    {
        Counter counterToUpdate = await BusinessValidation.CheckIfCounterExist(_countingQueries, request.CounterId,
            _stringLocalizer, cancellationToken);

        AnnotationShape annotation = counterToUpdate.CounterGroup.Annotation;

        BusinessValidation.CheckUserWritePermission(annotation, _claimsPrincipalProvider, _stringLocalizer);

        counterToUpdate.Shape = _geometryFactory.CreatePoint(new Coordinate(request.X, request.Y));

        if (!CheckIfCounterIsInsideTheArea(counterToUpdate, annotation))
        {
            string message = _stringLocalizer["APPLICATION.ANNOTATIONS.COUNTERS.NOT_CONTAINED", annotation.Id,
                annotation.Type, request.X, request.Y, request.CounterId];
            throw new MessageOnly(message).ToApiException();
        }

        _annotationDbContext.Set<Counter>().Update(counterToUpdate);

        return new GenericCudOperationDto(await _annotationDbContext.SaveChangesAsync(cancellationToken));
    }

    private bool CheckIfCounterIsInsideTheArea(Counter counter, AnnotationShape annotation)
    {
        Polygon polygon = null;
        if (annotation.Type == AnnotationType.Circle)
        {
            polygon = annotation.GetPolygonFromCircle(_appConfig.CirclePointApproximationCoefficient,
                _geometryFactory);
        }

        return BusinessValidation.CheckIfShapeContains(annotation, counter, polygon);
    }
}