using AutoMapper;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;

internal class SlideImageProfile : Profile
{
    public SlideImageProfile()
    {
        CreateMap<SlideImageDto, SlideImage>()
            .ForMember(dest => dest.SlideImageId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OwnedBy, opt => opt.MapFrom(src => src.OwnedBy));
    }
}