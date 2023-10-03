using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// Describe the Annotation data exchanged with front-end and other system.
/// </summary>
public class AnnotationDto
{
    /// <summary>
    /// Entities could be distributed over different micro services
    /// so we stick with globally unique identifiers here.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Unique identify the slide image.
    /// </summary>
    public Guid SlideImageId { get; set; }

    /// <summary>
    /// Contains the label inserted by user
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Contains the description of an annotation.
    /// users can use this field to add more details.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Contains information about colors, clients can decide to the format (RGB, RGBA...)
    /// </summary>
    public int[] Color { get; set; }

    /// <summary>
    /// Contains information about confidence value used by machine learning models.
    /// </summary>
    public double? Confidence { get; set; }

    /// <summary>
    /// Describe the annotation type. <see cref="AnnotationType" />
    /// </summary>
    public AnnotationType AnnotationType { get; set; }

    /// <summary>
    /// describe the access level of annotation. <see cref="AnnotationVisibility" />
    /// </summary>
    public AnnotationVisibility Visibility { get; set; }

    /// <summary>
    /// Contains the radius value if the annotation is of type circle
    /// </summary>
    public double Radius { get; set; }

    /// <summary>
    /// Contains the area value for closed shapes
    /// </summary>
    public double Area { get; set; }

    /// <summary>
    /// Contains the length of opened shapes and the border length of closed ones
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// Contains the list of coordinates that describe the annotation shape
    /// </summary>
    public double[][] Coordinates { get; set; }

    /// <summary>
    /// Contains the counter groups associated to this annotation
    /// </summary>
    public IReadOnlyList<CounterGroupDto> CounterGroups { get; set; }

    /// <summary>
    /// Identify the user that created the entity, will be the owner also
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// The creation date-time
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Define if the context user can delete annotation
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Define if the context user can edit annotation.
    /// Can change shape, can move, can change color etc
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Define if the context user can change annotation visibility.
    /// <see cref="AnnotationVisibility" />
    /// </summary>
    public bool CanManageVisibility { get; set; }
}