using MediatR;
using Microsoft.EntityFrameworkCore;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Header;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Queries.DeckGlGetAnnotations;

public class GetAnnotationsDeckGlHandler : IRequestHandler<GetAnnotationsDeckGl, BinaryDataWithHeaderDto>
{
    private readonly IAnnotationQueries _annotationQueries;
    private readonly AnnotationQueryFilter _annotationQueryFilter;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IDeckGlAnnotationSerializer _serializer;

    public GetAnnotationsDeckGlHandler(IAnnotationQueries annotationQueries,
        AnnotationQueryFilter annotationQueryFilter,
        IClaimsPrincipalProvider claimsPrincipalProvider, IDeckGlAnnotationSerializer serializer)
    {
        _annotationQueries = annotationQueries;
        _annotationQueryFilter = annotationQueryFilter;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _serializer = serializer;
    }

    public async Task<BinaryDataWithHeaderDto> Handle(GetAnnotationsDeckGl request,
        CancellationToken cancellationToken)
    {
        Expression<Func<AnnotationShape, bool>> filter =
            _annotationQueryFilter.GetAnnotationsFilter(request.SlideImageId,
                _claimsPrincipalProvider.Current.UserId);

        List<AnnotationShape> annotationList = await _annotationQueries.GetAnnotationsNoTrack(filter)
            .ToListAsync(cancellationToken);

        var builder = new LayerHeaderBuilder(_claimsPrincipalProvider);
        BuildResult result = builder.Build(annotationList);

        var binaryData = new Memory<byte>(new byte[result.BinaryDataSize]);

        _serializer.Serialize(result.LayerHeaders,
            result.DeckGlLayers,
            binaryData);

        return new BinaryDataWithHeaderDto { Data = binaryData.ToArray(), Headers = result.LayerHeaders };
    }
}