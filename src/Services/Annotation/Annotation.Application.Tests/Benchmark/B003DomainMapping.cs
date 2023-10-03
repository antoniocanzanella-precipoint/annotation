using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Header;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Application.Tests.Mocks;
using PreciPoint.Ims.Services.Annotation.Database;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace PreciPoint.Ims.Services.Annotation.Application.Tests.Benchmark;

[TestFixture]
[Order(3)]
[NonParallelizable]
[System.ComponentModel.Category("Benchmark")]
[MemoryDiagnoser]
public class B003DomainMapping
{
    public class AnnotationDomainSerializeForNetworkTransfer
    {
        private readonly ClaimsPrincipalProviderMock _mockPrincipal;
        private ServiceProvider _serviceProvider;

        public AnnotationDomainSerializeForNetworkTransfer()
        {
            Setup();
            _mockPrincipal = new ClaimsPrincipalProviderMock();
            IServiceScope scope = _serviceProvider.CreateScope();
            var queryFilter = scope.ServiceProvider.GetRequiredService<AnnotationQueryFilter>();
            Expression<Func<AnnotationShape, bool>> filter = queryFilter.GetAnnotationsFilter(new Guid("99f71f0a-785d-480d-8366-0fb92ae9b46d"),
                new Guid("c64aba85-b94f-4460-8ce6-2c18724f49d4"));
            var annotationQueries = scope.ServiceProvider.GetRequiredService<IAnnotationQueries>();

            Annotations = annotationQueries.GetAnnotationsNoTrack(filter).ToList();
        }

        public List<AnnotationShape> Annotations { get; set; }

        // [Benchmark]
        public byte[] GetAnnotations()
        {
            IServiceScope scope = _serviceProvider.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            var dto = mapper.Map<List<AnnotationDto>>(Annotations);

            byte[] json = JsonSerializer.SerializeToUtf8Bytes(dto);
            return json;
        }

        [Benchmark]
        public BinaryDataWithHeaderDto GetAnnotationsDeckGl()
        {
            IServiceScope scope = _serviceProvider.CreateScope();
            var serializer = scope.ServiceProvider.GetRequiredService<IDeckGlAnnotationSerializer>();

            var builder = new LayerHeaderBuilder(_mockPrincipal);

            BuildResult result = builder.Build(Annotations);

            var binaryData = new Memory<byte>(new byte[result.BinaryDataSize]);

            serializer.Serialize(result.LayerHeaders,
                result.DeckGlLayers,
                binaryData);

            return new BinaryDataWithHeaderDto { Data = binaryData.ToArray(), Headers = result.LayerHeaders };
        }

        private void Setup()
        {
            var services = new ServiceCollection();

            services.AddApplication(GetAppConfig().LocalizationConfig);
            services.AddDatabase(
                "Server=localhost;Port=54325;Database=annotation;Username=postgres;Password=precipoint");
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
    }

    [Test]
    [Order(1)]
    public void B001_001MappingTODO()
    {
        Summary summary = BenchmarkRunner.Run<AnnotationDomainSerializeForNetworkTransfer>(
            ManualConfig
                .Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));

        Assert.IsNotEmpty(summary.Reports);
        Assert.IsNotEmpty(summary.Reports[0].ExecuteResults);
        Assert.AreEqual(0, summary.Reports[0].ExecuteResults[0].ExitCode);
        FileAssert.Exists(summary.LogFilePath);
    }
}