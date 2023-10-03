using AutoMapper;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper.ValueResolvers;

internal class ShapeValueResolver : IValueResolver<AnnotationDto, AnnotationShape, Geometry>
{
    private readonly GeometryFactory _geometryFactory;
    private readonly IMapper _mapper;

    public ShapeValueResolver(IMapper mapper, GeometryFactory geometryFactory)
    {
        _mapper = mapper;
        _geometryFactory = geometryFactory;
    }

    public Geometry Resolve(AnnotationDto source, AnnotationShape destination, Geometry destMember,
        ResolutionContext context)
    {
        destination.ConfigureGeometry(_mapper.Map<Coordinate[]>(source.Coordinates.ToArray()), _geometryFactory);

        return destination.Shape;
    }
}