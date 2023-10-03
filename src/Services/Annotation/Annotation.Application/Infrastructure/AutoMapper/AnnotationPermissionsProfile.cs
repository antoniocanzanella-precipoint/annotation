using AutoMapper;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Aggregate;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;

public class AnnotationPermissionsProfile : Profile
{
    public AnnotationPermissionsProfile()
    {
        CreateMap<AnnotationPermissions, AnnotationPermissionsDto>()
            .ForMember(dest => dest.SlideImageId, opt => opt.MapFrom(src => src.SlideImage.SlideImageId))
            .ForMember(dest => dest.Permission, opt => opt.MapFrom(src => src.SlideImage.Permission))
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.SlideImage.OwnedBy))
            .ForMember(dest => dest.CanChangeAccess, opt => opt.MapFrom(src => src.CanChangeAccess))
            .ForMember(dest => dest.CanView, opt => opt.MapFrom(src => src.CanView))
            .ForMember(dest => dest.CanDrawOnSlide, opt => opt.MapFrom(src => src.CanDrawOnSlide));
    }
}