using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.Tests.ByteSerializer
{
    public class BitConverterByteSerializer : IByteSerializer
    {
        private int Copy(byte[] arr, Span<byte> target)
        {
            for (var i = 0; i < arr.Length; i++)
            {
                target[i] = arr[i];
            }
            return arr.Length;
        }

        public int Serialize(string value, Span<byte> target)
        {
            throw new NotImplementedException();
        }

        public int Serialize(int value, Span<byte> target)
        {
            var arr = BitConverter.GetBytes(value);
            return Copy(arr, target);
        }

        public int Serialize(int[] values, Span<byte> target)
        {
            var written = 0;
            for (var i = 0; i < values.Length; i++)
            {
                written += Serialize(values[i], target.Slice(written));
            }

            return written;
        }

        public int Serialize(long value, Span<byte> target)
        {
            var arr = BitConverter.GetBytes(value);
            return Copy(arr, target);
        }

        public int Serialize(long[] values, Span<byte> target)
        {
            var written = 0;
            for (var i = 0; i < values.Length; i++)
            {
                written += Serialize(values[i], target.Slice(written));
            }

            return written;
        }

        public int Serialize(float value, Span<byte> target)
        {
            var arr = BitConverter.GetBytes(value);
            return Copy(arr, target);
        }

        public int Serialize(float[] values, Span<byte> target)
        {
            var written = 0;
            for (var i = 0; i < values.Length; i++)
            {
                written += Serialize(values[i], target.Slice(written));
            }

            return written;
        }

        public int Serialize(double value, Span<byte> target)
        {
            var arr = BitConverter.GetBytes(value);
            return Copy(arr, target);
        }

        public int Serialize(double[] values, Span<byte> target)
        {
            var written = 0;
            for (var i = 0; i < values.Length; i++)
            {
                written += Serialize(values[i], target.Slice(written));
            }

            return written;
        }
        
        
        public int Serialize(byte value, Span<byte> target)
        {
            target[0] = value;
            
            return 1;
        }

        public int Serialize(bool value, Span<byte> target)
        {
            target[0] = Convert.ToByte(value);
            
            return 1;
        }
    }
}