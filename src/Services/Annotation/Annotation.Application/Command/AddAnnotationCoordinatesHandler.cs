using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class AddAnnotationCoordinates : IRequest<AnnotationDto>
{
    public AddAnnotationCoordinates(Guid annotationId, VertexUpdateDto dto)
    {
        AnnotationId = annotationId;
        Dto = dto;
    }

    public Guid AnnotationId { get; }
    public VertexUpdateDto Dto { get; }
}

public class AnnotationCoordinatesAddHandler : IRequestHandler<AddAnnotationCoordinates, AnnotationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly GeometryFactory _geometryFactory;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public AnnotationCoordinatesAddHandler(IAnnotationQueries annotationQueries, IDbContext annotationDbContext,
        IMapper mapper, IClaimsPrincipalProvider claimsPrincipalProvider, GeometryFactory geometryFactory,
        IStringLocalizer stringLocalizer)
    {
        _annotationQueries = annotationQueries;
        _annotationDbContext = annotationDbContext;
        _mapper = mapper;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _geometryFactory = geometryFactory;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<AnnotationDto> Handle(AddAnnotationCoordinates request,
        CancellationToken cancellationToken = default)
    {
        AnnotationShape annotationToUpdate = await BusinessValidation.CheckIfAnnotationExist(_annotationQueries,
            request.AnnotationId, _stringLocalizer, cancellationToken);

        BusinessValidation.CheckUserWritePermission(annotationToUpdate, _claimsPrincipalProvider, _stringLocalizer);

        BusinessValidation.CheckCoordinates(request.Dto.CoordinatesDto, _stringLocalizer);

        BusinessValidation.CheckIfAnnotationCanReceiveUpdate(annotationToUpdate, request.Dto.Index, _stringLocalizer);

        annotationToUpdate.AddCoordinatesFromDto(request.Dto.CoordinatesDto, request.Dto.Index,
            annotationToUpdate.Type, _geometryFactory);

        annotationToUpdate.IsModified(_claimsPrincipalProvider.Current.UserId);

        _annotationDbContext.Set<AnnotationShape>().Update(annotationToUpdate);

        await _annotationDbContext.SaveChangesAsync(cancellationToken);
        return _mapper.Map<AnnotationDto>(annotationToUpdate);
    }
}