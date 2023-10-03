namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// the class wrap the operation done by backend.
/// Create, update and delete
/// </summary>
public class GenericCudOperationDto
{
    /// <summary>
    /// at least must contain the rows involved
    /// </summary>
    /// <param name="affectedRows"></param>
    public GenericCudOperationDto(int affectedRows)
    {
        AffectedRows = affectedRows;
    }

    /// <summary>
    /// contains the number of rows involved into a single transaction
    /// </summary>
    public int AffectedRows { get; }
}