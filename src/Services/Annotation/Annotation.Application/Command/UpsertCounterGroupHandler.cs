using AutoMapper;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class UpsertCounterGroup : IRequest<CounterGroupDto>
{
    public UpsertCounterGroup(CounterGroupDto dto)
    {
        Dto = dto;
    }

    public CounterGroupDto Dto { get; }
}

public class UpsertCounterGroupHandler : IRequestHandler<UpsertCounterGroup, CounterGroupDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly ApplicationConfig _appConfig;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ICountingQueries _countingQueries;
    private readonly GeometryFactory _geometryFactory;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public UpsertCounterGroupHandler(IAnnotationQueries annotationQueries, ICountingQueries countingQueries,
        IDbContext annotationDbContext, IMapper mapper, ApplicationConfig appConfig,
        IClaimsPrincipalProvider claimsPrincipalProvider, GeometryFactory geometryFactory,
        IStringLocalizer stringLocalizer)
    {
        _annotationQueries = annotationQueries;
        _countingQueries = countingQueries;
        _annotationDbContext = annotationDbContext;
        _mapper = mapper;
        _appConfig = appConfig;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _geometryFactory = geometryFactory;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<CounterGroupDto> Handle(UpsertCounterGroup request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = new UpsertCounterGroupValidator(_stringLocalizer).Validate(request);
        if (!validationResult.IsValid)
        {
            throw validationResult.ToApiException();
        }

        AnnotationShape annotation = await BusinessValidation.CheckIfAnnotationExist(_annotationQueries, request.Dto.AnnotationId,
            _stringLocalizer, cancellationToken);

        BusinessValidation.CheckUserWritePermission(annotation, _claimsPrincipalProvider, _stringLocalizer);

        CounterGroup counterGroup;
        if (request.Dto.Id.HasValue)
        {
            counterGroup =
                await _countingQueries.GetCounterGroupById(request.Dto.Id.Value, cancellationToken);
            counterGroup = Update(request, counterGroup, annotation);
            _annotationDbContext.Set<CounterGroup>().Update(counterGroup);
        }
        else
        {
            BusinessValidation.CheckIfAnnotationCanHaveCounters(annotation, _stringLocalizer);
            counterGroup = Create(request, annotation);
            await _annotationDbContext.Set<CounterGroup>().AddAsync(counterGroup, cancellationToken);
        }

        await _annotationDbContext.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CounterGroupDto>(counterGroup);
    }

    private CounterGroup Create(UpsertCounterGroup request, AnnotationShape annotation)
    {
        var counterGroup = _mapper.Map<CounterGroup>(request);

        Polygon polygon = null;
        if (annotation.Type == AnnotationType.Circle)
        {
            polygon = annotation.GetPolygonFromCircle(_appConfig.CirclePointApproximationCoefficient, _geometryFactory);
        }

        for (var i = 0; i < request.Dto.Counters.Length; i++)
        {
            var counter = new Counter
            {
                Shape =
                    _geometryFactory.CreatePoint(
                        new Coordinate(
                            request.Dto.Counters[i][0],
                            request.Dto.Counters[i][1]
                        ))
            };

            if (BusinessValidation.CheckIfShapeContains(annotation, counter, polygon))
            {
                counterGroup.Counters.Add(counter);
            }
        }

        counterGroup.IsCreated(_claimsPrincipalProvider.Current.UserId);

        return counterGroup;
    }

    private CounterGroup Update(UpsertCounterGroup request, CounterGroup entityToUpdate, AnnotationShape annotation)
    {
        entityToUpdate.Label = request.Dto.Label;
        entityToUpdate.Description = request.Dto.Description;

        Polygon polygon = null;
        if (annotation.Type == AnnotationType.Circle)
        {
            polygon = annotation.GetPolygonFromCircle(_appConfig.CirclePointApproximationCoefficient, _geometryFactory);
        }

        entityToUpdate.Counters.Clear();
        for (var i = 0; i < request.Dto.Counters.Length; i++)
        {
            var counter = new Counter
            {
                Shape =
                    _geometryFactory.CreatePoint(
                        new Coordinate(
                            request.Dto.Counters[i][0],
                            request.Dto.Counters[i][1]
                        ))
            };

            if (BusinessValidation.CheckIfShapeContains(annotation, counter, polygon))
            {
                entityToUpdate.Counters.Add(counter);
            }
        }

        entityToUpdate.IsModified(_claimsPrincipalProvider.Current.UserId);

        return entityToUpdate;
    }
}