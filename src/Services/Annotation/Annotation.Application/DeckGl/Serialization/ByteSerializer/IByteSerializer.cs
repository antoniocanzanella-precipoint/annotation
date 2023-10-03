using System;

namespace PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;

public interface IByteSerializer
{
    int Serialize(string value, Span<byte> target);
    int Serialize(int value, Span<byte> target);

    int Serialize(int[] values, Span<byte> target);

    int Serialize(long value, Span<byte> target);

    int Serialize(long[] values, Span<byte> target);

    int Serialize(float value, Span<byte> target);

    int Serialize(float[] values, Span<byte> target);

    int Serialize(double value, Span<byte> target);

    int Serialize(double[] values, Span<byte> target);

    int Serialize(byte value, Span<byte> target);

    int Serialize(bool value, Span<byte> target);
}