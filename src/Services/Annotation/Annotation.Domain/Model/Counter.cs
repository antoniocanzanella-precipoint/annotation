using NetTopologySuite.Geometries;
using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.Model;

public class Counter : AEntity
{
    public Geometry Shape { get; set; }
    public Guid GroupCounterId { get; set; }
    public CounterGroup CounterGroup { get; set; }
}