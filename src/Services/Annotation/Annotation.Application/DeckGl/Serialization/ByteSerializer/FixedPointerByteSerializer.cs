using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;

public class FixedPointerByteSerializer : IByteSerializer
{
    public unsafe int Serialize(string value, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            var ptr = (char*) p;
            for (var i = 0; i < value.Length; i++)
            {
                *(ptr + i) = value[i];
            }
        }

        return sizeof(char) * value.Length;
    }

    public unsafe int Serialize(int value, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            *(int*) p = value;
        }

        return sizeof(int);
    }

    public unsafe int Serialize(int[] values, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            var ptr = (int*) p;
            for (var i = 0; i < values.Length; i++)
            {
                *(ptr + i) = values[i];
            }
        }

        return sizeof(int) * values.Length;
    }

    public unsafe int Serialize(long value, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            *(long*) p = value;
        }

        return sizeof(long);
    }

    public unsafe int Serialize(long[] values, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            var ptr = (long*) p;
            for (var i = 0; i < values.Length; i++)
            {
                *(ptr + i) = values[i];
            }
        }

        return sizeof(long) * values.Length;
    }

    public unsafe int Serialize(float value, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            *(float*) p = value;
        }

        return sizeof(float);
    }

    public unsafe int Serialize(float[] values, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            var ptr = (float*) p;
            for (var i = 0; i < values.Length; i++)
            {
                *(ptr + i) = values[i];
            }
        }

        return sizeof(float) * values.Length;
    }

    public unsafe int Serialize(double value, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            *(double*) p = value;
        }

        return sizeof(double);
    }

    public unsafe int Serialize(double[] values, Span<byte> target)
    {
        fixed (byte* p = target)
        {
            var ptr = (double*) p;
            for (var i = 0; i < values.Length; i++)
            {
                *(ptr + i) = values[i];
            }
        }

        return sizeof(double) * values.Length;
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