using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Queries;

public class GetAnnotationById : IRequest<AnnotationDto>
{
    public GetAnnotationById(Guid annotationId)
    {
        AnnotationId = annotationId;
    }

    public Guid AnnotationId { get; }
}

public class GetAnnotationByIdHandler : IRequestHandler<GetAnnotationById, AnnotationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public GetAnnotationByIdHandler(IAnnotationQueries annotationQueries, IStringLocalizer stringLocalizer,
        IMapper mapper, IDbContext annotationDbContext, IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _stringLocalizer = stringLocalizer;
        _mapper = mapper;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _annotationQueries = annotationQueries;
    }

    public async Task<AnnotationDto> Handle(GetAnnotationById request, CancellationToken cancellationToken)
    {
        await BusinessValidation.CheckUserReadPermissionByAnnotationId(_annotationDbContext, _claimsPrincipalProvider,
            request.AnnotationId, _stringLocalizer, cancellationToken);

        AnnotationShape annotation = await _annotationQueries.GetAnnotationByIdNoTrack(request.AnnotationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (annotation == null)
        {
            string message = _stringLocalizer["APPLICATION.ANNOTATIONS.NOT_FOUND", request.AnnotationId];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }

        return _mapper.Map<AnnotationDto>(annotation);
    }
}