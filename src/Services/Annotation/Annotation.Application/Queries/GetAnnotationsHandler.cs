using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Queries;

public class GetAnnotations : IRequest<IReadOnlyList<AnnotationDto>>
{
    public GetAnnotations(Guid slideImageId)
    {
        SlideImageId = slideImageId;
    }

    public Guid SlideImageId { get; }
}

public class GetAnnotationsHandler : IRequestHandler<GetAnnotations, IReadOnlyList<AnnotationDto>>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly AnnotationQueryFilter _annotationQueryFilter;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public GetAnnotationsHandler(IAnnotationQueries annotationQueries, IMapper mapper,
        AnnotationQueryFilter annotationQueryFilter, IClaimsPrincipalProvider claimsPrincipalProvider,
        IDbContext annotationDbContext, IStringLocalizer stringLocalizer)
    {
        _annotationQueries = annotationQueries;
        _mapper = mapper;
        _annotationQueryFilter = annotationQueryFilter;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _annotationDbContext = annotationDbContext;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<IReadOnlyList<AnnotationDto>> Handle(GetAnnotations request,
        CancellationToken cancellationToken)
    {
        await BusinessValidation.CheckUserReadPermissionBySlideImageId(_annotationDbContext, _claimsPrincipalProvider,
            request.SlideImageId, _stringLocalizer, cancellationToken);

        Expression<Func<AnnotationShape, bool>> filter =
            _annotationQueryFilter.GetAnnotationsFilter(request.SlideImageId,
                _claimsPrincipalProvider.Current.UserId);

        List<AnnotationShape> annotationList = await _annotationQueries.GetAnnotationsNoTrack(filter).ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<AnnotationDto>>(annotationList);
    }
}