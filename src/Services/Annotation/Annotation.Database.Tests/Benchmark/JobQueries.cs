using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Database.Tests.Config;
using PreciPoint.Ims.Services.Annotation.Database.Tests.Mocks;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Utils.TestUtils.Config;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Database.Tests.Benchmark;

[SimpleJob(RunStrategy.Throughput)]
[MemoryDiagnoser]
public class JobQueries
{
    private DatabaseTestConfig _config;
    private IServiceProvider _serviceProvider;

    // [Params("f56c73bd-be98-41f9-94b5-9ba1f4e098e1",
    //     "58740589-d449-4a92-bab9-76da922d034f",
    //     "8f3b7231-4d19-4fd2-bf21-49de2699d1f2",
    //     "81dbce2a-d132-4226-ac6c-2c6f62e038fa",
    //     "eb60ac27-7c8f-49fb-85de-e32f136172b7",
    //     "6d2a9098-ceb8-4d0a-94d0-1206c15737d9",
    //     "d04b069d-46ad-42e3-8089-59829cc9dbf4")]
    public string SlideImageId;

    public JobQueries()
    {
        Setup();
    }

    // [Benchmark]
    public Task GetAnnotationsQuery()
    {
        IServiceScope scope = _serviceProvider.CreateScope();
        var annotationQueryFilter = scope.ServiceProvider.GetService<AnnotationQueryFilter>();
        var annotationQueries = scope.ServiceProvider.GetService<IAnnotationQueries>();

        Expression<Func<AnnotationShape, bool>> filter = annotationQueryFilter.GetAnnotationsFilter(new Guid(SlideImageId),
            new Guid("c64aba85-b94f-4460-8ce6-2c18724f49d4"));

        return annotationQueries.GetAnnotationsNoTrack(filter).ToListAsync();
    }

    // [Benchmark]
    public Task GetAnnotationsQuerySplit()
    {
        IServiceScope scope = _serviceProvider.CreateScope();
        var annotationQueryFilter = scope.ServiceProvider.GetService<AnnotationQueryFilter>();
        var annotationQueries = scope.ServiceProvider.GetService<IAnnotationQueries>();


        Expression<Func<AnnotationShape, bool>> filter = annotationQueryFilter.GetAnnotationsFilter(new Guid(SlideImageId),
            new Guid("c64aba85-b94f-4460-8ce6-2c18724f49d4"));

        return annotationQueries.GetAnnotationsNoTrack(filter).AsSplitQuery().ToListAsync();
    }

    [Benchmark]
    public async Task<List<AnnotationShape>> GetAnnotations100KPolygon()
    {
        IServiceScope scope = _serviceProvider.CreateScope();
        var annotationQueryFilter = scope.ServiceProvider.GetService<AnnotationQueryFilter>();
        var annotationQueries = scope.ServiceProvider.GetService<IAnnotationQueries>();


        Expression<Func<AnnotationShape, bool>> filter = annotationQueryFilter.GetAnnotationsFilter(new Guid("99f71f0a-785d-480d-8366-0fb92ae9b46d"),
            new Guid("c64aba85-b94f-4460-8ce6-2c18724f49d4"));

        return await annotationQueries.GetAnnotationsNoTrack(filter).AsSplitQuery()
            .ToListAsync(CancellationToken.None);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        DisposeAllIDisposable();
    }

    private void Setup()
    {
        _config = new JsonSettings().Configuration.Get<DatabaseTestConfig>();

        var services = new ServiceCollection();

        services.AddApplication(GetAppConfig().LocalizationConfig);
        services.AddDatabase(_config.ConnectionString);
        services.AddSingleton<IClaimsPrincipalProvider, ClaimsPrincipalProviderMock>();
        services.AddSingleton(GetAppConfig());
        services.AddLogging(o => o.AddDebug());

        _serviceProvider = services.BuildServiceProvider(true);
    }

    private ApplicationConfig GetAppConfig()
    {
        return new ApplicationConfig
        {
            PerformanceBehaviour = new PerformanceBehaviour { LongRunningTriggerMilliseconds = 1000 },
            LocalizationConfig = new LocalizationConfig
            {
                SupportedCultures = new List<string>
                {
                    "en",
                    "de",
                    "it"
                }
            }
        };
    }

    private void DisposeAllIDisposable()
    {
        switch (_serviceProvider)
        {
            case null:
                return;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}