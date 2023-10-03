namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;

/// <summary>
/// The top level header representing meta data of a deckgl layer
/// </summary>
public class BaseHeaderDto
{
    /// <summary>
    /// The amount of bytes for the binary DeckGl data.
    /// </summary>
    public int TotalSizeInBytes { get; set; }

    /// <summary>
    /// The byte offset where the header is located at.
    /// The offset are always relative to the next top level header, never absolute.
    /// </summary>
    public int Offset { get; set; }
}