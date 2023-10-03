using AutoMapper;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class Translate : IRequest<IReadOnlyList<AnnotationDto>>
{
    public Translate(TranslateDto dto)
    {
        Dto = dto;
    }

    public TranslateDto Dto { get; }
}

public class TranslateHandler : IRequestHandler<Translate, IReadOnlyList<AnnotationDto>>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly AnnotationQueryFilter _annotationQueryFilter;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly ILogger<TranslateHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public TranslateHandler(IDbContext annotationDbContext, IMapper mapper,
        IClaimsPrincipalProvider claimsPrincipalProvider, ILogger<TranslateHandler> logger,
        IAnnotationQueries annotationQueries, AnnotationQueryFilter annotationQueryFilter,
        IStringLocalizer stringLocalizer)
    {
        _annotationDbContext = annotationDbContext;
        _mapper = mapper;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _logger = logger;
        _annotationQueries = annotationQueries;
        _annotationQueryFilter = annotationQueryFilter;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<IReadOnlyList<AnnotationDto>> Handle(Translate request, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await new TranslateValidator(_stringLocalizer).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw validationResult.ToApiException();
        }

        await TranslateAnnotations(request, cancellationToken);
        //if we go with raw sql this should be fast even without checking if annotations contains counters.
        await TranslateCounters(request, cancellationToken);

        await _annotationDbContext.SaveChangesAsync(cancellationToken);

        Expression<Func<AnnotationShape, bool>> filter = _annotationQueryFilter.GetAnnotationsFilter(request.Dto.AnnotationIds,
            _claimsPrincipalProvider.Current.UserId);

        List<AnnotationShape> annotationList = await _annotationQueries.GetAnnotationsNoTrack(filter).ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<AnnotationDto>>(annotationList);
    }

    private Task<int> TranslateAnnotations(Translate request, CancellationToken cancellationToken)
    {
        var filterUserCondition =
            $"\"CreatedBy\" = '{_claimsPrincipalProvider.Current.UserId}' or \"Visibility\" = '{AnnotationVisibility.Public.ToString().ToLowerInvariant()}'";

        var query =
            $"UPDATE ims.\"Annotations\" SET \"Shape\" = st_translate(\"Shape\", {request.Dto.DeltaX}, {request.Dto.DeltaY}) WHERE \"Id\" {GetIdsIntoString(request.Dto.AnnotationIds)} and ({filterUserCondition});";

        _logger.LogDebug(@"Translate annotation query: {TranslateAnnotationQuery}", query);

        return _annotationDbContext.ExecuteSqlRawAsync(query, cancellationToken);
    }

    private Task<int> TranslateCounters(Translate request, CancellationToken cancellationToken)
    {
        var filterUserCondition =
            $"annota.\"CreatedBy\" = '{_claimsPrincipalProvider.Current.UserId}' or annota.\"Visibility\" = '{AnnotationVisibility.Public.ToString().ToLowerInvariant()}'";

        string query =
            $"UPDATE ims.\"Counters\" SET \"Shape\" = st_translate(\"Shape\", {request.Dto.DeltaX}, {request.Dto.DeltaY}) " +
            $"WHERE \"GroupCounterId\" IN (select cg.\"Id\" from ims.\"CounterGroups\" as cg, ims.\"Annotations\" as annota where cg.\"AnnotationId\" = annota.\"Id\" and annota.\"Id\" {GetIdsIntoString(request.Dto.AnnotationIds)} and ({filterUserCondition}));";

        _logger.LogDebug(@"Translate counters query: {TranslateAnnotationQuery}", query);

        return _annotationDbContext.ExecuteSqlRawAsync(query, cancellationToken);
    }

    private string GetIdsIntoString(IReadOnlyList<Guid> ids)
    {
        if (ids.Count == 1)
        {
            return $"= '{ids[0]}'";
        }

        var strBuilder = new StringBuilder(ids.Count * 40);

        for (var i = 0; i < ids.Count - 1; i++)
        {
            strBuilder.Append($"'{ids[i]}', ");
        }

        strBuilder.Append($"'{ids[^1]}'");

        return $"in ({strBuilder})";
    }
}