using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Interfaces;

public interface ICountingQueries
{
    Task<IEnumerable<CounterGroup>> GetAnnotationCounters(Guid annotationId, CancellationToken cancellationToken);
    Task<CounterGroup> GetCounterGroupById(Guid counterGroupId, CancellationToken cancellationToken);
    Task<Counter> GetCounterById(Guid counterId, CancellationToken cancellationToken);

    Task<IEnumerable<CounterGroup>> GetAnnotationCountersNoTrack(Guid annotationId,
        CancellationToken cancellationToken);

    Task<CounterGroup> GetCounterGroupByIdNoTrack(Guid counterGroupId, CancellationToken cancellationToken);
    Task<Counter> GetCounterByIdNoTrack(Guid counterId, CancellationToken cancellationToken);
}