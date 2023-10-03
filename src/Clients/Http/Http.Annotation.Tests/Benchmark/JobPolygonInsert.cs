﻿/*using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Enums;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Benchmark;

[SimpleJob(RunStrategy.Throughput)]
public class JobPolygonInsert : ABenchmarkJob
{
    private AnnotationDto _shape10000Point;
    private AnnotationDto _shape1000Point;
    private AnnotationDto _shape100Point;
    private AnnotationDto _shape5000Point;
    private double[][] _100Point => CreatePolygon(100);
    private double[][] _1000Point => CreatePolygon(1000);
    private double[][] _5000Point => CreatePolygon(5000);
    private double[][] _10000Point => CreatePolygon(10000);

    [GlobalSetup]
    public void GlobalSetup()
    {
        _shape100Point = CreateAnnotation(AnnotationType.Polygon, AnnotationVisibility.Private, _100Point);
        _shape1000Point = CreateAnnotation(AnnotationType.Polygon, AnnotationVisibility.Private, _1000Point);
        _shape5000Point = CreateAnnotation(AnnotationType.Polygon, AnnotationVisibility.Private, _5000Point);
        _shape10000Point = CreateAnnotation(AnnotationType.Polygon, AnnotationVisibility.Private, _10000Point);
    }

    [Benchmark]
    public Task InsertPolygon_100Point()
    {
        var jobs = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(async () =>
            {
                await AnnotationHttpClient.AnnotationClient.InsertAnnotation(_shape100Point,
                    UniqueSlideImageId);
            }));

        return Task.WhenAll(jobs);
    }

    [Benchmark]
    public Task InsertPolygon_1000Point()
    {
        var jobs = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(async () =>
            {
                await AnnotationHttpClient.AnnotationClient.InsertAnnotation(_shape1000Point,
                    UniqueSlideImageId);
            }));

        return Task.WhenAll(jobs);
    }

    [Benchmark]
    public Task InsertPolygon_5000Point()
    {
        var jobs = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(async () =>
            {
                await AnnotationHttpClient.AnnotationClient.InsertAnnotation(_shape5000Point,
                    UniqueSlideImageId);
            }));

        return Task.WhenAll(jobs);
    }

    [Benchmark]
    public Task InsertPolygon_10000Point()
    {
        var jobs = Enumerable.Range(0, 1).Select(_ =>
            Task.Run(async () =>
            {
                await AnnotationHttpClient.AnnotationClient.InsertAnnotation(_shape10000Point,
                    UniqueSlideImageId);
            }));

        return Task.WhenAll(jobs);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        AnnotationHttpClient.Dispose();
    }
}*/

