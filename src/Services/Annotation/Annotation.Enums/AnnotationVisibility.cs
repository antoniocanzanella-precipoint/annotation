namespace PreciPoint.Ims.Services.Annotation.Enums;

/// <summary>
/// Describe the visibility of single annotation
/// </summary>
public enum AnnotationVisibility
{
    /// <summary>
    /// identify annotations visible to owner only
    /// </summary>
    Private,

    /// <summary>
    /// identify annotations visible to all users, all logged in users can read and modify
    /// </summary>
    Public,

    /// <summary>
    /// identify annotations that are only visible, the owner can change them
    /// </summary>
    ReadOnly,

    /// <summary>
    /// identify annotations that are visible and can be modified
    /// </summary>
    Editable
}