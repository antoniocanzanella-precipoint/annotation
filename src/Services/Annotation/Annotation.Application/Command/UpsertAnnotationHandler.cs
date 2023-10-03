using AutoMapper;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class UpsertAnnotation : IRequest<AnnotationDto>
{
    public UpsertAnnotation(Guid slideImageId, AnnotationDto annotationDto)
    {
        SlideImageId = slideImageId;
        AnnotationDto = annotationDto;
    }

    public Guid SlideImageId { get; }

    public AnnotationDto AnnotationDto { get; }
}

public class UpsertAnnotationHandler : IRequestHandler<UpsertAnnotation, AnnotationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly GeometryFactory _geometryFactory;
    private readonly ILogger<UpsertAnnotationHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public UpsertAnnotationHandler(ILogger<UpsertAnnotationHandler> logger, IAnnotationQueries annotationQueries, IDbContext annotationDbContext,
        IMapper mapper, IClaimsPrincipalProvider claimsPrincipalProvider, GeometryFactory geometryFactory, IStringLocalizer stringLocalizer)
    {
        _logger = logger;
        _annotationQueries = annotationQueries;
        _annotationDbContext = annotationDbContext;
        _mapper = mapper;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _geometryFactory = geometryFactory;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<AnnotationDto> Handle(UpsertAnnotation request, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await new UpsertAnnotationValidator().ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw validationResult.ToApiException();
        }

        BusinessValidation.CheckCoordinates(request.AnnotationDto.Coordinates, _stringLocalizer);

        AnnotationShape annotation;
        if (request.AnnotationDto.Id.HasValue)
        {
            annotation = await _annotationQueries.GetAnnotationById(request.AnnotationDto.Id.Value, cancellationToken);
            BusinessValidation.CheckUserWritePermission(annotation, _claimsPrincipalProvider, _stringLocalizer);
            BusinessValidation.CheckAnnotationIdAndSlideImageId(annotation, request.SlideImageId, _claimsPrincipalProvider.Current.UserId, _stringLocalizer);

            annotation = Update(request, annotation);
            _annotationDbContext.Set<AnnotationShape>().Update(annotation);
            _logger.LogInformation($"user {_claimsPrincipalProvider.Current.UserId} updated the annotation {annotation.Id}");
        }
        else
        {
            annotation = await Create(request, cancellationToken);
            _annotationDbContext.Set<AnnotationShape>().Add(annotation);
            _logger.LogInformation($"user {_claimsPrincipalProvider.Current.UserId} created the annotation {annotation.Id}");
        }

        await _annotationDbContext.SaveChangesAsync(cancellationToken);
        return _mapper.Map<AnnotationDto>(annotation);
    }

    private async Task<AnnotationShape> Create(UpsertAnnotation request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<AnnotationShape>(request);

        BusinessValidation.CheckCoordinates(request.AnnotationDto.Coordinates, _stringLocalizer);
        BusinessValidation.CheckCoordinatesAndAnnotationType(entity, request.AnnotationDto.Coordinates,
            _stringLocalizer);
        BusinessValidation.CheckColor(request.AnnotationDto.Color, _stringLocalizer);

        entity.SlideImage = await _annotationDbContext.Set<SlideImage>()
            .FirstAsync(e => e.SlideImageId == entity.SlideImageId, cancellationToken);
        entity.TransformCoordinatesFromDto(request.AnnotationDto.Coordinates, _geometryFactory);
        entity.IsCreated(_claimsPrincipalProvider.Current.UserId);

        return entity;
    }

    private AnnotationShape Update(UpsertAnnotation request, AnnotationShape annotationToUpdate)
    {
        BusinessValidation.CheckCoordinates(request.AnnotationDto.Coordinates, _stringLocalizer);
        BusinessValidation.CheckCoordinatesAndAnnotationType(annotationToUpdate, request.AnnotationDto.Coordinates, _stringLocalizer);
        BusinessValidation.CheckColor(request.AnnotationDto.Color, _stringLocalizer);

        annotationToUpdate.TransformCoordinatesFromDto(request.AnnotationDto.Coordinates, _geometryFactory);

        annotationToUpdate.Label = request.AnnotationDto.Label;
        annotationToUpdate.Description = request.AnnotationDto.Description;
        annotationToUpdate.Type = request.AnnotationDto.AnnotationType;
        annotationToUpdate.Visibility = request.AnnotationDto.Visibility;
        annotationToUpdate.IsModified(_claimsPrincipalProvider.Current.UserId);

        return annotationToUpdate;
    }
}