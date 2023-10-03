using AutoMapper;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Exceptions;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper.ValueResolvers;

public class CanManageAnnotationVisibilityByValueResolver : IValueResolver<AnnotationShape, AnnotationDto, bool>
{
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IStringLocalizer _stringLocalizer;

    public CanManageAnnotationVisibilityByValueResolver(IClaimsPrincipalProvider claimsPrincipalProvider,
        IStringLocalizer stringLocalizer)
    {
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _stringLocalizer = stringLocalizer;
    }

    public bool Resolve(AnnotationShape source, AnnotationDto destination, bool destMember, ResolutionContext context)
    {
        try
        {
            BusinessValidation.CheckUserCanChangeVisibility(source, _claimsPrincipalProvider, _stringLocalizer);
            return true;
        }
        catch (ApiException)
        {
            return false;
        }
    }
}