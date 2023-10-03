using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Domain.Model;

public class CounterGroup : AEntityAuditable
{
    public CounterGroup()
    {
        Counters = new HashSet<Counter>();
    }

    public string Label { get; set; }
    public string Description { get; set; }
    public ICollection<Counter> Counters { get; set; }
    public Guid AnnotationId { get; set; }
    public AnnotationShape Annotation { get; set; }

    public IReadOnlyList<Guid> GetCounterIdList()
    {
        return Counters.Select(x => x.Id).ToList();
    }

    public Coordinate[] GetCoordinateArray()
    {
        return Counters.Select(x => x.Shape.Coordinates).SelectMany(i => i).ToArray();
    }
}