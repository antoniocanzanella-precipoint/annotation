using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Tests.Benchmark;

[TestFixture]
[Order(2)]
[NonParallelizable]
[System.ComponentModel.Category("Benchmark")]
public class B002ByteArraySerialization
{
    public class IntegerArrayByteSerializerBenchmark : AArrayBenchmarkJob<int>
    {
        public IntegerArrayByteSerializerBenchmark() : base(sizeof(int)) { }

        protected override int CreateValue()
        {
            return Random.Next();
        }

        protected override void DoSerialize(int[] value, Span<byte> target, IByteSerializer serializer)
        {
            serializer.Serialize(value, target);
        }
    }

    public class FloatArrayByteSerializerBenchmark : AArrayBenchmarkJob<float>
    {
        public FloatArrayByteSerializerBenchmark() : base(sizeof(float)) { }

        protected override float CreateValue()
        {
            return Random.NextSingle();
        }

        protected override void DoSerialize(float[] value, Span<byte> target, IByteSerializer serializer)
        {
            serializer.Serialize(value, target);
        }

        [Benchmark]
        public byte[] SerializePositionFixed()
        {
            var written = 0;
            for (var i = 0; i < Count; i++)
            {
                written += FixedPointerByteSerializer.Serialize(IntValues[i], Memory.Span.Slice(written));
            }

            return Memory.ToArray();
        }

        [Benchmark]
        public byte[] SerializePositionPrimitive()
        {
            var written = 0;
            for (var i = 0; i < Count; i++)
            {
                written += BinaryPrimitivesByteSerializer.Serialize(IntValues[i], Memory.Span.Slice(written));
            }

            return Memory.ToArray();
        }
    }

    public class DoubleArrayByteSerializerBenchmark : AArrayBenchmarkJob<double>
    {
        public DoubleArrayByteSerializerBenchmark() : base(sizeof(double)) { }

        protected override double CreateValue()
        {
            return Random.NextDouble();
        }

        protected override void DoSerialize(double[] value, Span<byte> target, IByteSerializer serializer)
        {
            serializer.Serialize(value, target);
        }
    }

    public class LongArrayByteSerializerBenchmark : AArrayBenchmarkJob<long>
    {
        public LongArrayByteSerializerBenchmark() : base(sizeof(long)) { }

        protected override long CreateValue()
        {
            return Random.NextInt64();
        }

        protected override void DoSerialize(long[] value, Span<byte> target, IByteSerializer serializer)
        {
            serializer.Serialize(value, target);
        }
    }

    [Test]
    [Order(1)]
    public void B002_001IntegerArraySerialization()
    {
        Summary summary = BenchmarkRunner.Run<IntegerArrayByteSerializerBenchmark>(
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
    public void B002_002FloatArraySerialization()
    {
        Summary summary = BenchmarkRunner.Run<FloatArrayByteSerializerBenchmark>(
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
    public void B002_003DoubleArraySerialization()
    {
        Summary summary = BenchmarkRunner.Run<DoubleArrayByteSerializerBenchmark>(
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
    public void B002_004LongArraySerialization()
    {
        Summary summary = BenchmarkRunner.Run<LongArrayByteSerializerBenchmark>(
            ManualConfig
                .Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));

        Assert.IsNotEmpty(summary.Reports);
        Assert.IsNotEmpty(summary.Reports[0].ExecuteResults);
        Assert.AreEqual(0, summary.Reports[0].ExecuteResults[0].ExitCode);
        FileAssert.Exists(summary.LogFilePath);
    }
}