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

public class DeleteCounter : IRequest<DeleteOperationDto>
{
    public DeleteCounter(Guid counterId)
    {
        CounterId = counterId;
    }

    public Guid CounterId { get; }
}

public class DeleteCounterHandler : IRequestHandler<DeleteCounter, DeleteOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ICountingQueries _countingQueries;
    private readonly IStringLocalizer _stringLocalizer;

    public DeleteCounterHandler(ICountingQueries countingQueries, IStringLocalizer stringLocalizer,
        IDbContext annotationDbContext, IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _countingQueries = countingQueries;
        _stringLocalizer = stringLocalizer;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public async Task<DeleteOperationDto> Handle(DeleteCounter request,
        CancellationToken cancellationToken = default)
    {
        Counter counterToUpdate = await BusinessValidation.CheckIfCounterExist(_countingQueries, request.CounterId,
            _stringLocalizer, cancellationToken);

        BusinessValidation.CheckUserWritePermission(counterToUpdate.CounterGroup.Annotation, _claimsPrincipalProvider,
            _stringLocalizer);

        _annotationDbContext.Set<Counter>().Remove(counterToUpdate);

        return new DeleteOperationDto { NumberOfEntityRemoved = await _annotationDbContext.SaveChangesAsync(cancellationToken) };
    }
}