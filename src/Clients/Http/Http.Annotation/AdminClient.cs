using PreciPoint.Ims.Core.DataTransfer.Http;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation;

/// <summary>
/// ADMIN Annotation responsible client.
/// </summary>
public class AdminClient : AHttpApiClient
{
    internal AdminClient(AHttpClient httpClient, string apiEndpoint) : base(httpClient, apiEndpoint) { }

    private string AnnotationsEndpoint => $"{ApiEndpoint}/Annotation";

   /// <summary>
    /// Consumers can use this to bootstrap the synchronization of tables
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public Task<ApiResponse<GenericCudOperationDto>> SyncSlideImagesAndUsers(CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/synchronize";
        return HttpClient.PutJsonAsync<ApiResponse<GenericCudOperationDto>>(requestUrl, null, null, cancellationToken);
    }
}