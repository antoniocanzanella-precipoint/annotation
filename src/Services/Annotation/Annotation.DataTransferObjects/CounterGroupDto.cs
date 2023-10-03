using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// Describe a counter group linked to a specific annotation
/// </summary>
public class CounterGroupDto
{
    /// <summary>
    /// Entities could be distributed over different micro services
    /// so we stick with globally unique identifiers here.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Contains the label inserted by user
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Contains the description of a counter group.
    /// users can use this field to add details.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Contains the ordered list of point ids corresponding to element stored into coordinates object.
    /// </summary>
    public IReadOnlyList<Guid> CounterIds { get; set; }

    /// <summary>
    /// contains all counters belonging to this group
    /// </summary>
    public double[][] Counters { get; set; }

    /// <summary>
    /// Describe the annotation unique id.
    /// </summary>
    public Guid AnnotationId { get; set; }
}