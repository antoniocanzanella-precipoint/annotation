using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Domain.Aggregate;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application;

public static class BusinessRepository
{
    public static async Task<AnnotationPermissions> GetGlobalAnnotationPermissionsBySlideImageId(IDbContext dbContext,
        Guid slideImageId, IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        SlideImage slideImage = await dbContext.Set<SlideImage>()
            .FirstOrDefaultAsync(e => e.SlideImageId == slideImageId, cancellationToken);

        if (slideImage == null)
        {
            string message = stringLocalizer["APPLICATION.SLIDE_IMAGES.NOT_FOUND", slideImageId];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }

        return new AnnotationPermissions(slideImage, claimsPrincipalProvider.Current.UserId);
    }

    public static async Task<AnnotationPermissions> GetGlobalAnnotationPermissionsByAnnotationId(IDbContext dbContext,
        Guid annotationId, IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        AnnotationShape annotation = await dbContext.Set<AnnotationShape>()
            .Include(e => e.SlideImage)
            .FirstOrDefaultAsync(e => e.Id == annotationId, cancellationToken);

        if (annotation == null)
        {
            string message = stringLocalizer["APPLICATION.ANNOTATIONS.NOT_FOUND", annotationId];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }

        return new AnnotationPermissions(annotation.SlideImage, claimsPrincipalProvider.Current.UserId);
    }

    public static async Task<AnnotationPermissions> GetGlobalAnnotationPermissionsByCounterGroupId(IDbContext dbContext,
        Guid counterGroupId, IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        CounterGroup counterGroup = await dbContext.Set<CounterGroup>()
            .Include(e => e.Annotation)
            .ThenInclude(e => e.SlideImage)
            .FirstOrDefaultAsync(e => e.Id == counterGroupId, cancellationToken);

        if (counterGroup == null)
        {
            string message = stringLocalizer["APPLICATION.COUNTERGROUPS.NOT_FOUND", counterGroupId];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }

        return new AnnotationPermissions(counterGroup.Annotation.SlideImage, claimsPrincipalProvider.Current.UserId);
    }
}