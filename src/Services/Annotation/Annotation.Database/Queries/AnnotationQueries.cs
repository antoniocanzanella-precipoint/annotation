using Microsoft.EntityFrameworkCore;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Database.Queries;

public class AnnotationQueries : AQueryBase, IAnnotationQueries
{
    public AnnotationQueries(IDbContext annotationDbContext) : base(annotationDbContext) { }

    public IQueryable<AnnotationShape> GetAnnotations(Expression<Func<AnnotationShape, bool>> filter)
    {
        return _annotationDbContext.Set<AnnotationShape>()
            .Where(filter)
            .Include(e => e.SlideImage);
    }

    public async Task<AnnotationShape> GetAnnotationById(Guid annotationId, CancellationToken cancellationToken)
    {
        return await _annotationDbContext.Set<AnnotationShape>()
            .Include(e => e.SlideImage)
            .FirstOrDefaultAsync(e => e.Id == annotationId, cancellationToken);
    }

    public IQueryable<AnnotationShape> GetAnnotationsNoTrack(Expression<Func<AnnotationShape, bool>> filter)
    {
        return
            _annotationDbContext.Set<AnnotationShape>().AsNoTracking().AsSplitQuery()
                .Where(filter)
                .Include(e => e.SlideImage)
                .Include(e => e.CounterGroups)
                .ThenInclude(e => e.Counters);
    }

    public IQueryable<AnnotationShape> GetAnnotationByIdNoTrack(Guid annotationId)
    {
        return
            _annotationDbContext.Set<AnnotationShape>().AsNoTracking()
                .Where(e => e.Id == annotationId)
                .Include(e => e.SlideImage)
                .Include(e => e.CounterGroups)
                .ThenInclude(e => e.Counters);
    }
}