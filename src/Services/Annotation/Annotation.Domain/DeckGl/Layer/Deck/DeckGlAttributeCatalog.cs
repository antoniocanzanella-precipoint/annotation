using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Attribute;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer.Deck;

public static class DeckGlAttributeCatalog
{
    private static PrimitiveDataType PrecisionDataType(bool useHighPrecision)
    {
        return useHighPrecision ? PrimitiveDataType.Double : PrimitiveDataType.Float;
    }

    public static DeckGlAttribute Position(bool useHighPrecision = false, byte size = 2)
    {
        return new(PrecisionDataType(useHighPrecision), DeckGlDataAccessor.GetPosition, size);
    }

    public static DeckGlAttribute Radius(bool useHighPrecision = false, byte size = 1)
    {
        return new(PrecisionDataType(useHighPrecision), DeckGlDataAccessor.GetRadius, size);
    }

    public static DeckGlAttribute FillColor(byte size = 4)
    {
        return new(PrimitiveDataType.UInt8, DeckGlDataAccessor.GetFillColor, size, true);
    }

    public static DeckGlAttribute LineColor(byte size = 4)
    {
        return new(PrimitiveDataType.UInt8, DeckGlDataAccessor.GetLineColor, size, true);
    }

    public static DeckGlAttribute Path(bool useHighPrecision = false, byte size = 2)
    {
        return new(PrecisionDataType(useHighPrecision), DeckGlDataAccessor.GetPath, size);
    }

    public static DeckGlAttribute Color(byte size = 4)
    {
        return new(PrimitiveDataType.UInt8, DeckGlDataAccessor.GetColor, size, true);
    }

    public static DeckGlAttribute Width(byte size = 1)
    {
        return new(PrimitiveDataType.UInt8, DeckGlDataAccessor.GetWidth, size);
    }

    public static DeckGlAttribute Polygon(bool useHighPrecision = false, byte size = 2)
    {
        return new(PrecisionDataType(useHighPrecision), DeckGlDataAccessor.GetPolygon, size);
    }

    public static DeckGlAttribute Elevation(bool useHighPrecision = false, byte size = 1)
    {
        return new(PrecisionDataType(useHighPrecision), DeckGlDataAccessor.GetElevation, size);
    }

    public static DeckGlAttribute Text(PrimitiveDataType dataType, byte size = 1)
    {
        if (dataType is PrimitiveDataType.UInt8 or PrimitiveDataType.UInt16 or PrimitiveDataType.UInt32)
        {
            return new DeckGlAttribute(dataType, DeckGlDataAccessor.GetText, size);
        }

        throw new ArgumentException($"Text attribute does not support datatype {dataType}", nameof(dataType));
    }
}