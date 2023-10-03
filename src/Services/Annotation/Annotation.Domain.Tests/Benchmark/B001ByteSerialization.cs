using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.Tests.Benchmark
{
    [TestFixture]
    [Order(1)]
    [NonParallelizable]
    [System.ComponentModel.Category("Benchmark")]
    public class B001ByteSerialization
    {
        public class IntegerByteSerializerBenchmark : ABenchmarkJob<int>
        {
            public IntegerByteSerializerBenchmark() : base(sizeof(int))
            {
            }

            protected override void DoSerialize(int value, Span<byte> target, IByteSerializer serializer)
            {
                serializer.Serialize(value, target);
            }
        }
        public class FloatByteSerializerBenchmark : ABenchmarkJob<float>
        {
            public FloatByteSerializerBenchmark() : base(sizeof(float))
            {
            }

            protected override void DoSerialize(float value, Span<byte> target, IByteSerializer serializer)
            {
                serializer.Serialize(value, target);
            }
        }
        
        public class DoubleByteSerializerBenchmark : ABenchmarkJob<double>
        {
            public DoubleByteSerializerBenchmark() : base(sizeof(double))
            {
            }

            protected override void DoSerialize(double value, Span<byte> target, IByteSerializer serializer)
            {
                serializer.Serialize(value, target);
            }
        }
        
        public class LongByteSerializerBenchmark : ABenchmarkJob<long>
        {
            public LongByteSerializerBenchmark() : base(sizeof(long))
            {
            }

            protected override void DoSerialize(long value, Span<byte> target, IByteSerializer serializer)
            {
                serializer.Serialize(value, target);
            }
        }
        
        [Test]
        [Order(1)]
        public void B001_001IntegerSerialization()
        {
            var summary = BenchmarkRunner.Run<IntegerByteSerializerBenchmark>(
                    ManualConfig
                    .Create(DefaultConfig.Instance)
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));
            
            Assert.IsNotEmpty(summary.Reports);
            Assert.IsNotEmpty(summary.Reports[0].ExecuteResults);
            Assert.AreEqual(0, summary.Reports[0].ExecuteResults[0].ExitCode);
            FileAssert.Exists(summary.LogFilePath);
        }
        
        [Test]
        [Order(2)]
        public void B001_002FloatSerialization()
        {
            var summary = BenchmarkRunner.Run<FloatByteSerializerBenchmark>(
                    ManualConfig
                    .Create(DefaultConfig.Instance)
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));
            
            Assert.IsNotEmpty(summary.Reports);
            Assert.IsNotEmpty(summary.Reports[0].ExecuteResults);
            Assert.AreEqual(0, summary.Reports[0].ExecuteResults[0].ExitCode);
            FileAssert.Exists(summary.LogFilePath);
        }
        [Test]
        [Order(3)]
        public void B001_003DoubleSerialization()
        {
            var summary = BenchmarkRunner.Run<DoubleByteSerializerBenchmark>(
                    ManualConfig
                    .Create(DefaultConfig.Instance)
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));
            
            Assert.IsNotEmpty(summary.Reports);
            Assert.IsNotEmpty(summary.Reports[0].ExecuteResults);
            Assert.AreEqual(0, summary.Reports[0].ExecuteResults[0].ExitCode);
            FileAssert.Exists(summary.LogFilePath);
        }
        
        [Test]
        [Order(4)]
        public void B001_004LongSerialization()
        {
            var summary = BenchmarkRunner.Run<LongByteSerializerBenchmark>(
                    ManualConfig
                    .Create(DefaultConfig.Instance)
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));
            
            Assert.IsNotEmpty(summary.Reports);
            Assert.IsNotEmpty(summary.Reports[0].ExecuteResults);
            Assert.AreEqual(0, summary.Reports[0].ExecuteResults[0].ExitCode);
            FileAssert.Exists(summary.LogFilePath);
        }
        
    }
    


}
