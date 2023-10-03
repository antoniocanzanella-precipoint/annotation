using PreciPoint.Ims.Core.Authorization.Config;
using PreciPoint.Ims.Services.Annotation.Domain.Configuration;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Application.Configuration;

public class ApplicationConfig
{
    /// <summary>
    /// Specifies the database connection parameters.
    /// </summary>
    public DatabasesConfig Databases { get; set; }

    /// <summary>
    /// Define the coefficient for points calculus
    /// </summary>
    public int CirclePointApproximationCoefficient { get; set; }

    /// <summary>
    /// Configure GZIP middleware compression, to be used only if kestrel is used directly
    /// </summary>
    public bool UseMiddlewareCompression { get; set; }

    /// <summary>
    /// Specifies from which addresses browsers are allowed to access the API.
    /// </summary>
    public IReadOnlyList<string> DomainsForCors { get; set; }

    /// <summary>
    /// default is 32 KB, we can tick this parameter to increase size
    /// </summary>
    public int MaximumReceiveMessageSizeInByte { get; set; }

    /// <summary>
    /// Allow strongly typed retrieval of OAuth2 configuration parameters.
    /// </summary>
    public OAuth2Config OAuth2 { get; set; }

    /// <summary>
    /// Specifies the parameter to connect to a message bus.
    /// </summary>
    public MessagingConfig MessagingConfig { get; set; }

    /// <summary>
    /// Contains metrics that define how the application reacts to decrease of performances
    /// </summary>
    public PerformanceBehaviour PerformanceBehaviour { get; set; }

    /// <summary>
    /// Specifies how localization is managed.
    /// </summary>
    public LocalizationConfig LocalizationConfig { get; set; }

    /// <summary>
    /// The parameters necessary to get a connection with the image management service.
    /// </summary>
    public HttpClientConfig ImageManagement { get; set; }
}