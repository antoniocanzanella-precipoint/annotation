using PreciPoint.Ims.Services.Annotation.Enums;
using System;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// Describe the global annotation permission for the context user
/// </summary>
public class AnnotationPermissionsDto
{
    /// <summary>
    /// Identify the slide image
    /// </summary>
    public Guid SlideImageId { get; set; }

    /// <summary>
    /// Describe the permission set up
    /// </summary>
    public AnnotationPermission Permission { get; set; }

    /// <summary>
    /// Identify the owner of the slide image in the annotation context
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Define if the context user can draw annotation
    /// </summary>
    public bool CanDrawOnSlide { get; set; }

    /// <summary>
    /// Define if the context user can view annotation
    /// </summary>
    public bool CanView { get; set; }

    /// <summary>
    /// Define if the context user can change global annotation permission
    /// </summary>
    public bool CanChangeAccess { get; set; }
}