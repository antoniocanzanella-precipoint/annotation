using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;

/// <summary>
/// The dto containing all of the headers to interpret the binary deckgl data. And the data itself.
/// </summary>
public class BinaryDataWithHeaderDto
{
    /// <summary>
    /// The headers describing how to read the binary data
    /// </summary>
    public IReadOnlyList<BaseLayerHeaderDto> Headers { get; set; }

    /// <summary>
    /// The binary data containing all of the good deckgl ones and zeros, used by the deckgl engine to show visualize
    /// annotations.
    /// </summary>
    public byte[] Data { get; set; }
}