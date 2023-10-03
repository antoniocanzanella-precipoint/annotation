using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Application.Configuration;

/// <summary>
/// Specifies how localization is managed.
/// </summary>
public class LocalizationConfig
{
    /// <summary>
    /// A list of all the cultures that are supported. First one will be default culture.
    /// </summary>
    public IReadOnlyList<string> SupportedCultures { get; set; }
}