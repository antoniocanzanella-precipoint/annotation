using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;

/// <summary>
/// Dto for 'simple' deckgl layers. Simple meaning the basic layers that deckgl offers out of the box.
/// </summary>
public class LayerHeaderDto : BaseLayerHeaderDto
{
    /// <summary>
    /// The attributes of this layer.
    /// </summary>
    public Dictionary<DeckGlDataAccessor, AttributeHeaderDto> AttributeHeaders { get; set; }

    /// <summary>
    /// The start indices used by deckgl.
    /// </summary>
    public int[] StartIndices { get; set; }

    /// <summary>
    /// The amount of vertices that are present in the binary data.
    /// </summary>
    public int VertexCount { get; set; }
}