using System;

namespace PreciPoint.Ims.Services.Annotation.Enums.DeckGl;

/// <summary>
/// Bit permission flags to mark annotation permissions, just like <see cref="AnnotationPermission" />.
/// </summary>
[Flags]
public enum AnnotationPermissionFlags : byte
{
    /// <summary>
    /// No flags are set
    /// </summary>
    None = 0,

    /// <summary>
    /// CanDelete Flag
    /// </summary>
    CanDelete = 1,

    /// <summary>
    /// CanEdit Flag
    /// </summary>
    CanEdit = 2,

    /// <summary>
    /// CanManageVisibility Flag
    /// </summary>
    CanManageVisibility = 4
}