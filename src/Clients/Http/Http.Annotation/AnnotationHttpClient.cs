using PreciPoint.Ims.Core.DataTransfer.Http;
using System;

namespace PreciPoint.Ims.Clients.Http.Annotation;

/// <summary>
/// Annotation service consuming library. Class should provide unified access via HTTP to annotation resources.
/// </summary>
public class AnnotationHttpClient : AHttpClientContainer
{
    /// <summary>
    /// Always use class as singleton to ensure only one HTTP client is running simultaneously per domain.
    /// </summary>
    /// <param name="httpClient">Consumer must ensure to only provide one http client per application if threading is involved.</param>
    /// <param name="apiEndpoint">Where should we send the HTTP requests.</param>
    /// <exception cref="ArgumentNullException">If api endpoint is not given.</exception>
    public AnnotationHttpClient(AHttpClient httpClient, string apiEndpoint) : base(httpClient)
    {
        HttpApiClients.Add(AnnotationClient = new AnnotationClient(httpClient, apiEndpoint));
        HttpApiClients.Add(AdminClient = new AdminClient(httpClient, apiEndpoint));
    }

    /// <summary>
    /// Annotation responsible client.
    /// </summary>
    public AnnotationClient AnnotationClient { get; }

    /// <summary>
    /// ADMIN Annotation responsible client.
    /// </summary>
    public AdminClient AdminClient { get; }
}