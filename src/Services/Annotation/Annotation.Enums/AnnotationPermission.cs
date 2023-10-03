namespace PreciPoint.Ims.Services.Annotation.Enums;

/// <summary>
/// Define
/// </summary>
public enum AnnotationPermission
{
    /// <summary>
    /// Annotation service is disabled, clients should not request for AnnotationDto as it’s useless.
    /// If requested, Annotation service just answers with [].
    /// </summary>
    Disabled,

    /// <summary>
    /// Only the owner of the slide image can draw, all other users can read only.
    /// </summary>
    View,

    /// <summary>
    /// Users can draw annotations.
    /// </summary>
    Draw
}