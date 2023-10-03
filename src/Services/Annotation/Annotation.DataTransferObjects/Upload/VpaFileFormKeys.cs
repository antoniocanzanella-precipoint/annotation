namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects.Upload;

/// <summary>
/// </summary>
public class VpaFileFormKeys
{
    /// <summary>
    /// The binary octet stream of the complete file we want to upload.
    /// </summary>
    public const string VpaFile = "vpaFile";

    /// <summary>
    /// The file name
    /// </summary>
    public const string FileName = "fileName";

    /// <summary>
    /// The slide image unique identifier
    /// </summary>
    public const string SlideImageId = "slideImageId";

    /// <summary>
    /// The flag indicates whether we need to respond with only the annotations uploaded
    /// or all annotations of the slide image an upload occured towards.
    /// </summary>
    public const string ReloadAllAnnotations = "reloadAllAnnotations";
}