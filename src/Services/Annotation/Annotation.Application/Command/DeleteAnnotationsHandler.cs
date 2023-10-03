using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class DeleteAnnotations : IRequest<DeleteOperationDto>
{
    public DeleteAnnotations(Guid slideImageId)
    {
        SlideImageId = slideImageId;
    }

    public Guid SlideImageId { get; }
}

public class DeleteAnnotationsHandler : IRequestHandler<DeleteAnnotations, DeleteOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly AnnotationQueryFilter _annotationQueryFilter;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ILogger<DeleteAnnotationsHandler> _logger;

    public DeleteAnnotationsHandler(ILogger<DeleteAnnotationsHandler> logger, IAnnotationQueries annotationQueries,
        AnnotationQueryFilter annotationQueryFilter, IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _logger = logger;
        _annotationQueries = annotationQueries;
        _annotationQueryFilter = annotationQueryFilter;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public async Task<DeleteOperationDto> Handle(DeleteAnnotations request,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<AnnotationShape, bool>> filter =
            _annotationQueryFilter.GetAnnotationsFilter(request.SlideImageId, _claimsPrincipalProvider.Current.UserId);

        List<AnnotationShape> annotationsToDelete = await _annotationQueries.GetAnnotations(filter).ToListAsync(cancellationToken);
        if (!annotationsToDelete.Any())
        {
            _logger.LogWarning(
                "no annotations to delete for slide image id: '{SlideImageId}' and for User id: '{UserId}'",
                request.SlideImageId, _claimsPrincipalProvider.Current.UserId);

            return new DeleteOperationDto { NumberOfEntityRemoved = 0 };
        }

        _annotationDbContext.Set<AnnotationShape>().RemoveRange(annotationsToDelete);

        int result = await _annotationDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("User '{UserId}' deleted '{Deleted}' annotations for slide image id: '{SlideImageId}'",
            _claimsPrincipalProvider.Current.UserId, annotationsToDelete.Count, request.SlideImageId);

        return new DeleteOperationDto { NumberOfEntityRemoved = result };
    }
}