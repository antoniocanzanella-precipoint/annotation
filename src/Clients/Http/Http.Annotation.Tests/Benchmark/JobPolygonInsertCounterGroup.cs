/*using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Enums;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Benchmark;

[SimpleJob(RunStrategy.Throughput)]
public class JobPolygonInsertCounterGroup : ABenchmarkJob
{
    private readonly CounterGroupDto _counterGroupDto;
    private readonly AnnotationDto _shape5000Point;

    public JobPolygonInsertCounterGroup()
    {
        _counterGroupDto = new CounterGroupDto();
        _shape5000Point = CreateAnnotation(AnnotationType.Polygon, AnnotationVisibility.Private, _5000Point);

        _shape5000Point = AnnotationHttpClient.AnnotationClient
            .InsertAnnotation(_shape5000Point, UniqueSlideImageId).GetAwaiter()
            .GetResult().Data;

        _counterGroupDto.AnnotationId = _shape5000Point.Id.Value;
        _counterGroupDto.Label = "my label";
        _counterGroupDto.Description = "my Description";
    }

    private double[][] _5000Point => CreatePolygon(5000);

    [GlobalSetup(Target = nameof(Polygon_AddCounter_100))]
    public void GlobalSetup()
    {
        _counterGroupDto.Counters = CreateCoordinates(100);
    }

    [Benchmark]
    public Task Polygon_AddCounter_100()
    {
        var jobs = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(async () =>
            {
                await AnnotationHttpClient.AnnotationClient.InsertCounterGroup(_counterGroupDto);
            }));

        return Task.WhenAll(jobs);
    }

    [GlobalSetup(Target = nameof(Polygon_AddCounter_1000))]
    public void GlobalSetup_1000()
    {
        _counterGroupDto.Counters = CreateCoordinates(1000);
    }

    [Benchmark]
    public Task Polygon_AddCounter_1000()
    {
        var jobs = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(async () =>
            {
                await AnnotationHttpClient.AnnotationClient.InsertCounterGroup(_counterGroupDto);
            }));

        return Task.WhenAll(jobs);
    }

    [GlobalSetup(Target = nameof(Polygon_AddCounter_5000))]
    public void GlobalSetup_5000()
    {
        _counterGroupDto.Counters = CreateCoordinates(5000);
    }

    [Benchmark]
    public Task Polygon_AddCounter_5000()
    {
        var jobs = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(async () =>
            {
                await AnnotationHttpClient.AnnotationClient.InsertCounterGroup(_counterGroupDto);
            }));

        return Task.WhenAll(jobs);
    }

    [GlobalSetup(Target = nameof(Polygon_AddCounter_10000))]
    public void GlobalSetup_10000()
    {
        _counterGroupDto.Counters = CreateCoordinates(10000);
    }

    [Benchmark]
    public Task Polygon_AddCounter_10000()
    {
        var jobs = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(async () =>
            {
                await AnnotationHttpClient.AnnotationClient.InsertCounterGroup(_counterGroupDto);
            }));

        return Task.WhenAll(jobs);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        AnnotationHttpClient.Dispose();
    }
}*/

