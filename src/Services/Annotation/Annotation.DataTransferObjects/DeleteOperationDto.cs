namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// Describe the result of a delete operation
/// </summary>
public class DeleteOperationDto
{
    /// <summary>
    /// Contains the number of entity removed on a single delete request
    /// </summary>
    public int NumberOfEntityRemoved { get; set; }
}