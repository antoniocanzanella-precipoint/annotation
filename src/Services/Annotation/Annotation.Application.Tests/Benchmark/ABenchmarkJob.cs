using BenchmarkDotNet.Attributes;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.Tests.ByteSerializer;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Tests.Benchmark;

[MemoryDiagnoser]
public abstract class ABenchmarkJob<T> where T : struct
{
    private readonly BinaryPrimitivesByteSerializer _binaryPrimitivesByteSerializer;
    private readonly BitConverterByteSerializer _bitConverterByteSerializer;
    private readonly FixedPointerByteSerializer _fixedPointerByteSerializer;
    private readonly ShiftByteSerializer _shiftByteSerializer;
    private readonly int _sizeOfDataType;

    private readonly T[] _intValues;
    private readonly Memory<byte> _memory;

    public ABenchmarkJob(int sizeOfDataType)
    {
        _sizeOfDataType = sizeOfDataType;
        _shiftByteSerializer = new ShiftByteSerializer();
        _bitConverterByteSerializer = new BitConverterByteSerializer();
        _fixedPointerByteSerializer = new FixedPointerByteSerializer();
        _binaryPrimitivesByteSerializer = new BinaryPrimitivesByteSerializer();

        _intValues = new T[Count];
        _memory = new Memory<byte>(new byte[Count * _sizeOfDataType]);
    }

    [Params(1, 10, 100, 1000)]
    public int Count { get; set; }

    protected abstract void DoSerialize(T value, Span<byte> target, IByteSerializer serializer);

    [Benchmark]
    public byte[] ShiftSerialize()
    {
        for (var i = 0; i < Count; i++)
        {
            DoSerialize(_intValues[i], _memory.Span.Slice(i), _shiftByteSerializer);
        }

        return _memory.ToArray();
    }

    [Benchmark]
    public byte[] BitConverterSerialize()
    {
        for (var i = 0; i < Count; i++)
        {
            DoSerialize(_intValues[i], _memory.Span.Slice(i), _bitConverterByteSerializer);
        }

        return _memory.ToArray();
    }

    [Benchmark]
    public byte[] FixedPointerSerialize()
    {
        for (var i = 0; i < Count; i++)
        {
            DoSerialize(_intValues[i], _memory.Span.Slice(i), _fixedPointerByteSerializer);
        }

        return _memory.ToArray();
    }

    [Benchmark]
    public byte[] BinaryPrimitivesSerializer()
    {
        for (var i = 0; i < Count; i++)
        {
            DoSerialize(_intValues[i], _memory.Span.Slice(i), _binaryPrimitivesByteSerializer);
        }

        return _memory.ToArray();
    }
}