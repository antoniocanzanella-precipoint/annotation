using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.Configuration;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Config;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Extensions;
using PreciPoint.Ims.Core.DataTransfer.Factories;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Utils.TestUtils.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Benchmark;

[SimpleJob(RunStrategy.Throughput)]
public class JobGetAnnotations
{
    private readonly AnnotationHttpClient _annotationHttpClient;
    private readonly AnnotationTestConfig _configuration;

    private readonly Random _random = new(1337);

    private Guid _slideImageId;

    public JobGetAnnotations()
    {
        _configuration = new JsonSettings().Configuration.Get<AnnotationTestConfig>();
        var httpClientFactory = new HttpClientFactory(_configuration.SecuritySystem);
        var apiUrl = $"{_configuration.AnnotationHost}/api";

        _annotationHttpClient =
            new AnnotationHttpClient(httpClientFactory.CreateUserHttpClient(_configuration), apiUrl);
    }

    [Params(1, 10, 50)]
    public int Users { get; set; }

    [Benchmark]
    public Task GetAnnotations()
    {
        IEnumerable<Task> jobs = Enumerable.Range(0, Users).Select(_ =>
            Task.Run(async () =>
            {
                int index = _random.Next(0, _configuration.SlideImageIds.Count);
                _slideImageId = _configuration.SlideImageIds[index];

                await _annotationHttpClient.AnnotationClient.GetAnnotations(_slideImageId);
            }));

        return Task.WhenAll(jobs);
    }

    [Benchmark]
    public Task GetAnnotationsDeckGl()
    {
        IEnumerable<Task<BinaryDataWithHeaderDto>> jobs = Enumerable.Range(0, Users).Select(_ => Task.Run(async () =>
        {
            int index = _random.Next(0, _configuration.SlideImageIds.Count);
            _slideImageId = _configuration.SlideImageIds[index];

            return await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImageId);
        }));

        return Task.WhenAll(jobs);
    }

    [Benchmark]
    public Task GetAnnotations_MostExpensiveImage()
    {
        IEnumerable<Task> jobs = Enumerable.Range(0, Users).Select(_ =>
            Task.Run(async () =>
            {
                await _annotationHttpClient.AnnotationClient.GetAnnotations(
                    _configuration.GetExpensiveSlideImage());
            }));

        return Task.WhenAll(jobs);
    }

    [Benchmark]
    public Task GetAnnotations_MostExpensiveImageDeckGl()
    {
        IEnumerable<Task> jobs = Enumerable.Range(0, Users).Select(_ =>
            Task.Run(async () =>
            {
                await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(
                    _configuration.GetExpensiveSlideImage());
            }));

        return Task.WhenAll(jobs);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _annotationHttpClient.Dispose();
    }
}