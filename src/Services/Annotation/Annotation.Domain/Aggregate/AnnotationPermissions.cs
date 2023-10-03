using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.Aggregate;

public class AnnotationPermissions
{
    public AnnotationPermissions(SlideImage slideImage, Guid contextUserId)
    {
        SlideImage = slideImage;
        CanDrawOnSlide = CanDrawOnSlideCalculate(slideImage, contextUserId);
        CanView = CanViewCalculate(slideImage, contextUserId);
        CanChangeAccess = CanChangeAccessCalculate(slideImage, contextUserId);
    }

    public SlideImage SlideImage { get; }
    public bool CanDrawOnSlide { get; }
    public bool CanView { get; }
    public bool CanChangeAccess { get; }

    private bool CanDrawOnSlideCalculate(SlideImage slideImage, Guid contextUserId)
    {
        if (slideImage.OwnedBy == contextUserId)
        {
            return true;
        }

        if (slideImage.Permission == AnnotationPermission.Draw && DomainConstants.AnonymousId != contextUserId)
        {
            return true;
        }

        return false;
    }

    private bool CanViewCalculate(SlideImage slideImage, Guid contextUserId)
    {
        if (slideImage.OwnedBy == contextUserId)
        {
            return true;
        }

        if (slideImage.Permission is AnnotationPermission.View or AnnotationPermission.Draw)
        {
            return true;
        }

        return false;
    }

    private bool CanChangeAccessCalculate(SlideImage slideImage, Guid contextUserId)
    {
        if (slideImage.OwnedBy == contextUserId)
        {
            return true;
        }

        return false;
    }
}