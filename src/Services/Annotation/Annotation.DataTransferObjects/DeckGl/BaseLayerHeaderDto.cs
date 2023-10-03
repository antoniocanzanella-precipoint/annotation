using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;

/// <summary>
/// Base class dto for headers that describe a layer
/// </summary>
public abstract class BaseLayerHeaderDto : BaseHeaderDto
{
    /// <summary>
    /// The annotation ids that are represented in the deckgl data
    /// </summary>
    public List<Guid> AnnotationIds { get; set; }

    /// <summary>
    /// The list of counter group ids for each annotation.
    /// </summary>
    public List<List<Guid>> CounterGroupIds { get; set; }

    /// <summary>
    /// Holds the counter group to counter mapping. Key is CounterGroup GUID. Value is a list of all counter ids of that
    /// counter group.
    /// </summary>
    public Dictionary<Guid, List<Guid>> CounterIds { get; set; }

    /// <summary>
    /// Access Rights for annotations in a bit-field manner.
    /// </summary>
    public List<AnnotationPermissionFlags> PermissionFlags { get; set; }

    /// <summary>
    /// Holds the labels for all annotations.
    /// </summary>
    public List<string> AnnotationLabels { get; set; }

    /// <summary>
    /// Holds the descriptions for all annotations.
    /// </summary>
    public List<string> AnnotationDescriptions { get; set; }

    /// <summary>
    /// Holds the types for all annotations. When all annotations in this layer are of the same type, there will only be one
    /// entry.
    /// </summary>
    public List<AnnotationType> AnnotationTypes { get; set; }

    /// <summary>
    /// The id of the deckgl layer
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The type of the layer
    /// </summary>
    public DeckGlLayerType Type { get; set; }

    /// <summary>
    /// The amount of annotations are represented in the deckgl data
    /// </summary>
    public int AmountOfEntries { get; set; }
}