using System;

namespace PreciPoint.Ims.Services.Annotation.Domain;

public abstract class AEntity
{
    public virtual Guid Id { get; set; }
}