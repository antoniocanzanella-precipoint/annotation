using FluentValidation;
using PreciPoint.Ims.Services.Annotation.Domain;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class UpsertAnnotationValidator : AbstractValidator<UpsertAnnotation>
{
    public UpsertAnnotationValidator()
    {
        RuleFor(x => x.SlideImageId).NotEmpty();
        RuleFor(x => x.AnnotationDto).NotNull();
        RuleFor(x => x.AnnotationDto.AnnotationType).NotNull();
        RuleFor(x => x.AnnotationDto.Visibility).NotNull();
        RuleFor(x => x.AnnotationDto.Label).MaximumLength(DomainConstants.AnnotationLabelLength);
        RuleFor(x => x.AnnotationDto.Description).MaximumLength(DomainConstants.AnnotationDescriptionLength);
    }
}