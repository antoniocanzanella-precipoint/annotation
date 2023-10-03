using System;

namespace PreciPoint.Ims.Services.Annotation.Domain;

public abstract class AEntityAuditable : AEntity
{
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public void IsCreated(Guid userId)
    {
        CreatedBy = userId;
        CreatedDate = DateTime.UtcNow;
    }

    public void IsModified(Guid userId)
    {
        LastModifiedBy = userId;
        LastModifiedDate = DateTime.UtcNow;
    }
}