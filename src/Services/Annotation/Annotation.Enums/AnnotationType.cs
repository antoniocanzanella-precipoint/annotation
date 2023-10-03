namespace PreciPoint.Ims.Services.Annotation.Enums;

/// <summary>
/// Maps all possible annotation type
/// </summary>
public enum AnnotationType : byte
{
    /// <summary>
    /// Unique identify a point into cartesian plane
    /// </summary>
    Point,

    /// <summary>
    /// Unique identify a marker into cartesian plane.
    /// A marker unique identify a point and label it with text
    /// </summary>
    Marker,

    /// <summary>
    /// Unique identify a line into cartesian plane.
    /// </summary>
    Line,

    /// <summary>
    /// Unique identify a circle into cartesian plane
    /// </summary>
    Circle,

    /// <summary>
    /// Unique identify a rectangle into cartesian plane
    /// </summary>
    Rectangular,

    /// <summary>
    /// Unique identify a polygon into cartesian plane
    /// </summary>
    Polygon,

    /// <summary>
    /// Unique identify a polyline into cartesian plane
    /// </summary>
    Polyline,

    /// <summary>
    /// Unique identify a grid annotation into cartesian plane
    /// </summary>
    Grid
}