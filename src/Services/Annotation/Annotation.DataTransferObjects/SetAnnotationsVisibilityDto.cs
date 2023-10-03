using PreciPoint.Ims.Services.Annotation.Enums;
using System;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// Describe the annotation visibility
/// </summary>
public class SetAnnotationsVisibilityDto
{
    /// <summary>
    /// Unique identify the slide image.
    /// </summary>
    public Guid SlideImageId { get; set; }

    /// <summary>
    /// describe the access level of annotation. <see cref="AnnotationVisibility" />
    /// </summary>
    public AnnotationVisibility Visibility { get; set; }
}