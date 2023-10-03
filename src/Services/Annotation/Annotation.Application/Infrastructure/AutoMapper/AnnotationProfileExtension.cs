using NetTopologySuite.Geometries;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;

public static class AnnotationProfileExtension
{
    public static AnnotationShape TransformCoordinatesFromDto(this AnnotationShape entity,
        double[][] coordinatesDto, GeometryFactory geometryFactory)
    {
        var coordinates = new Coordinate[coordinatesDto.Length];
        for (var i = 0; i < coordinatesDto.Length; i++)
        {
            coordinates[i] = new Coordinate(coordinatesDto[i][0], coordinatesDto[i][1]);
        }

        entity.ConfigureGeometry(coordinates, geometryFactory);
        return entity;
    }

    public static AnnotationShape AddCoordinatesFromDto(this AnnotationShape entity,
        double[][] coordinatesDto, int indexToSplit, AnnotationType annotationType, GeometryFactory geometryFactory)
    {
        var coordinates = new Coordinate[coordinatesDto.Length];
        for (var i = 0; i < coordinatesDto.Length; i++)
        {
            coordinates[i] = new Coordinate(coordinatesDto[i][0], coordinatesDto[i][1]);
        }

        Coordinate[] arrayMerge;

        if (annotationType == AnnotationType.Polyline && indexToSplit < 0)
        {
            arrayMerge = coordinates.Concat(entity.Shape.Coordinates).ToArray();
        }
        else if (indexToSplit >= entity.Shape.Coordinates.Length)
        {
            arrayMerge = entity.Shape.Coordinates.Concat(coordinates).ToArray();
        }
        else
        {
            IEnumerable<Coordinate> array1 = entity.Shape.Coordinates.Take(indexToSplit + 1);
            IEnumerable<Coordinate> array2 = entity.Shape.Coordinates.Skip(indexToSplit + 1);
            arrayMerge = array1.Concat(coordinates).Concat(array2).ToArray();
        }

        entity.ConfigureGeometry(arrayMerge, geometryFactory);

        return entity;
    }
}