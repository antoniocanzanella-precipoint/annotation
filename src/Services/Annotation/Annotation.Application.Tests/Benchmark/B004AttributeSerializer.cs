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
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Application.Tests.Mocks;
using PreciPoint.Ims.Services.Annotation.Database;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PreciPoint.Ims.Services.Annotation.Application.Tests.Benchmark;

[TestFixture]
[Order(3)]
public class B004AttributeSerializer
{
    public class AttributeSerializerJobs
    {
        private readonly ClaimsPrincipalProviderMock _mockPrincipal;
        private ServiceProvider _serviceProvider;

        public AttributeSerializerJobs()
        {
            Setup();
            _mockPrincipal = new ClaimsPrincipalProviderMock();
            IServiceScope scope = _serviceProvider.CreateScope();
            var queryFilter = scope.ServiceProvider.GetRequiredService<AnnotationQueryFilter>();
            Expression<Func<AnnotationShape, bool>> filter = queryFilter.GetAnnotationsFilter(new Guid("99f71f0a-785d-480d-8366-0fb92ae9b46d"),
                new Guid("c64aba85-b94f-4460-8ce6-2c18724f49d4"));
            var annotationQueries = scope.ServiceProvider.GetRequiredService<IAnnotationQueries>();

            var builder = new LayerHeaderBuilder(_mockPrincipal);

            Annotations = annotationQueries.GetAnnotationsNoTrack(filter).ToList();
            Result = builder.Build(Annotations);

            Header = Result.LayerHeaders.First() as LayerHeaderDto;
            Layer = Result.DeckGlLayers.Values.First();

            Buffer = new Memory<byte>(new byte[Result.BinaryDataSize]);

            Serializer = _serviceProvider.GetRequiredService<IByteSerializer>();
        }

        public List<AnnotationShape> Annotations { get; set; }
        public BuildResult Result { get; set; }

        public LayerHeaderDto Header { get; set; }
        public DeckGlLayer<AnnotationShape> Layer { get; set; }
        public Memory<byte> Buffer { get; set; }

        public IByteSerializer Serializer { get; set; }

        // [Benchmark]
        // public ReadOnlySpan<byte> SerializePosition()
        // {
        //     var scope = _serviceProvider.CreateScope();
        //     var serializer = scope.ServiceProvider.GetRequiredService<AnnotationPolygonAttributeSerializer>();
        //     
        //     serializer.SerializeAttribute(Layer, Header, Buffer.Span);
        //     
        //     return Buffer.Span;
        // }

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
    public void B004_001Serializer()
    {
        Summary summary = BenchmarkRunner.Run<AttributeSerializerJobs>(
            ManualConfig
                .Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));

        Assert.IsNotEmpty(summary.Reports);
        Assert.IsNotEmpty(summary.Reports[0].ExecuteResults);
        Assert.AreEqual(0, summary.Reports[0].ExecuteResults[0].ExitCode);
        FileAssert.Exists(summary.LogFilePath);
    }
}