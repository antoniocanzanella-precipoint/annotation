using AutoMapper;
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

namespace PreciPoint.Ims.Services.Annotation.Application.Queries;

public class GetAnnotationCounterGroupById : IRequest<CounterGroupDto>
{
    public GetAnnotationCounterGroupById(Guid counterGroupId)
    {
        CounterGroupId = counterGroupId;
    }

    public Guid CounterGroupId { get; }
}

public class GetAnnotationCounterGroupByIdHandler : IRequestHandler<GetAnnotationCounterGroupById, CounterGroupDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ICountingQueries _countingQueries;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public GetAnnotationCounterGroupByIdHandler(ICountingQueries countingQueries, IMapper mapper,
        IStringLocalizer stringLocalizer, IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _countingQueries = countingQueries;
        _mapper = mapper;
        _stringLocalizer = stringLocalizer;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public async Task<CounterGroupDto> Handle(GetAnnotationCounterGroupById request,
        CancellationToken cancellationToken)
    {
        await BusinessValidation.CheckUserReadPermissionByCounterGroupId(_annotationDbContext, _claimsPrincipalProvider,
            request.CounterGroupId, _stringLocalizer, cancellationToken);

        CounterGroup counterGroup =
            await _countingQueries.GetCounterGroupByIdNoTrack(request.CounterGroupId, cancellationToken);

        if (counterGroup == null)
        {
            string message = _stringLocalizer["APPLICATION.COUNTERGROUPS.NOT_FOUND", request.CounterGroupId];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.NotFound);
        }

        return _mapper.Map<CounterGroupDto>(counterGroup);
    }
}