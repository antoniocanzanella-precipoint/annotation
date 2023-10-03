namespace PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

/// <summary>
/// Enumeration of deckgl layers. For more information see
/// <see href="https://deck.gl/docs/api-reference/layers#core-layers">deckgl layer catalog</see>
/// </summary>
public enum DeckGlLayerType : byte
{
    /// <summary>
    /// Scatterplot layer
    /// </summary>
    Scatterplot,

    /// <summary>
    /// Path layer
    /// </summary>
    Path,

    /// <summary>
    /// Text layer
    /// </summary>
    Text,

    /// <summary>
    /// Counter layer
    /// </summary>
    Counter,

    /// <summary>
    /// Composite layer
    /// </summary>
    Composite,

    /// <summary>
    /// Polygon Layer
    /// </summary>
    Polygon
}