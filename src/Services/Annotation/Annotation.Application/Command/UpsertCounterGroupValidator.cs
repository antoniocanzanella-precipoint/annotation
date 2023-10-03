using FluentValidation;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Services.Annotation.Domain;
using System.Net;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class UpsertCounterGroupValidator : AbstractValidator<UpsertCounterGroup>
{
    public UpsertCounterGroupValidator(IStringLocalizer stringLocalizer)
    {
        RuleFor(x => x.Dto.AnnotationId).NotEmpty();
        RuleFor(x => x.Dto.Label).NotNull();
        RuleFor(x => x.Dto.Label)
            .MaximumLength(DomainConstants.GroupCounterLabelLength);
        RuleFor(x => x.Dto.Description)
            .MaximumLength(DomainConstants.GroupCounterDescriptionLength);
        RuleFor(x => x.Dto.Counters)
            .Must(collection => collection is { Length: > 0 })
            .WithMessage(stringLocalizer["APPLICATION.ANNOTATIONS.EMPTY_COORDINATE_LIST"])
            .WithErrorCode(HttpStatusCode.BadRequest.ToString());
    }
}