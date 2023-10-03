using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.Configuration;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Config;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Extensions;
using PreciPoint.Ims.Core.DataTransfer.Factories;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Utils.TestUtils.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Benchmark;

[SimpleJob(RunStrategy.Throughput)]
public class JobGetAnnotationById
{
    private readonly AnnotationHttpClient _annotationHttpClient;
    private readonly List<AnnotationDto> _annotations = new();
    private readonly AnnotationTestConfig _configuration;
    private readonly Random _random = new(1337);
    private Guid _annotationId;

    public JobGetAnnotationById()
    {
        _configuration = new JsonSettings().Configuration.Get<AnnotationTestConfig>();
        var httpClientFactory = new HttpClientFactory(_configuration.SecuritySystem);
        var apiUrl = $"{_configuration.AnnotationHost}/api";

        _annotationHttpClient =
            new AnnotationHttpClient(httpClientFactory.CreateUserHttpClient(_configuration), apiUrl);

        ApiListResponse<AnnotationDto> result = _annotationHttpClient.AnnotationClient.GetAnnotations(_configuration.GetExpensiveSlideImage())
            .GetAwaiter().GetResult();

        _annotations.AddRange(result.Data);
    }

    [Params(1, 10, 50)]
    public int Users { get; set; }

    [Benchmark]
    public Task GetSlideImageAnnotationById()
    {
        IEnumerable<Task> jobs = Enumerable.Range(0, Users).Select(_ =>
            Task.Run(async () =>
            {
                int index = _random.Next(0, _annotations.Count - 1);
                _annotationId = _annotations[index].Id.Value;

                await _annotationHttpClient.AnnotationClient.GetAnnotationById(_annotationId);
            }));

        return Task.WhenAll(jobs);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _annotationHttpClient.Dispose();
    }
}