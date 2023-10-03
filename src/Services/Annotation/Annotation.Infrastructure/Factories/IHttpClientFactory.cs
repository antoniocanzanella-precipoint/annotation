using PreciPoint.Ims.Clients.Http.ImageManagement;

namespace PreciPoint.Ims.Services.Annotation.Infrastructure.Factories;

/// <summary>
/// Creates HTTP clients required for synchronous inter service communication.
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// Usually only a single image management HTTP client is sufficient.
    /// </summary>
    /// <param name="clientIndex">Currently not relevant as there's only one central image management service.</param>
    /// <returns>An HTTP client responsible for image management service interaction.</returns>
    public ImageManagementHttpClient GetOrAddImageManagementHttpClient(int clientIndex = 0);
}