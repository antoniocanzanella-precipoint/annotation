using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Queries;

public class GetAnnotationCounterGroups : IRequest<IReadOnlyList<CounterGroupDto>>
{
    public GetAnnotationCounterGroups(Guid annotationId)
    {
        AnnotationId = annotationId;
    }

    public Guid AnnotationId { get; }
}

public class
    GetAnnotationCounterGroupsHandler : IRequestHandler<GetAnnotationCounterGroups, IReadOnlyList<CounterGroupDto>>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ICountingQueries _countingQueries;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public GetAnnotationCounterGroupsHandler(ICountingQueries countingQueries, IMapper mapper,
        IStringLocalizer stringLocalizer, IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider)
    {
        _countingQueries = countingQueries;
        _mapper = mapper;
        _stringLocalizer = stringLocalizer;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
    }

    public async Task<IReadOnlyList<CounterGroupDto>> Handle(GetAnnotationCounterGroups request,
        CancellationToken cancellationToken)
    {
        await BusinessValidation.CheckUserReadPermissionByAnnotationId(_annotationDbContext, _claimsPrincipalProvider,
            request.AnnotationId, _stringLocalizer, cancellationToken);

        IEnumerable<CounterGroup> counterGroupList =
            await _countingQueries.GetAnnotationCountersNoTrack(request.AnnotationId, cancellationToken);

        return _mapper.Map<IReadOnlyList<CounterGroupDto>>(counterGroupList);
    }
}