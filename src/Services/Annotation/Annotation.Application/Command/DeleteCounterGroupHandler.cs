using MediatR;
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

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class DeleteCounterGroup : IRequest<DeleteOperationDto>
{
    public DeleteCounterGroup(Guid annotationCounterId)
    {
        AnnotationCounterId = annotationCounterId;
    }

    public Guid AnnotationCounterId { get; }
}

public class DeleteCounterGroupHandler : IRequestHandler<DeleteCounterGroup, DeleteOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ICountingQueries _countingQueries;
    private readonly IStringLocalizer _stringLocalizer;

    public DeleteCounterGroupHandler(ICountingQueries countingQueries, IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer)
    {
        _countingQueries = countingQueries;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<DeleteOperationDto> Handle(DeleteCounterGroup request,
        CancellationToken cancellationToken = default)
    {
        CounterGroup counterGroup = await _countingQueries.GetCounterGroupById(request.AnnotationCounterId, cancellationToken);

        if (counterGroup == null)
        {
            string message = _stringLocalizer["APPLICATION.COUNTERGROUPS.NOT_FOUND", request.AnnotationCounterId];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }

        BusinessValidation.CheckUserDeletePermission(counterGroup.Annotation, _claimsPrincipalProvider,
            _stringLocalizer);

        _annotationDbContext.Set<CounterGroup>().Remove(counterGroup);

        return new DeleteOperationDto { NumberOfEntityRemoved = await _annotationDbContext.SaveChangesAsync(cancellationToken) };
    }
}