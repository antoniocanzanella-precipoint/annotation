using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.DataTransferObjects;

/// <summary>
/// Describe the Annotation's Folder data exchanged with front-end and other system.
/// </summary>
public class FolderDto
{
    /// <summary>
    /// Entities could be distributed over different micro services
    /// so we stick with globally unique identifiers here.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Contains the name inserted by user
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Contains the description of a folder.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Contains the brief description of a folder.
    /// </summary>
    public string BriefDescription { get; set; }

    /// <summary>
    /// Describe if contains sub folders
    /// </summary>
    public bool HasSubFolders { get; set; }

    /// <summary>
    /// sub folders owned by this folder
    /// </summary>
    public IReadOnlyList<FolderDto> SubFolders { get; set; }

    /// <summary>
    /// Annotations contained by this folder
    /// </summary>
    public IReadOnlyList<AnnotationDto> Annotations { get; set; }
}