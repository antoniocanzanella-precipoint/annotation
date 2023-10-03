using MediatR;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class AddAnnotationCounters : IRequest<GenericCudOperationDto>
{
    public AddAnnotationCounters(CounterGroupDto dto)
    {
        Dto = dto;
    }

    public CounterGroupDto Dto { get; }
}

public class AddAnnotationCountersHandler : IRequestHandler<AddAnnotationCounters, GenericCudOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly ApplicationConfig _appConfig;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ICountingQueries _countingQueries;
    private readonly GeometryFactory _geometryFactory;
    private readonly IStringLocalizer _stringLocalizer;

    public AddAnnotationCountersHandler(ICountingQueries countingQueries, IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer,
        GeometryFactory geometryFactory, ApplicationConfig appConfig)
    {
        _countingQueries = countingQueries;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _stringLocalizer = stringLocalizer;
        _geometryFactory = geometryFactory;
        _appConfig = appConfig;
    }

    public async Task<GenericCudOperationDto> Handle(AddAnnotationCounters request,
        CancellationToken cancellationToken = default)
    {
        CounterGroup countingGroupToUpdate = await BusinessValidation.CheckIfAnnotationCounterGroupExist(_countingQueries,
            request.Dto.Id, _stringLocalizer, cancellationToken);

        BusinessValidation.CheckUserWritePermission(countingGroupToUpdate.Annotation, _claimsPrincipalProvider,
            _stringLocalizer);

        BusinessValidation.CheckCoordinates(request.Dto.Counters, _stringLocalizer);

        countingGroupToUpdate.Counters = AddCountersFromDto(countingGroupToUpdate, request.Dto.Counters,
            countingGroupToUpdate.Annotation);

        countingGroupToUpdate.IsModified(_claimsPrincipalProvider.Current.UserId);

        _annotationDbContext.Set<CounterGroup>().Update(countingGroupToUpdate);

        return new GenericCudOperationDto(await _annotationDbContext.SaveChangesAsync(cancellationToken));
    }

    private ICollection<Counter> AddCountersFromDto(CounterGroup entity, double[][] dto, AnnotationShape annotation)
    {
        Polygon polygon = null;
        if (annotation.Type == AnnotationType.Circle)
        {
            polygon = annotation.GetPolygonFromCircle(_appConfig.CirclePointApproximationCoefficient, _geometryFactory);
        }

        for (var i = 0; i < dto.Length; i++)
        {
            var counter = new Counter { Shape = _geometryFactory.CreatePoint(new Coordinate(dto[i][0], dto[i][1])), GroupCounterId = entity.Id };

            if (BusinessValidation.CheckIfShapeContains(annotation, counter, polygon))
            {
                entity.Counters.Add(counter);
            }
        }

        return entity.Counters;
    }
}