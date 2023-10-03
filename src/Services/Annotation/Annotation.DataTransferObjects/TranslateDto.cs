using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// Describe the request for annotation translate
/// </summary>
public class TranslateDto
{
    /// <summary>
    /// contains all identifiers of annotation to move
    /// </summary>
    public IReadOnlyList<Guid> AnnotationIds { get; set; }

    /// <summary>
    /// the x axis value
    /// </summary>
    public double DeltaX { get; set; }

    /// <summary>
    /// the x axis value
    /// </summary>
    public double DeltaY { get; set; }
}