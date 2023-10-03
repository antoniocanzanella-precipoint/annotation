using PreciPoint.Ims.Services.Annotation.Enums;
using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.Extensions;

public static class PrimitiveDataTypeExtensions
{
    public static byte GetSize(this PrimitiveDataType type)
    {
        switch (type)
        {
            case PrimitiveDataType.Double:
            case PrimitiveDataType.Int64:
            case PrimitiveDataType.UInt64:
                return 8;
            case PrimitiveDataType.Int32:
            case PrimitiveDataType.UInt32:
            case PrimitiveDataType.Float:
                return 4;
            case PrimitiveDataType.Int16:
            case PrimitiveDataType.UInt16:
                return 2;
            case PrimitiveDataType.Int8:
            case PrimitiveDataType.UInt8:
                return 1;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}