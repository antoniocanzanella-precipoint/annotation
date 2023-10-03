using MediatR;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class DeleteAnnotation : IRequest<DeleteOperationDto>
{
    public DeleteAnnotation(Guid annotationId)
    {
        AnnotationId = annotationId;
    }

    public Guid AnnotationId { get; }
}

public class DeleteAnnotationHandler : IRequestHandler<DeleteAnnotation, DeleteOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IStringLocalizer _stringLocalizer;

    public DeleteAnnotationHandler(IAnnotationQueries annotationQueries, IStringLocalizer stringLocalizer,
        IDbContext annotationDbContext, IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _annotationQueries = annotationQueries;
        _stringLocalizer = stringLocalizer;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public async Task<DeleteOperationDto> Handle(DeleteAnnotation request,
        CancellationToken cancellationToken = default)
    {
        AnnotationShape annotation = await BusinessValidation.CheckIfAnnotationExist(_annotationQueries, request.AnnotationId,
            _stringLocalizer, cancellationToken);
        BusinessValidation.CheckUserDeletePermission(annotation, _claimsPrincipalProvider, _stringLocalizer);

        _annotationDbContext.Set<AnnotationShape>().Remove(annotation);
        int result = await _annotationDbContext.SaveChangesAsync(cancellationToken);

        return new DeleteOperationDto { NumberOfEntityRemoved = result };
    }
}