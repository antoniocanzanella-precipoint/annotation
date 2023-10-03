using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Interfaces;

public interface ISlideImageRepo
{
    Task<ApiPagedResponse<SlideImageDto>> GetAllSlideImages(int pageCounter, CancellationToken cancellationToken);
}