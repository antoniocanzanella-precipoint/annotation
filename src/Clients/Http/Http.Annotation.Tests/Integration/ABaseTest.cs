using Microsoft.Extensions.Configuration;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Config;
using PreciPoint.Ims.Core.DataTransfer.Factories;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Utils.TestUtils.Config;
using System;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Integration;

public abstract class ABaseTest
{
    protected const string Folder = "./files";
    protected readonly AnnotationTestConfig Configuration;
    protected readonly HttpClientFactory HttpClientFactory;

    protected ABaseTest()
    {
        Configuration = new JsonSettings().Configuration.Get<AnnotationTestConfig>();
        HttpClientFactory = new HttpClientFactory(Configuration.SecuritySystem);
    }

    protected AnnotationDto CreateAnnotation(AnnotationType type, AnnotationVisibility visibility)
    {
        return new AnnotationDto
        {
            Label = $"a label - {type} - {visibility}",
            Description = $"a description  - {type} - {visibility}",
            AnnotationType = type,
            Visibility = visibility
        };
    }

    protected CounterGroupDto CreateCounterGroup(Guid annotationId)
    {
        return new CounterGroupDto
        {
            AnnotationId = annotationId,
            Label = "a label",
            Description = "a description"
        };
    }

    protected async Task<AnnotationDto> CreateGenericPolygon(AnnotationHttpClient client,
        AnnotationVisibility visibility, Guid slideImageId)
    {
        AnnotationDto polygon = CreateAnnotation(AnnotationType.Polygon, visibility);
        var coordinatesDto =
            new double[7][]
            {
                new double[2] { 3.3, 3 }, new double[2] { 5, 2.5 }, new double[2] { 7.5, 2.5 }, new double[2] { 8, 4 }, new double[2] { 7, 6 },
                new double[2] { 6, 4 }, new double[2] { 3.3, 3 }
            };
        polygon.Coordinates = coordinatesDto;

        ApiResponse<AnnotationDto> response = await client.AnnotationClient.InsertAnnotation(polygon, slideImageId);

        return response.Data;
    }

    protected async Task<CounterGroupDto> CreateGenericCounter(Guid annotationId, AnnotationHttpClient client)
    {
        CounterGroupDto annotationCounterDto = CreateCounterGroup(annotationId);
        var counters =
            new double[3][] { new double[2] { 4, 3 }, new double[2] { 4, 4 }, new double[2] { 4, 10 } };
        annotationCounterDto.Counters = counters;

        ApiResponse<CounterGroupDto> response = await client.AnnotationClient.InsertCounterGroup(annotationCounterDto);

        return response.Data;
    }
}