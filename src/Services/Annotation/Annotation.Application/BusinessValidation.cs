using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Authorization;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Domain;
using PreciPoint.Ims.Services.Annotation.Domain.Aggregate;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application;

public static class BusinessValidation
{
    public static async Task<AnnotationShape> CheckIfAnnotationExist(IAnnotationQueries annotationQueries,
        Guid annotationId, IStringLocalizer stringLocalizer, CancellationToken cancellationToken)
    {
        AnnotationShape annotation = await annotationQueries.GetAnnotationById(annotationId, cancellationToken);
        if (annotation == null)
        {
            string message = stringLocalizer["APPLICATION.ANNOTATIONS.NOT_FOUND", annotationId];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }

        return annotation;
    }

    public static void CheckUserWritePermission(AnnotationShape annotation,
        IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer)
    {
        if (claimsPrincipalProvider.Current.HasRole(Roles.ManageForeignAnnotations))
        {
            return;
        }

        string message;

        if (DomainConstants.AnonymousId == claimsPrincipalProvider.Current.UserId)
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.UNAUTHORIZED"];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
        }

        switch (annotation.Visibility)
        {
            case AnnotationVisibility.Private:
                if (claimsPrincipalProvider.Current.UserId != annotation.CreatedBy)
                {
                    message = stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_MANAGE",
                        annotation.Visibility, claimsPrincipalProvider.Current.UserId, annotation.Id];
                    throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
                }

                break;

            case AnnotationVisibility.ReadOnly:
                if (claimsPrincipalProvider.Current.UserId != annotation.CreatedBy)
                {
                    message = stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_MANAGE",
                        annotation.Visibility, claimsPrincipalProvider.Current.UserId, annotation.Id];
                    throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
                }

                break;

            case AnnotationVisibility.Public:
            case AnnotationVisibility.Editable:
                break;

            default:
                message = stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_MANAGE", annotation.Visibility,
                    claimsPrincipalProvider.Current.UserId, annotation.Id];
                throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
        }
    }

    public static bool CheckUserWritePermissionBool(AnnotationShape annotation,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        if (claimsPrincipalProvider.Current.HasRole(Roles.ManageForeignAnnotations))
        {
            return true;
        }

        if (DomainConstants.AnonymousId == claimsPrincipalProvider.Current.UserId)
        {
            return false;
        }

        switch (annotation.Visibility)
        {
            case AnnotationVisibility.Private:
                if (claimsPrincipalProvider.Current.UserId == annotation.CreatedBy)
                {
                    return true;
                }

                break;

            case AnnotationVisibility.ReadOnly:
                if (claimsPrincipalProvider.Current.UserId == annotation.CreatedBy)
                {
                    return true;
                }

                break;

            case AnnotationVisibility.Public:
            case AnnotationVisibility.Editable:
                return true;

            default:
                return false;
        }

        return false;
    }

    public static void CheckUserDeletePermission(AnnotationShape annotation,
        IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer)
    {
        if (annotation.SlideImage is null)
        {
            throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name}: Slide Image cannot be null");
        }

        if (claimsPrincipalProvider.Current.HasRole(Roles.DeleteForeignAnnotations))
        {
            return;
        }

        if (claimsPrincipalProvider.Current.UserId == annotation.CreatedBy)
        {
            return;
        }

        if (claimsPrincipalProvider.Current.UserId == annotation.SlideImage.OwnedBy)
        {
            return;
        }

        LocalizedString message = stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_DELETE", claimsPrincipalProvider.Current.UserId,
            annotation.Id];
        throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
    }

    public static bool CheckUserDeletePermissionBool(AnnotationShape annotation,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        if (annotation.SlideImage is null)
        {
            return false;
        }

        if (claimsPrincipalProvider.Current.HasRole(Roles.DeleteForeignAnnotations))
        {
            return true;
        }

        if (claimsPrincipalProvider.Current.UserId == annotation.CreatedBy)
        {
            return true;
        }

        if (claimsPrincipalProvider.Current.UserId == annotation.SlideImage.OwnedBy)
        {
            return true;
        }

        return false;
    }

    public static void CheckUserCanChangeVisibility(AnnotationShape annotation,
        IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer)
    {
        if (annotation.SlideImage is null)
        {
            throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name}: Slide Image cannot be null");
        }

        if (claimsPrincipalProvider.Current.HasRole(Roles.ManageForeignAnnotations))
        {
            return;
        }

        if (claimsPrincipalProvider.Current.UserId == annotation.CreatedBy)
        {
            return;
        }

        LocalizedString message = stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_MANAGE",
            annotation.Visibility, claimsPrincipalProvider.Current.UserId, annotation.Id];
        throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
    }

    public static bool CheckUserCanChangeVisibilityBool(AnnotationShape annotation,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        if (annotation.SlideImage is null)
        {
            return false;
        }

        if (claimsPrincipalProvider.Current.HasRole(Roles.ManageForeignAnnotations))
        {
            return true;
        }

        if (claimsPrincipalProvider.Current.UserId == annotation.CreatedBy)
        {
            return true;
        }

        return false;
    }

    public static void CheckCoordinates(double[][] dto, IStringLocalizer stringLocalizer)
    {
        if (dto == null || dto.Length == 0)
        {
            string message = stringLocalizer["APPLICATION.ANNOTATIONS.COORDINATE_NULL_OR_EMPTY"];
            throw new MessageOnly(message).ToApiException();
        }
    }

    public static void CheckIfAnnotationCanReceiveUpdate(AnnotationShape entity, int splitIndex,
        IStringLocalizer stringLocalizer)
    {
        string message;

        if (entity.Type is AnnotationType.Marker or AnnotationType.Point or AnnotationType.Circle
            or AnnotationType.Rectangular or AnnotationType.Line)
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_ADD_COORDINATE", entity.Id, entity.Type,
                splitIndex,
                entity.Shape.Coordinates.Length - 2];
            throw new MessageOnly(message).ToApiException();
        }

        if (entity.Type == AnnotationType.Polygon &&
            (splitIndex < 0 || splitIndex >= entity.Shape.Coordinates.Length - 1))
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.WRONG_INDEX", entity.Id, entity.Type, splitIndex,
                entity.Shape.Coordinates.Length - 2];
            throw new MessageOnly(message).ToApiException();
        }

        if (entity.Type == AnnotationType.Polyline && (splitIndex < -1 || splitIndex > entity.Shape.Coordinates.Length))
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.WRONG_INDEX", entity.Id, entity.Type, splitIndex,
                entity.Shape.Coordinates.Length - 2];
            throw new MessageOnly(message).ToApiException();
        }
    }

    public static void CheckIfAnnotationCoordinateCanBeDeleted(AnnotationShape entity, int index,
        IStringLocalizer stringLocalizer)
    {
        string message;

        if (entity.Type is AnnotationType.Marker or AnnotationType.Point or AnnotationType.Circle
            or AnnotationType.Rectangular or AnnotationType.Line)
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_DELETE_COORDINATE", entity.Id, entity.Type];
            throw new MessageOnly(message).ToApiException();
        }

        if (entity.Type == AnnotationType.Polygon && (index < 0 || index >= entity.Shape.Coordinates.Length - 1))
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.WRONG_INDEX", entity.Id, entity.Type, index,
                entity.Shape.Coordinates.Length - 2];
            throw new MessageOnly(message).ToApiException();
        }

        if (entity.Type == AnnotationType.Polyline && (index < -1 || index > entity.Shape.Coordinates.Length))
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.WRONG_INDEX", entity.Id, entity.Type, index,
                entity.Shape.Coordinates.Length - 2];
            throw new MessageOnly(message).ToApiException();
        }
    }

    public static void CheckCoordinatesAndAnnotationType(AnnotationShape entity, double[][] coordinatesDto,
        IStringLocalizer stringLocalizer)
    {
        string message;

        if (entity.Type is AnnotationType.Point && coordinatesDto.Length != 1)
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.NEED_ONE_COORDINATE", entity.Type];
            throw new MessageOnly(message).ToApiException();
        }

        if (entity.Type is AnnotationType.Marker or AnnotationType.Line && coordinatesDto.Length != 2)
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.NEED_MULTI_COORDINATE", entity.Type, 2];
            throw new MessageOnly(message).ToApiException();
        }

        if (entity.Type == AnnotationType.Circle && coordinatesDto.Length != 2)
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.NEED_MULTI_COORDINATE", entity.Type, 2];
            throw new MessageOnly(message).ToApiException();
        }

        if (entity.Type == AnnotationType.Rectangular && coordinatesDto.Length != 5)
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.NEED_MULTI_COORDINATE", entity.Type, 5];
            throw new MessageOnly(message).ToApiException();
        }

        if (entity.Type == AnnotationType.Polygon &&
            (coordinatesDto[0][0] != coordinatesDto[^1][0] || coordinatesDto[0][1] != coordinatesDto[^1][1]))
        {
            message = stringLocalizer["APPLICATION.ANNOTATIONS.FIRST_AND_LAST_MISMATCH", entity.Type,
                coordinatesDto[0][0], coordinatesDto[0][1], coordinatesDto[^1][0], coordinatesDto[^1][1]];
            throw new MessageOnly(message).ToApiException();
        }
    }

    public static void CheckIfAnnotationCanHaveCounters(AnnotationShape entity, IStringLocalizer stringLocalizer)
    {
        if (!entity.Type.Equals(AnnotationType.Circle) && !entity.Type.Equals(AnnotationType.Polygon) &&
            !entity.Type.Equals(AnnotationType.Rectangular) && !entity.Type.Equals(AnnotationType.Grid))
        {
            string message = stringLocalizer["APPLICATION.ANNOTATIONS.CANNOT_HAVE_COUNTERS", entity.Id, entity.Type];
            throw new MessageOnly(message).ToApiException();
        }
    }

    public static async Task<CounterGroup> CheckIfAnnotationCounterGroupExist(ICountingQueries countingQueries,
        Guid? counterGroupId, IStringLocalizer stringLocalizer, CancellationToken cancellationToken)
    {
        if (counterGroupId.HasValue == false)
        {
            string message = stringLocalizer["APPLICATION.COUNTERGROUPS.NOT_NULL_ID"];
            throw new MessageOnly(message).ToApiException();
        }

        CounterGroup counterGroup = await countingQueries.GetCounterGroupById(counterGroupId.Value, cancellationToken);
        if (counterGroup == null)
        {
            string message = stringLocalizer["APPLICATION.COUNTERGROUPS.NOT_FOUND", counterGroupId.Value];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }

        return counterGroup;
    }

    public static async Task<Counter> CheckIfCounterExist(ICountingQueries countingQueries, Guid counterId,
        IStringLocalizer stringLocalizer, CancellationToken cancellationToken)
    {
        Counter counter = await countingQueries.GetCounterById(counterId, cancellationToken);

        if (counter != null)
        {
            return counter;
        }

        string message = stringLocalizer["APPLICATION.COUNTERGROUPS.NOT_FOUND", counterId];
        throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
    }

    public static bool CheckIfShapeContains(AnnotationShape annotation, Counter counter, Polygon polygon)
    {
        if (annotation.Type != AnnotationType.Circle)
        {
            return annotation.Shape.Contains(counter.Shape);
        }

        return polygon.Contains(counter.Shape);
    }

    public static void CheckAnnotationIdAndSlideImageId(AnnotationShape annotation, Guid slideImageIdFromRequest,
        Guid userId, IStringLocalizer stringLocalizer)
    {
        if (annotation.SlideImageId != slideImageIdFromRequest)
        {
            string message = stringLocalizer["APPLICATION.ANNOTATIONS.WRONG_COMBO_ID_AND_SLIDEIMAGE_ID", userId,
                annotation.Id, annotation.SlideImageId];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }
    }

    public static async Task CheckUserReadPermissionBySlideImageId(IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider, Guid slideImageId, IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        AnnotationPermissions annotationPermission = await BusinessRepository.GetGlobalAnnotationPermissionsBySlideImageId(
            annotationDbContext,
            slideImageId, claimsPrincipalProvider, stringLocalizer, cancellationToken);

        if (claimsPrincipalProvider.Current.HasRole(Roles.QueryAnnotations) && annotationPermission.CanView)
        {
            return;
        }

        string message = stringLocalizer["APPLICATION.ANNOTATIONS.UNAUTHORIZED"];
        throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
    }

    public static async Task CheckUserReadPermissionByAnnotationId(IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider, Guid annotationId, IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        AnnotationPermissions annotationPermission =
            await BusinessRepository.GetGlobalAnnotationPermissionsByAnnotationId(annotationDbContext, annotationId,
                claimsPrincipalProvider, stringLocalizer, cancellationToken);

        if (annotationPermission.CanView)
        {
            return;
        }

        string message = stringLocalizer["APPLICATION.ANNOTATIONS.UNAUTHORIZED"];
        throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
    }

    public static async Task CheckUserReadPermissionByCounterGroupId(IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider, Guid counterGroupId, IStringLocalizer stringLocalizer,
        CancellationToken cancellationToken)
    {
        AnnotationPermissions annotationPermission =
            await BusinessRepository.GetGlobalAnnotationPermissionsByCounterGroupId(annotationDbContext, counterGroupId,
                claimsPrincipalProvider, stringLocalizer, cancellationToken);

        if (annotationPermission.CanView)
        {
            return;
        }

        string message = stringLocalizer["APPLICATION.ANNOTATIONS.UNAUTHORIZED"];
        throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
    }

    private static bool IsColorInRange(int color)
    {
        return color is >= 0 and <= 255;
    }

    public static void CheckColor(int[] color, IStringLocalizer stringLocalizer)
    {
        if (color == null || color.Length == 0)
        {
            return;
        }

        string message;
        if (color.Length is < 3 or > 4)
        {
            message = stringLocalizer["APPLICATION.COLOR_FORMAT_INVALID", color.Length];
            throw new MessageOnly(message).ToApiException();
        }

        if (!IsColorInRange(color[0]))
        {
            message = stringLocalizer["APPLICATION.COLOR_OUT_OF_RANGE", color[0]];
            throw new MessageOnly(message).ToApiException();
        }

        if (!IsColorInRange(color[1]))
        {
            message = stringLocalizer["APPLICATION.COLOR_OUT_OF_RANGE", color[1]];
            throw new MessageOnly(message).ToApiException();
        }

        if (!IsColorInRange(color[2]))
        {
            message = stringLocalizer["APPLICATION.COLOR_OUT_OF_RANGE", color[2]];
            throw new MessageOnly(message).ToApiException();
        }

        if (color.Length == 4 && !IsColorInRange(color[3]))
        {
            message = stringLocalizer["APPLICATION.COLOR_OUT_OF_RANGE", color[3]];
            throw new MessageOnly(message).ToApiException();
        }
    }
}