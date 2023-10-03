namespace PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

/// <summary>
/// DataAccessor tell deckGl how to interpret the binary data
/// </summary>
public enum DeckGlDataAccessor : byte
{
    /// <summary>
    /// The binary data is a position vector
    /// </summary>
    GetPosition,

    /// <summary>
    /// The binary data is a radius vector
    /// </summary>
    GetRadius,

    /// <summary>
    /// The binary data contains the fill color
    /// </summary>
    GetFillColor,

    /// <summary>
    /// The binary data contains the color. The color of what is determined by the layer that uses this accessor
    /// </summary>
    GetColor,

    /// <summary>
    /// The binary data contains the line color
    /// </summary>
    GetLineColor,

    /// <summary>
    /// The binary data contains the line width
    /// </summary>
    GetLineWidth,

    /// <summary>
    /// The binary data contains the vector of vertices that describes the paths
    /// </summary>
    GetPath,

    /// <summary>
    /// The binary data contains the width
    /// </summary>
    GetWidth,

    /// <summary>
    /// The binary data contains the vector of vertices that describe the polygons
    /// </summary>
    GetPolygon,

    /// <summary>
    /// The binary data contains the elevation vector
    /// </summary>
    GetElevation,

    /// <summary>
    /// The binary data contains the text vector
    /// </summary>
    GetText
}