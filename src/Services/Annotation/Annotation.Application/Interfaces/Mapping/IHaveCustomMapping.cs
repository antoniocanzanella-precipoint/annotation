using AutoMapper;

namespace PreciPoint.Ims.Services.Annotation.Application.Interfaces.Mapping;

public interface IHaveCustomMapping
{
    void CreateMappings(Profile configuration);
}