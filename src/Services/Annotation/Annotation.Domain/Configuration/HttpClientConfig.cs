namespace PreciPoint.Ims.Services.Annotation.Domain.Configuration;

/// <summary>
/// Contains information about the HTTP connection properties necessary to connect to another service.
/// </summary>
public class HttpClientConfig
{
    /// <summary>
    /// The protocol of the corresponding service.
    /// </summary>
    public string Protocol { get; set; }

    /// <summary>
    /// The hostname of the corresponding service.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// The port of the corresponding service.
    /// </summary>
    public int Port { get; set; }
}