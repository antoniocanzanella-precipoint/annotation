using AutoMapper;
using PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper.ValueResolvers;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;

internal class AnnotationProfile : Profile
{
    public AnnotationProfile()
    {
        CreateMap<AnnotationShape, AnnotationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.AnnotationType, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.Radius, opt => opt.MapFrom(src => src.GetRadius()))
            .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Shape.Length))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Shape.Area))
            .ForMember(dest => dest.Coordinates, opt => opt.MapFrom(src => src.Shape.Coordinates))
            .ForMember(dest => dest.CounterGroups, opt => opt.MapFrom(src => src.CounterGroups))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.CanDelete, opt => opt.MapFrom<CanDeleteAnnotationByValueResolver>())
            .ForMember(dest => dest.CanEdit, opt => opt.MapFrom<CanEditAnnotationByValueResolver>())
            .ForMember(dest => dest.CanManageVisibility,
                opt => opt.MapFrom<CanManageAnnotationVisibilityByValueResolver>());

        CreateMap<AnnotationDto, AnnotationShape>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom<AnnotationIdByValueResolver>())
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.AnnotationType))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.Confidence, opt => opt.MapFrom(src => src.Confidence))
            .ForMember(dest => dest.SlideImageId, opt => opt.MapFrom(src => src.SlideImageId))
            .ForMember(dest => dest.Shape, opt => opt.MapFrom<ShapeValueResolver>())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));
    }
}