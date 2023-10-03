using PreciPoint.Ims.Services.Annotation.Domain.Model;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Interfaces;

public interface IAnnotationQueries
{
    IQueryable<AnnotationShape> GetAnnotations(Expression<Func<AnnotationShape, bool>> filter);
    Task<AnnotationShape> GetAnnotationById(Guid annotationId, CancellationToken cancellationToken);
    IQueryable<AnnotationShape> GetAnnotationsNoTrack(Expression<Func<AnnotationShape, bool>> filter);
    IQueryable<AnnotationShape> GetAnnotationByIdNoTrack(Guid annotationId);
}