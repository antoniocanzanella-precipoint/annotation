using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using System;
using System.Buffers.Binary;

namespace PreciPoint.Ims.Services.Annotation.Domain.Tests.ByteSerializer
{
    public class BinaryPrimitivesByteSerializer : IByteSerializer
    {
        public int Serialize(string value, Span<byte> target)
        {
            throw new NotImplementedException();
        }

        public int Serialize(int value, Span<byte> target)
        {
           BinaryPrimitives.WriteInt32LittleEndian(target, value);
           
           return sizeof(int);
        }

        public int Serialize(int[] values, Span<byte> target)
        {
            int written = 0;
            for (int i = 0; i < values.Length; i++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(target.Slice(written), values[i]);
                written += sizeof(int);
            }

            return written;
        }

        public int Serialize(long value, Span<byte> target)
        {
           BinaryPrimitives.WriteInt64LittleEndian(target, value);
           
           return sizeof(long);
        }

        public int Serialize(long[] values, Span<byte> target)
        {
            int written = 0;
            for (int i = 0; i < values.Length; i++)
            {
                BinaryPrimitives.WriteInt64LittleEndian(target.Slice(written), values[i]);
                written += sizeof(long);
            }

            return written;
        }

        public int Serialize(float value, Span<byte> target)
        {
           BinaryPrimitives.WriteSingleLittleEndian(target, value);
           
           return sizeof(float);
        }

        public int Serialize(float[] values, Span<byte> target)
        {
            int written = 0;
            for (int i = 0; i < values.Length; i++)
            {
                BinaryPrimitives.WriteSingleLittleEndian(target.Slice(written), values[i]);
                written += sizeof(float);
            }

            return written;
        }

        public int Serialize(double value, Span<byte> target)
        {
           BinaryPrimitives.WriteDoubleLittleEndian(target, value);
           
           return sizeof(double);
        }

        public int Serialize(double[] values, Span<byte> target)
        {
            int written = 0;
            for (int i = 0; i < values.Length; i++)
            {
                BinaryPrimitives.WriteDoubleLittleEndian(target.Slice(written), values[i]);
                written += sizeof(double);
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