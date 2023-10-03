using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Queries;

public class GetFolders : IRequest<IReadOnlyList<FolderDto>>
{
    public GetFolders(Guid slideImageId)
    {
        SlideImageId = slideImageId;
    }

    public Guid SlideImageId { get; }
}

public class GetFoldersHandler : IRequestHandler<GetFolders, IReadOnlyList<FolderDto>>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public GetFoldersHandler(IMapper mapper, IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider, IStringLocalizer stringLocalizer)
    {
        _mapper = mapper;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<IReadOnlyList<FolderDto>> Handle(GetFolders request, CancellationToken cancellationToken)
    {
        await BusinessValidation.CheckUserReadPermissionBySlideImageId(_annotationDbContext, _claimsPrincipalProvider,
            request.SlideImageId, _stringLocalizer, cancellationToken);

        List<Folder> folderList = await _annotationDbContext.Set<Folder>()
            .Where(e => e.Annotations.Any(annotation => annotation.SlideImageId == request.SlideImageId))
            .Include(e => e.Annotations)
            .OrderBy(e => e.DisplayOder)
            .ToListAsync(cancellationToken);

        ILookup<Guid?, Folder> lookup = folderList.ToLookup(folder => folder.ParentFolderId);

        foreach (Folder folder in folderList)
        {
            folder.SubFolders = lookup[folder.Id].ToList();
        }

        return _mapper.Map<List<FolderDto>>(folderList.Where(e => e.ParentFolderId.HasValue is false).ToList());
    }
}