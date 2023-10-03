namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// Describe a generic class used to update the annotation vertex
/// </summary>
public class VertexUpdateDto
{
    /// <summary>
    /// contains the coordinates that must be updated/inserted
    /// </summary>
    public double[][] CoordinatesDto { get; set; }

    /// <summary>
    /// indicates the index of coordinates array of the shape where the modification of vertices must start.
    /// </summary>
    public int Index { get; set; }
}