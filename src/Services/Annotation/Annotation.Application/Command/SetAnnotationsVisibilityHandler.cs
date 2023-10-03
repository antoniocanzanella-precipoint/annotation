using MediatR;
using Microsoft.EntityFrameworkCore;
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

public class SetAnnotationsVisibility : IRequest<GenericCudOperationDto>
{
    public SetAnnotationsVisibility(SetAnnotationsVisibilityDto dto)
    {
        Dto = dto;
    }

    public SetAnnotationsVisibilityDto Dto { get; }
}

public class SetAnnotationsVisibilityHandler : IRequestHandler<SetAnnotationsVisibility, GenericCudOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly AnnotationQueryFilter _annotationQueryFilter;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;

    public SetAnnotationsVisibilityHandler(AnnotationQueryFilter annotationQueryFilter, IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _annotationQueryFilter = annotationQueryFilter;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public async Task<GenericCudOperationDto> Handle(SetAnnotationsVisibility request,
        CancellationToken cancellationToken)
    {
        Expression<Func<AnnotationShape, bool>> filter =
            _annotationQueryFilter.GetEditableAnnotationsFilter(request.Dto.SlideImageId, _claimsPrincipalProvider,
                request.Dto.Visibility);

        List<AnnotationShape> annotationsToUpdate =
            await _annotationDbContext.Set<AnnotationShape>().Where(filter).ToListAsync(cancellationToken);

        foreach (AnnotationShape annotation in annotationsToUpdate)
        {
            annotation.Visibility = request.Dto.Visibility;
            _annotationDbContext.Set<AnnotationShape>().Update(annotation);
        }

        return new GenericCudOperationDto(await _annotationDbContext.SaveChangesAsync(cancellationToken));
    }
}