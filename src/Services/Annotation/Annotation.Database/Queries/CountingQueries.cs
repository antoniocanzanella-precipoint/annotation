using Microsoft.EntityFrameworkCore;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Database.Queries;

public class CountingQueries : AQueryBase, ICountingQueries
{
    public CountingQueries(IDbContext annotationDbContext) : base(annotationDbContext) { }

    public async Task<IEnumerable<CounterGroup>> GetAnnotationCounters(Guid annotationId,
        CancellationToken cancellationToken)
    {
        return await _annotationDbContext.Set<CounterGroup>()
            .Where(e => e.AnnotationId == annotationId)
            .Include(e => e.Counters)
            .ToListAsync(cancellationToken);
    }

    public async Task<CounterGroup> GetCounterGroupById(Guid counterGroupId, CancellationToken cancellationToken)
    {
        return await _annotationDbContext.Set<CounterGroup>()
            .Include(e => e.Counters)
            .Include(e => e.Annotation)
            .ThenInclude(e => e.SlideImage)
            .FirstOrDefaultAsync(e => e.Id == counterGroupId, cancellationToken);
    }

    public async Task<Counter> GetCounterById(Guid counterId, CancellationToken cancellationToken)
    {
        return await _annotationDbContext.Set<Counter>()
            .Include(e => e.CounterGroup)
            .ThenInclude(e => e.Annotation)
            .ThenInclude(e => e.SlideImage)
            .FirstOrDefaultAsync(e => e.Id == counterId, cancellationToken);
    }

    public async Task<IEnumerable<CounterGroup>> GetAnnotationCountersNoTrack(Guid annotationId,
        CancellationToken cancellationToken)
    {
        return await _annotationDbContext.Set<CounterGroup>()
            .Where(e => e.AnnotationId == annotationId)
            .Include(e => e.Counters)
            .ToListAsync(cancellationToken);
    }

    public async Task<CounterGroup> GetCounterGroupByIdNoTrack(Guid counterGroupId,
        CancellationToken cancellationToken)
    {
        return await _annotationDbContext.Set<CounterGroup>()
            .Include(e => e.Counters)
            .FirstOrDefaultAsync(e => e.Id == counterGroupId, cancellationToken);
    }

    public async Task<Counter> GetCounterByIdNoTrack(Guid counterId, CancellationToken cancellationToken)
    {
        return await _annotationDbContext.Set<Counter>()
            .Include(e => e.CounterGroup)
            .FirstOrDefaultAsync(e => e.Id == counterId, cancellationToken);
    }
}