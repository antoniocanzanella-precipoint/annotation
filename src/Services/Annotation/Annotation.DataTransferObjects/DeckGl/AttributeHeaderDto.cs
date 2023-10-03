using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;

/// <summary>
/// Dto for a deckgl attribute
/// </summary>
public class AttributeHeaderDto : BaseHeaderDto
{
    /// <summary>
    /// The Datatype of the attribute (float, double, UInt8, ...)
    /// </summary>
    public PrimitiveDataType DataType { get; set; }

    /// <summary>
    /// The amount of bytes one datatype takes. For example UInt8 takes 1 byte
    /// </summary>
    public byte SizeOfDataType { get; set; }

    /// <summary>
    /// The amount of values that make up one entry.
    /// For example an RGBA color is represented by 4 values one for R, G, B and A.
    /// </summary>
    public byte Size { get; set; }

    /// <summary>
    ///     <see cref="DeckGlDataAccessor" />
    /// </summary>
    public DeckGlDataAccessor DataAccessor { get; set; }

    /// <summary>
    /// Whether data values should be normalized. Note that all color attributes in deck.gl layers are normalized by default
    /// </summary>
    public bool IsNormalized { get; set; }
}