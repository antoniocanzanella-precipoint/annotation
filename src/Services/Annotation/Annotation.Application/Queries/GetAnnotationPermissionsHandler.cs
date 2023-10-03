using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Aggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Queries;

public class GetAnnotationPermissions : IRequest<AnnotationPermissionsDto>
{
    public GetAnnotationPermissions(Guid slideImageId)
    {
        SlideImageId = slideImageId;
    }

    public Guid SlideImageId { get; }
}

public class GetAnnotationPermissionsHandler : IRequestHandler<GetAnnotationPermissions, AnnotationPermissionsDto>
{
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public GetAnnotationPermissionsHandler(IDbContext dbContext, IClaimsPrincipalProvider claimsPrincipalProvider,
        IStringLocalizer stringLocalizer, IMapper mapper)
    {
        _dbContext = dbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _stringLocalizer = stringLocalizer;
        _mapper = mapper;
    }

    public async Task<AnnotationPermissionsDto> Handle(GetAnnotationPermissions request,
        CancellationToken cancellationToken)
    {
        AnnotationPermissions permission = await BusinessRepository.GetGlobalAnnotationPermissionsBySlideImageId(_dbContext,
            request.SlideImageId,
            _claimsPrincipalProvider, _stringLocalizer, cancellationToken);

        return _mapper.Map<AnnotationPermissionsDto>(permission);
    }
}