using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.Tests.ByteSerializer
{
    public class ShiftByteSerializer : IByteSerializer
    {
        public int Serialize(string value, Span<byte> target)
        {
            throw new NotImplementedException();
        }

        public int Serialize(int value, Span<byte> target)
        {
            target[0] = (byte) value;
            target[1] = (byte) (value >> 8);
            target[2] = (byte) (value >> 16);
            target[3] = (byte) (value >> 24);

            return sizeof(int);
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
            target[0] = (byte) value;
            target[1] = (byte) (value >> 8);
            target[2] = (byte) (value >> 16);
            target[3] = (byte) (value >> 24);
            target[4] = (byte) (value >> 32);
            target[5] = (byte) (value >> 40);
            target[6] = (byte) (value >> 48);
            target[7] = (byte) (value >> 56);

            return sizeof(long);
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
            return Serialize((int) value, target);
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
            return Serialize((long) value, target);
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