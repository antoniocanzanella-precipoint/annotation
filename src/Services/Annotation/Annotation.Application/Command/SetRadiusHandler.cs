using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class SetRadius : IRequest<AnnotationDto>
{
    public SetRadius(Guid annotationId, double radius)
    {
        AnnotationId = annotationId;
        Radius = radius;
    }

    public Guid AnnotationId { get; }
    public double Radius { get; }
}

public class SetRadiusHandler : IRequestHandler<SetRadius, AnnotationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly GeometryFactory _geometryFactory;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public SetRadiusHandler(IDbContext annotationDbContext, IMapper mapper,
        IClaimsPrincipalProvider claimsPrincipalProvider, GeometryFactory geometryFactory,
        IAnnotationQueries annotationQueries, IStringLocalizer stringLocalizer)
    {
        _annotationDbContext = annotationDbContext;
        _mapper = mapper;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _geometryFactory = geometryFactory;
        _annotationQueries = annotationQueries;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<AnnotationDto> Handle(SetRadius request, CancellationToken cancellationToken)
    {
        AnnotationShape annotationToUpdate = await BusinessValidation.CheckIfAnnotationExist(_annotationQueries,
            request.AnnotationId, _stringLocalizer, cancellationToken);

        if (annotationToUpdate.Type != AnnotationType.Circle)
        {
            string message = _stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_SET_RADIUS", request.AnnotationId,
                annotationToUpdate.Type];
            throw new MessageOnly(message).ToApiException();
        }

        double[][] coordinates = _mapper.Map<double[][]>(annotationToUpdate.Shape.Coordinates);
        coordinates[1][0] = coordinates[0][0] + request.Radius;
        coordinates[1][1] = coordinates[0][1];

        annotationToUpdate.TransformCoordinatesFromDto(coordinates, _geometryFactory);

        annotationToUpdate.IsModified(_claimsPrincipalProvider.Current.UserId);

        _annotationDbContext.Set<AnnotationShape>().Update(annotationToUpdate);
        await _annotationDbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AnnotationDto>(annotationToUpdate);
    }
}