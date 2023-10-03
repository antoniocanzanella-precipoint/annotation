using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Extensions;

public static class AnnotationTypeExtensions
{
    public static DeckGlLayerType ToDeckGlLayer(this AnnotationType type)
    {
        switch (type)
        {
            case AnnotationType.Marker:
                return DeckGlLayerType.Composite;
            case AnnotationType.Point:
            case AnnotationType.Circle:
                return DeckGlLayerType.Scatterplot;
            case AnnotationType.Line:
            case AnnotationType.Polyline:
                return DeckGlLayerType.Path;
            case AnnotationType.Rectangular:
            case AnnotationType.Polygon:
                return DeckGlLayerType.Polygon;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}