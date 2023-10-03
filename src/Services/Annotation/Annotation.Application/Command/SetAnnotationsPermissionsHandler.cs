using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Aggregate;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class SetAnnotationPermissions : IRequest<AnnotationPermissionsDto>
{
    public SetAnnotationPermissions(AnnotationPermissionsDto dto)
    {
        Dto = dto;
    }

    public AnnotationPermissionsDto Dto { get; }
}

public class SetAnnotationPermissionsHandler : IRequestHandler<SetAnnotationPermissions, AnnotationPermissionsDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IMapper _mapper;

    public SetAnnotationPermissionsHandler(IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer, IMapper mapper)
    {
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _stringLocalizer = stringLocalizer;
        _mapper = mapper;
    }

    public async Task<AnnotationPermissionsDto> Handle(SetAnnotationPermissions request,
        CancellationToken cancellationToken)
    {
        AnnotationPermissions annotationPermission = await BusinessRepository.GetGlobalAnnotationPermissionsBySlideImageId(
            _annotationDbContext,
            request.Dto.SlideImageId, _claimsPrincipalProvider, _stringLocalizer, cancellationToken);

        if (annotationPermission.CanChangeAccess)
        {
            annotationPermission.SlideImage.Permission = request.Dto.Permission;

            _annotationDbContext.Entry(annotationPermission.SlideImage, EntityState.Modified);
            await _annotationDbContext.SaveChangesAsync(cancellationToken);

            var annotationPermissions = new AnnotationPermissions(annotationPermission.SlideImage,
                _claimsPrincipalProvider.Current.UserId);

            return _mapper.Map<AnnotationPermissionsDto>(annotationPermissions);
        }

        string message = _stringLocalizer["APPLICATION.ANNOTATIONS.UNAUTHORIZED"];
        throw new MessageOnly(message).ToApiException(HttpStatusCode.Unauthorized);
    }
}