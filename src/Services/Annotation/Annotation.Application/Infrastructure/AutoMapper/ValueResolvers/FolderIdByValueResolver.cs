using AutoMapper;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Infrastructure.AutoMapper.ValueResolvers;

internal class FolderIdByValueResolver : IValueResolver<FolderDto, Folder, Guid>
{
    public Guid Resolve(FolderDto source, Folder destination, Guid destMember, ResolutionContext context)
    {
        return source.Id.HasValue ? source.Id.Value : Guid.NewGuid();
    }
}