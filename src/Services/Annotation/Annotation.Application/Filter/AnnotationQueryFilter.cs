using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.Extensions.Expressions;
using PreciPoint.Ims.Services.Annotation.Application.Authorization;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PreciPoint.Ims.Services.Annotation.Application.Filter;

public class AnnotationQueryFilter
{
    public Expression<Func<AnnotationShape, bool>> GetAnnotationsFilter(Guid slideImageId, Guid userId)
    {
        Expression<Func<AnnotationShape, bool>> filter =
            annotation => annotation.SlideImageId == slideImageId &&
                          (annotation.CreatedBy == userId
                           || annotation.Visibility == AnnotationVisibility.Public
                           || annotation.Visibility == AnnotationVisibility.Editable
                           || annotation.Visibility == AnnotationVisibility.ReadOnly);

        return filter;
    }

    public Expression<Func<AnnotationShape, bool>> GetAnnotationsFilter(IReadOnlyList<Guid> annotationIds,
        Guid userId)
    {
        Expression<Func<AnnotationShape, bool>> filter =
            annotation => annotationIds.Contains(annotation.Id) &&
                          (annotation.CreatedBy == userId
                           || annotation.Visibility == AnnotationVisibility.Public
                           || annotation.Visibility == AnnotationVisibility.Editable
                           || annotation.Visibility == AnnotationVisibility.ReadOnly);

        return filter;
    }

    public Expression<Func<AnnotationShape, bool>> GetEditableAnnotationsFilter(Guid slideImageId,
        IClaimsPrincipalProvider claimsPrincipalProvider, AnnotationVisibility annotationVisibility)
    {
        Expression<Func<AnnotationShape, bool>> filter =
            annotation => annotation.SlideImageId == slideImageId;

        if (claimsPrincipalProvider.Current.HasRole(Roles.ManageForeignAnnotations))
        {
            return filter;
        }

        return filter.And(annotation => annotation.CreatedBy == claimsPrincipalProvider.Current.UserId);
    }
}