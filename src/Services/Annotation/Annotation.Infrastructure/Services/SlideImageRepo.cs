using PreciPoint.Ims.Clients.Http.ImageManagement;
using PreciPoint.Ims.Clients.Http.ImageManagement.Params;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Core.Enums;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Infrastructure.Factories;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Infrastructure.Services;

public class SlideImageRepo : ISlideImageRepo
{
    private readonly ImageManagementHttpClient _imageManagementHttpClient;

    public SlideImageRepo(IHttpClientFactory httpClientFactory)
    {
        _imageManagementHttpClient = httpClientFactory.GetOrAddImageManagementHttpClient();
    }

    public async Task<ApiPagedResponse<SlideImageDto>> GetAllSlideImages(int pageCounter, CancellationToken cancellationToken = default)
    {
        var filter = new FindAllSlideImagesParams { Page = pageCounter, DateTimeMatch = DateTimeMatch.NotNull };

        return await _imageManagementHttpClient.SlideImageClient.FindSlideImages(filter);
    }
}