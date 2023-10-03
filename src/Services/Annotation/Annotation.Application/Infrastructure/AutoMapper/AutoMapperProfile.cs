using AutoMapper;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Services.Annotation.Application.Command;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces.Mapping;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System.Collections.Generic;
using System.Reflection;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        LoadStandardMappings();
        LoadCustomMappings();
        LoadConverters();
    }

    private void LoadConverters()
    {
        CreateMap<Coordinate, double[]>()
            .ConvertUsing(src => new[] { src.X, src.Y });

        CreateMap<double[], Coordinate>()
            .ForMember(dest => dest.X, opt => opt.MapFrom(src => src[0]))
            .ForMember(dest => dest.Y, opt => opt.MapFrom(src => src[1]));

        CreateMap<CounterGroup, CounterGroupDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.AnnotationId, opt => opt.MapFrom(src => src.AnnotationId))
            .ForMember(x => x.CounterIds, opt => opt.MapFrom(src => src.GetCounterIdList()))
            .ForMember(x => x.Counters, opt => opt.MapFrom(src => src.GetCoordinateArray()));

        CreateMap<UpsertAnnotation, AnnotationShape>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AnnotationDto.Id))
            .ForMember(dest => dest.SlideImageId, opt => opt.MapFrom(src => src.SlideImageId))
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.AnnotationDto.Label))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.AnnotationDto.Description))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.AnnotationDto.AnnotationType))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.AnnotationDto.Visibility));

        CreateMap<UpsertCounterGroup, CounterGroup>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Dto.Id))
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Dto.Label))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Dto.Description))
            .ForMember(dest => dest.AnnotationId, opt => opt.MapFrom(src => src.Dto.AnnotationId));
    }

    private void LoadStandardMappings()
    {
        IList<Map> mapsFrom = MapperProfileHelper.LoadStandardMappings(Assembly.GetExecutingAssembly());

        foreach (Map map in mapsFrom)
        {
            CreateMap(map.Source, map.Destination).ReverseMap();
        }
    }

    private void LoadCustomMappings()
    {
        IList<IHaveCustomMapping> mapsFrom = MapperProfileHelper.LoadCustomMappings(Assembly.GetExecutingAssembly());

        foreach (IHaveCustomMapping map in mapsFrom)
        {
            map.CreateMappings(this);
        }
    }
}