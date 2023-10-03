using AutoMapper;
using PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper.ValueResolvers;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;

internal class FolderProfile : Profile
{
    public FolderProfile()
    {
        CreateMap<FolderDto, Folder>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom<FolderIdByValueResolver>())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.BriefDescription, opt => opt.MapFrom(src => src.BriefDescription));

        CreateMap<Folder, FolderDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.BriefDescription, opt => opt.MapFrom(src => src.BriefDescription));
    }
}