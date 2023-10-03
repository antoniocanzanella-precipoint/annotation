using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class SetColor : IRequest<AnnotationDto>
{
    public SetColor(AnnotationDto dto)
    {
        Dto = dto;
    }

    public AnnotationDto Dto { get; }
}

public class SetColorHandler : IRequestHandler<SetColor, AnnotationDto>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public SetColorHandler(IDbContext annotationDbContext, IMapper mapper,
        IClaimsPrincipalProvider claimsPrincipalProvider, IAnnotationQueries annotationQueries,
        IStringLocalizer stringLocalizer)
    {
        _annotationDbContext = annotationDbContext;
        _mapper = mapper;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _annotationQueries = annotationQueries;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<AnnotationDto> Handle(SetColor request, CancellationToken cancellationToken)
    {
        if (request.Dto.Id.HasValue == false)
        {
            string message = _stringLocalizer["APPLICATION.ANNOTATIONS.NOT_NULL_ID"];
            throw new MessageOnly(message).ToApiException();
        }


        AnnotationShape annotationToUpdate = await BusinessValidation.CheckIfAnnotationExist(_annotationQueries,
            request.Dto.Id.Value, _stringLocalizer, cancellationToken);

        BusinessValidation.CheckUserWritePermission(annotationToUpdate, _claimsPrincipalProvider, _stringLocalizer);

        BusinessValidation.CheckColor(request.Dto.Color, _stringLocalizer);

        annotationToUpdate.Color = request.Dto.Color;

        _annotationDbContext.Set<AnnotationShape>().Update(annotationToUpdate);
        await _annotationDbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AnnotationDto>(annotationToUpdate);
    }
}