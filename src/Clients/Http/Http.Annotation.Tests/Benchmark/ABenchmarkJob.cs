using Microsoft.Extensions.Configuration;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Config;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Extensions;
using PreciPoint.Ims.Core.DataTransfer.Factories;
using PreciPoint.Ims.Utils.TestUtils.Config;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Benchmark;

public class ABenchmarkJob
{
    protected const string Folder = "./files";
    protected readonly AnnotationTestConfig Configuration;
    protected AnnotationHttpClient AnnotationHttpClient;

    public ABenchmarkJob()
    {
        Configuration = new JsonSettings().Configuration.Get<AnnotationTestConfig>();
        var httpClientFactory = new HttpClientFactory(Configuration.SecuritySystem);
        var apiUrl = $"{Configuration.AnnotationHost}/api";

        AnnotationHttpClient = new AnnotationHttpClient(httpClientFactory.CreateUserHttpClient(Configuration), apiUrl);
    }

/*
    private const double Maximum = 311.9573487;
    private const double Minimum = -311.36276168;
    
    protected AnnotationDto CreateAnnotation(AnnotationType type,
        AnnotationVisibility visibility, double[][] coordinates)
    {
        return new AnnotationDto
        {
            Label = $"a label - {type} - {visibility}",
            Description = $"a description  - {type} - {visibility}",
            AnnotationType = type,
            Visibility = visibility,
            Coordinates = coordinates
        };
    }

    protected double[][] CreatePolygon(int numOfPoint)
    {
        var geometricShapeFactory = new GeometricShapeFactory
        {
            Centre = new Coordinate(0, 0),
            Size = numOfPoint / 10,
            NumPoints = numOfPoint
        };

        var coordinateArray = geometricShapeFactory.CreateCircle().Coordinates;

        var coordinates = new double[coordinateArray.Length][];

        for (var i = 0; i < coordinateArray.Length; i++)
            coordinates[i] = new double[2]
            {
                coordinateArray[i].X, coordinateArray[i].Y
            };

        return coordinates;
    }

    protected double[][] CreateCoordinates(int numberOfElement)
    {
        var xCoordinates =
            Enumerable.Range(0, numberOfElement)
                .Select(_ => new Random().NextDouble() * (Maximum - Minimum) + Minimum).ToArray();
        var yCoordinates =
            Enumerable.Range(0, numberOfElement)
                .Select(_ => new Random().NextDouble() * (Maximum - Minimum) + Minimum).ToArray();

        var result = new double[numberOfElement][];

        for (var i = 0; i < numberOfElement; i++) result[i] = new double[2] {xCoordinates[i], yCoordinates[i]};

        return result;
    }*/
}