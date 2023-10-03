using PreciPoint.Ims.Services.Annotation.Application.Interfaces;

namespace PreciPoint.Ims.Services.Annotation.Database.Queries;

public class AQueryBase
{
    protected readonly IDbContext _annotationDbContext;

    public AQueryBase(IDbContext annotationDbContext)
    {
        _annotationDbContext = annotationDbContext;
    }
}