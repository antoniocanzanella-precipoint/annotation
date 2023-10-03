using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;

/// <summary>
/// Dto for the composite layer header. Describing meta information for composite deckgl layer.
/// </summary>
public class CompositeLayerHeaderDto : BaseLayerHeaderDto
{
    /// <summary>
    /// The custom type this layer represents.
    /// </summary>
    public DeckGlCustomLayerType CustomLayerType { get; set; }

    /// <summary>
    /// The subheaders of the composite header.
    /// </summary>
    public List<BaseLayerHeaderDto> CompositeLayerHeaders { get; set; }
}