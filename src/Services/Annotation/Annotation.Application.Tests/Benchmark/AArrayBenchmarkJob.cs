using BenchmarkDotNet.Attributes;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Tests.ByteSerializer;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Tests.Benchmark;

[MemoryDiagnoser]
// [SimpleJob(launchCount: 2, warmupCount: 0, targetCount: 10)]
public abstract class AArrayBenchmarkJob<T> where T : struct
{
    private readonly BitConverterByteSerializer _bitConverterByteSerializer;
    private readonly ShiftByteSerializer _shiftByteSerializer;
    private readonly int _sizeOfDataType;
    protected readonly BinaryPrimitivesByteSerializer BinaryPrimitivesByteSerializer;
    protected readonly FixedPointerByteSerializer FixedPointerByteSerializer;

    protected T[] IntValues;
    protected Memory<byte> Memory;

    protected Random Random = new(420);

    public AArrayBenchmarkJob(int sizeOfDataType)
    {
        _sizeOfDataType = sizeOfDataType;
        _shiftByteSerializer = new ShiftByteSerializer();
        _bitConverterByteSerializer = new BitConverterByteSerializer();
        FixedPointerByteSerializer = new FixedPointerByteSerializer();
        BinaryPrimitivesByteSerializer = new BinaryPrimitivesByteSerializer();
    }

    // [Params(10, 100, 1000, 10000, 100000)]
    [Params(1000)]
    public int Count { get; set; }

    protected abstract T CreateValue();

    [GlobalSetup]
    public void GlobalSetup()
    {
        Memory = new Memory<byte>(new byte[Count * _sizeOfDataType]);
        IntValues = new T[Count];
        for (var i = 0; i < Count; i++)
        {
            IntValues[i] = CreateValue();
        }
    }

    protected abstract void DoSerialize(T[] value, Span<byte> target, IByteSerializer serializer);

    // [Benchmark]
    public byte[] ShiftSerialize()
    {
        DoSerialize(IntValues, Memory.Span, _shiftByteSerializer);

        return Memory.ToArray();
    }

    [Benchmark]
    public byte[] BitConverterSerialize()
    {
        DoSerialize(IntValues, Memory.Span, _bitConverterByteSerializer);

        return Memory.ToArray();
    }

    [Benchmark]
    public byte[] FixedPointerSerialize()
    {
        DoSerialize(IntValues, Memory.Span, FixedPointerByteSerializer);

        return Memory.ToArray();
    }

    // [Benchmark]
    public byte[] BinaryPrimitivesBytesSerialize()
    {
        DoSerialize(IntValues, Memory.Span, BinaryPrimitivesByteSerializer);

        return Memory.ToArray();
    }
}