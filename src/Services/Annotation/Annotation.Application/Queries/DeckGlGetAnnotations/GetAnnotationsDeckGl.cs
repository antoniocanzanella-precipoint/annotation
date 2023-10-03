using MediatR;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Queries.DeckGlGetAnnotations;

public class GetAnnotationsDeckGl : IRequest<BinaryDataWithHeaderDto>
{
    public GetAnnotationsDeckGl(Guid slideImageId)
    {
        SlideImageId = slideImageId;
    }

    public Guid SlideImageId { get; }
}