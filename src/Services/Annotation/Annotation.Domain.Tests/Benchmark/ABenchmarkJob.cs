using BenchmarkDotNet.Attributes;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Domain.Tests.ByteSerializer;
using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.Tests.Benchmark
{
        [MemoryDiagnoser]
        public abstract class ABenchmarkJob<T> where T : struct
        {
            private readonly int _sizeOfDataType;
            private readonly ShiftByteSerializer _shiftByteSerializer;
            private readonly BitConverterByteSerializer _bitConverterByteSerializer;
            private readonly FixedPointerByteSerializer _fixedPointerByteSerializer;
            private readonly BinaryPrimitivesByteSerializer _binaryPrimitivesByteSerializer;

            [Params(1, 10, 100, 1000)]
            public int Count { get; set; }

            private T[] _intValues;
            private Memory<byte> _memory;

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

            protected abstract void DoSerialize(T value, Span<byte> target, IByteSerializer serializer);
            
            [Benchmark]
            public byte[] ShiftSerialize()
            {
                for (int i = 0; i < Count; i++)
                {
                    DoSerialize(_intValues[i], _memory.Span.Slice(i), _shiftByteSerializer);
                }

                return _memory.ToArray();
            }

            [Benchmark]
            public byte[] BitConverterSerialize()
            {
                for (int i = 0; i < Count; i++)
                {
                    DoSerialize(_intValues[i], _memory.Span.Slice(i), _bitConverterByteSerializer);
                }

                return _memory.ToArray();
            }
            
            [Benchmark]
            public byte[] FixedPointerSerialize()
            {
                for (int i = 0; i < Count; i++)
                {
                    DoSerialize(_intValues[i], _memory.Span.Slice(i), _fixedPointerByteSerializer);
                }

                return _memory.ToArray();
            }
            
            [Benchmark]
            public byte[] BinaryPrimitivesSerializer()
            {
                for (int i = 0; i < Count; i++)
                {
                    DoSerialize(_intValues[i], _memory.Span.Slice(i), _binaryPrimitivesByteSerializer);
                }

                return _memory.ToArray();
            }
        }
}