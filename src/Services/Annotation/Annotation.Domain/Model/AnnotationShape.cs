using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Domain.Model;

public class AnnotationShape : AEntityAuditable
{
    public AnnotationShape()
    {
        CounterGroups = new HashSet<CounterGroup>();
    }

    public string Label { get; set; }
    public string Description { get; set; }
    public bool IsVisible { get; set; } = true;
    public double? Confidence { get; set; }
    public Geometry Shape { get; set; }
    public int[] Color { get; set; }
    public AnnotationType Type { get; set; }
    public AnnotationVisibility Visibility { get; set; } = AnnotationVisibility.Private;
    public Guid SlideImageId { get; set; }
    public Guid? ImportId { get; set; }
    public Guid? FolderId { get; set; }
    public ICollection<CounterGroup> CounterGroups { get; set; }
    public SlideImage SlideImage { get; set; }
    public Import Import { get; set; }
    public Folder Folder { get; set; }

    public void ConfigureGeometry(Coordinate[] coordinateList, GeometryFactory geometryFactory)
    {
        Shape = Type switch
        {
            AnnotationType.Point => geometryFactory.CreatePoint(coordinateList[0]),
            AnnotationType.Marker => geometryFactory.CreateMultiPoint(coordinateList.Select(t => new Point(t))
                .ToArray()),
            AnnotationType.Line => geometryFactory.CreateLineString(coordinateList),
            AnnotationType.Circle => geometryFactory.CreateMultiPoint(coordinateList.Select(t => new Point(t))
                .ToArray()),
            AnnotationType.Rectangular => geometryFactory.CreatePolygon(coordinateList),
            AnnotationType.Polygon => geometryFactory.CreatePolygon(coordinateList),
            AnnotationType.Polyline => geometryFactory.CreateLineString(coordinateList),
            AnnotationType.Grid => geometryFactory.CreateMultiPoint(coordinateList.Select(t => new Point(t))
                .ToArray()),
            _ => geometryFactory.CreateLineString(coordinateList)
        };
    }

    public void UpdateCoordinate(double x, double y, int index, GeometryFactory geometryFactory)
    {
        Shape = Type switch
        {
            AnnotationType.Point => geometryFactory.CreatePoint(new Coordinate(x, y)),
            AnnotationType.Marker => geometryFactory.CreateMultiPoint(UpdatePointsList(x, y, index)),
            AnnotationType.Line => geometryFactory.CreateMultiPoint(UpdatePointsList(x, y, index)),
            AnnotationType.Circle => geometryFactory.CreateMultiPoint(UpdatePointsList(x, y, index)),
            AnnotationType.Rectangular => geometryFactory.CreatePolygon(UpdateCoordinatesList(x, y, index)),
            AnnotationType.Polygon => geometryFactory.CreatePolygon(UpdateCoordinatesList(x, y, index)),
            AnnotationType.Polyline => geometryFactory.CreateLineString(UpdateCoordinatesList(x, y, index)),
            _ => geometryFactory.CreateLineString(UpdateCoordinatesList(x, y, index))
        };
    }

    public void DeleteCoordinate(int index, GeometryFactory geometryFactory)
    {
        List<Coordinate> coordinates = Shape.Coordinates.ToList();
        coordinates.RemoveAt(index);

        Shape = Type switch
        {
            AnnotationType.Polygon => geometryFactory.CreatePolygon(coordinates.ToArray()),
            AnnotationType.Polyline => geometryFactory.CreateLineString(coordinates.ToArray()),
            _ => geometryFactory.CreateLineString(coordinates.ToArray())
        };
    }

    public Polygon GetPolygonFromCircle(int coefficient, GeometryFactory geometryFactory)
    {
        if (Type != AnnotationType.Circle)
        {
            throw new InvalidOperationException(
                $"This method can be called only on annotation of type {AnnotationType.Circle}");
        }

        var gsf = new GeometricShapeFactory
        {
            Centre = Shape.Coordinates[0],
            Size = GetRadius(),
            NumPoints = (int) (coefficient * GetRadius())
        };

        Polygon circle = gsf.CreateCircle();

        return geometryFactory.CreatePolygon(circle.Coordinates);
    }

    public double GetRadius()
    {
        return
            Type == AnnotationType.Circle
                ? Shape.Coordinates[0].Distance(Shape.Coordinates[1])
                : 0.0;
    }

    private Coordinate[] UpdateCoordinatesList(double x, double y, int index)
    {
        List<Coordinate> coordinates = Shape.Coordinates.ToList();
        Coordinate coordinate = coordinates.ElementAt(index);

        coordinate.X = x;
        coordinate.Y = y;

        if (Type is AnnotationType.Polygon or AnnotationType.Rectangular &&
            (index == 0 || index == Shape.Coordinates.Length))
        {
            coordinates[0] = coordinate;
            coordinates[Shape.Coordinates.Length - 1] = coordinate;
        }
        else
        {
            coordinates[index] = coordinate;
        }

        return coordinates.ToArray();
    }

    private Point[] UpdatePointsList(double x, double y, int index)
    {
        return UpdateCoordinatesList(x, y, index)
            .Select(t => new Point(t)).ToArray();
    }
}