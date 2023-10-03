using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Domain.Model;

public class SlideImage
{
    public SlideImage()
    {
        Annotations = new HashSet<AnnotationShape>();
    }

    public SlideImage(Guid slideImageId, Guid ownedBy)
    {
        SlideImageId = slideImageId;
        OwnedBy = ownedBy;
    }

    public SlideImage(Guid slideImageId, AnnotationPermission permission, Guid ownedBy)
    {
        SlideImageId = slideImageId;
        Permission = permission;
        OwnedBy = ownedBy;
    }

    public SlideImage(Guid slideImageId, Guid ownedBy, ICollection<AnnotationShape> annotations)
    {
        SlideImageId = slideImageId;
        OwnedBy = ownedBy;
        Annotations = annotations;
    }

    public Guid SlideImageId { get; }
    public AnnotationPermission Permission { get; set; } = AnnotationPermission.Disabled;
    public Guid OwnedBy { get; private set; }
    public ICollection<AnnotationShape> Annotations { get; }

    public void Update(Guid ownedBy)
    {
        OwnedBy = ownedBy;
    }
}