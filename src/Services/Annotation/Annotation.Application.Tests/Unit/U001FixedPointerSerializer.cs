using FluentAssertions;
using NUnit.Framework;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Tests.Unit;

[Category("Unit")]
public class U001FixedPointerSerializer
{
    [Test]
    [Order(0)]
    public void U001_000IsBigEndian()
    {
        // Please Read https://github.com/visgl/deck.gl/blob/master/docs/developer-guide/binary-data.md#endianness
        BitConverter.IsLittleEndian.Should().BeTrue();
    }

    [Test]
    [Order(1)]
    public void U001_001CanSerializeInt()
    {
        var serializer = new FixedPointerByteSerializer();

        Span<byte> span = new byte[sizeof(int)];
        int[] values = { 0, int.MaxValue, int.MinValue, 42, 69, -52354722, -123455 };
        foreach (int value in values)
        {
            int wrote = serializer.Serialize(value, span);
            wrote.Should().Be(sizeof(int));
            var retrieved = BitConverter.ToInt32(span);
            retrieved.Should().Be(value);
        }
    }

    [Test]
    [Order(2)]
    public void U001_002CanSerializeFloat()
    {
        var serializer = new FixedPointerByteSerializer();

        Span<byte> span = new byte[sizeof(float)];
        float[] values =
        {
            0, float.MaxValue, float.MinValue, -42, 42, 0.42424f, 0.13371337f, float.Epsilon, float.NaN, float.NegativeInfinity, float.PositiveInfinity
        };
        foreach (float value in values)
        {
            int wrote = serializer.Serialize(value, span);
            wrote.Should().Be(sizeof(float));
            var retrieved = BitConverter.ToSingle(span);
            retrieved.Should().Be(value);
        }
    }

    [Test]
    [Order(3)]
    public void U001_003CanSerializeDouble()
    {
        var serializer = new FixedPointerByteSerializer();

        Span<byte> span = new byte[sizeof(double)];
        double[] values =
        {
            0, double.Epsilon, double.MaxValue, double.MinValue, double.NaN, double.NegativeInfinity, double.PositiveInfinity, -42.000000000001, 42.0d,
            1337.1337d
        };
        foreach (double value in values)
        {
            int wrote = serializer.Serialize(value, span);
            wrote.Should().Be(sizeof(double));
            var retrieved = BitConverter.ToDouble(span);
            retrieved.Should().Be(value);
        }
    }

    [Test]
    [Order(4)]
    public void U001_004CanSerializeLong()
    {
        var serializer = new FixedPointerByteSerializer();

        Span<byte> span = new byte[sizeof(long)];
        long[] values = { 0, -1, 1, long.MaxValue, long.MinValue, -42, 42, 1337 };
        foreach (long value in values)
        {
            int wrote = serializer.Serialize(value, span);
            wrote.Should().Be(sizeof(long));
            var retrieved = BitConverter.ToInt64(span);
            retrieved.Should().Be(value);
        }
    }

    [Test]
    [Order(5)]
    public void U001_005CanSerializeIntArray()
    {
        var serializer = new FixedPointerByteSerializer();
        int[] values = { 0, int.MaxValue, int.MinValue, 42, 69, -52354722, -123455 };
        const int sizeOfDataType = sizeof(int);
        int totalBytes = sizeOfDataType * values.Length;
        Span<byte> span = new byte[totalBytes];
        int wrote = serializer.Serialize(values, span);
        wrote.Should().Be(totalBytes);
        for (var i = 0; i < values.Length; i++)
        {
            var retrieved = BitConverter.ToInt32(span.Slice(i * sizeOfDataType));
            retrieved.Should().Be(values[i]);
        }
    }

    [Test]
    [Order(6)]
    public void U001_006CanSerializeFloatArray()
    {
        var serializer = new FixedPointerByteSerializer();
        float[] values =
        {
            0, float.MaxValue, float.MinValue, -42, 42, 0.42424f, 0.13371337f, float.Epsilon, float.NaN, float.NegativeInfinity, float.PositiveInfinity
        };
        const int sizeOfDataType = sizeof(float);
        int totalBytes = sizeOfDataType * values.Length;
        Span<byte> span = new byte[totalBytes];
        int wrote = serializer.Serialize(values, span);
        wrote.Should().Be(totalBytes);
        for (var i = 0; i < values.Length; i++)
        {
            var retrieved = BitConverter.ToSingle(span.Slice(i * sizeOfDataType));
            retrieved.Should().Be(values[i]);
        }
    }

    [Test]
    [Order(7)]
    public void U001_007CanSerializeDoubleArray()
    {
        var serializer = new FixedPointerByteSerializer();
        double[] values =
        {
            0, double.Epsilon, double.MaxValue, double.MinValue, double.NaN, double.NegativeInfinity, double.PositiveInfinity, -42.000000000001, 42.0d,
            1337.1337d
        };
        const int sizeOfDataType = sizeof(double);
        int totalBytes = sizeOfDataType * values.Length;
        Span<byte> span = new byte[totalBytes];
        int wrote = serializer.Serialize(values, span);
        wrote.Should().Be(totalBytes);
        for (var i = 0; i < values.Length; i++)
        {
            var retrieved = BitConverter.ToDouble(span.Slice(i * sizeOfDataType));
            retrieved.Should().Be(values[i]);
        }
    }

    [Test]
    [Order(8)]
    public void U001_007CanSerializeLongArray()
    {
        var serializer = new FixedPointerByteSerializer();
        long[] values = { 0, -1, 1, long.MaxValue, long.MinValue, -42, 42, 1337 };
        const int sizeOfDataType = sizeof(long);
        int totalBytes = sizeOfDataType * values.Length;
        Span<byte> span = new byte[totalBytes];
        int wrote = serializer.Serialize(values, span);
        wrote.Should().Be(totalBytes);
        for (var i = 0; i < values.Length; i++)
        {
            var retrieved = BitConverter.ToInt64(span.Slice(i * sizeOfDataType));
            retrieved.Should().Be(values[i]);
        }
    }
}