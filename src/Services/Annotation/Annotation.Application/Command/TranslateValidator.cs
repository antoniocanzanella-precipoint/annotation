using FluentValidation;
using Microsoft.Extensions.Localization;
using System.Net;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class TranslateValidator : AbstractValidator<Translate>
{
    public TranslateValidator(IStringLocalizer stringLocalizer)
    {
        RuleFor(x => x.Dto.AnnotationIds).NotNull();
        RuleFor(x => x.Dto.AnnotationIds)
            .Must(collection => collection is { Count: > 0 })
            .WithMessage(stringLocalizer["APPLICATION.ANNOTATIONS.EMPTY_LIST"])
            .WithErrorCode(HttpStatusCode.BadRequest.ToString());
    }
}