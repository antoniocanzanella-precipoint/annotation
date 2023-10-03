using MediatR;
using Microsoft.EntityFrameworkCore;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class DeleteFolder : IRequest<DeleteOperationDto>
{
    public DeleteFolder(Guid folderId)
    {
        FolderId = folderId;
    }

    public Guid FolderId { get; }
}

public class DeleteFolderHandler : IRequestHandler<DeleteFolder, DeleteOperationDto>
{
    private readonly IDbContext _annotationDbContext;

    public DeleteFolderHandler(IDbContext annotationDbContext)
    {
        _annotationDbContext = annotationDbContext;
    }

    public async Task<DeleteOperationDto> Handle(DeleteFolder request,
        CancellationToken cancellationToken = default)
    {
        Folder folder = await _annotationDbContext.Set<Folder>().FirstOrDefaultAsync(e => e.Id == request.FolderId);

        var result = new DeleteOperationDto { NumberOfEntityRemoved = 0 };

        if (folder is null)
        {
            return result;
        }

        _annotationDbContext.Set<Folder>().Remove(folder);
        result.NumberOfEntityRemoved = await _annotationDbContext.SaveChangesAsync(cancellationToken);

        return result;
    }
}