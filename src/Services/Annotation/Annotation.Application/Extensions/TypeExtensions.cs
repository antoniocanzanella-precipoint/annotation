using PreciPoint.Ims.Services.Annotation.Enums;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Extensions;

public static class TypeExtensions
{
    public static PrimitiveDataType ToPrimitiveDataType(this Type type)
    {
        if (type == typeof(float))
        {
            return PrimitiveDataType.Float;
        }

        if (type == typeof(double))
        {
            return PrimitiveDataType.Double;
        }

        if (type == typeof(byte))
        {
            return PrimitiveDataType.UInt8;
        }

        if (type == typeof(ushort))
        {
            return PrimitiveDataType.UInt16;
        }

        if (type == typeof(uint))
        {
            return PrimitiveDataType.UInt32;
        }

        if (type == typeof(ulong))
        {
            return PrimitiveDataType.UInt64;
        }

        if (type == typeof(sbyte))
        {
            return PrimitiveDataType.Int8;
        }

        if (type == typeof(short))
        {
            return PrimitiveDataType.Int16;
        }

        if (type == typeof(int))
        {
            return PrimitiveDataType.Int32;
        }

        if (type == typeof(long))
        {
            return PrimitiveDataType.Int64;
        }

        throw new ArgumentOutOfRangeException($"{type} cannot be converted to a primitive datatype!");
    }
}