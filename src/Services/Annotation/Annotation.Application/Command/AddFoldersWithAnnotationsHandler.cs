using AutoMapper;
using MediatR;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class AddFoldersWithAnnotations : IRequest<GenericCudOperationDto>
{
    public AddFoldersWithAnnotations(IList<FolderDto> folders)
    {
        Folders = folders;
    }

    public IList<FolderDto> Folders { get; }
}

public class AddFoldersWithAnnotationsHandler : IRequestHandler<AddFoldersWithAnnotations, GenericCudOperationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IMapper _mapper;

    public AddFoldersWithAnnotationsHandler(IDbContext annotationDbContext, IMapper mapper)
    {
        _annotationDbContext = annotationDbContext;
        _mapper = mapper;
    }

    public async Task<GenericCudOperationDto> Handle(AddFoldersWithAnnotations request,
        CancellationToken cancellationToken)
    {
        var entities = _mapper.Map<List<Folder>>(request.Folders);

        _annotationDbContext.Set<Folder>().AddRange(entities);

        return new GenericCudOperationDto(await _annotationDbContext.SaveChangesAsync(cancellationToken));
    }
}