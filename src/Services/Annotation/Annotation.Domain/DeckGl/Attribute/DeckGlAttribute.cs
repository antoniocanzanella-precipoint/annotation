using PreciPoint.Ims.Services.Annotation.Domain.Extensions;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

namespace PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Attribute;

public class DeckGlAttribute
{
    public DeckGlAttribute(PrimitiveDataType dataType, DeckGlDataAccessor dataAccessor,
        byte size, bool isNormalize = false)
    {
        DataType = dataType;
        DataAccessor = dataAccessor;
        Size = size;
        IsNormalize = isNormalize;
    }

    public PrimitiveDataType DataType { get; }

    public DeckGlDataAccessor DataAccessor { get; }

    public byte Size { get; }

    public bool IsNormalize { get; }

    public int SizeInBytesPerEntry => DataType.GetSize() * Size;
}