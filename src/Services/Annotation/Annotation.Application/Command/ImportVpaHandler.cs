using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Meta;
using PreciPoint.Ims.Core.FluentValidation.Extensions;
using PreciPoint.Ims.Services.Annotation.Application.Common;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Infrastructure.VPA;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Application.Queries;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ImportEntity = PreciPoint.Ims.Services.Annotation.Domain.Model.Import;
using Polygon = PreciPoint.Ims.Services.Annotation.Application.Infrastructure.VPA.Polygon;
using Type = PreciPoint.Ims.Services.Annotation.Application.Infrastructure.VPA.Type;

namespace PreciPoint.Ims.Services.Annotation.Application.Command;

public class ImportVpa : IRequest<IReadOnlyList<AnnotationDto>>
{
    public ImportVpa(Guid slideImageId, IFormFile fileToImport, string fileName, bool reloadAllAnnotations)
    {
        SlideImageId = slideImageId;
        FileToImport = fileToImport;
        FileName = fileName;
        ReloadAllAnnotations = reloadAllAnnotations;
    }

    public Guid SlideImageId { get; }
    public IFormFile FileToImport { get; }
    public string FileName { get; }
    public bool ReloadAllAnnotations { get; }
}

public class ImportVpaHandler : IRequestHandler<ImportVpa, IReadOnlyList<AnnotationDto>>
{
    private readonly IDbContext _annotationDbContext;
    private readonly IAnnotationQueries _annotationQueries;
    private readonly AnnotationQueryFilter _annotationQueryFilter;
    private readonly IClaimsPrincipalProvider _claimsPrincipalProvider;
    private readonly GeometryFactory _geometryFactory;
    private readonly ILogger<ImportVpaHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _stringLocalizer;

    public ImportVpaHandler(ILogger<ImportVpaHandler> logger, IAnnotationQueries annotationQueries,
        AnnotationQueryFilter annotationQueryFilter, IStringLocalizer stringLocalizer, IDbContext annotationDbContext,
        IClaimsPrincipalProvider claimsPrincipalProvider, IMapper mapper, GeometryFactory geometryFactory)
    {
        _logger = logger;
        _annotationQueries = annotationQueries;
        _annotationQueryFilter = annotationQueryFilter;
        _stringLocalizer = stringLocalizer;
        _annotationDbContext = annotationDbContext;
        _claimsPrincipalProvider = claimsPrincipalProvider;
        _mapper = mapper;
        _geometryFactory = geometryFactory;
    }

    public async Task<IReadOnlyList<AnnotationDto>> Handle(ImportVpa request,
        CancellationToken cancellationToken = default)
    {
        AnnotationsVpaMapping document = Deserialize(request);

        await using IDbContextTransaction transaction =
            await _annotationDbContext.GetDatabase().BeginTransactionAsync(cancellationToken);

        try
        {
            ImportEntity import = PersistImport(GetByte(request), request);
            PersistPolygon(document, request, import);
            PersistPolyline(document, request, import);
            PersistCountings(document, request, import);
            PersistMarkers(document, request, import);
            PersistLines(document, request, import);
            PersistCircles(document, request, import);
            PersistRectangle(document, request, import);

            int rowInserted = await _annotationDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"added {rowInserted} elements from vpa file.");

            await transaction.CommitAsync(cancellationToken);

            if (request.ReloadAllAnnotations)
            {
                var requestGetAnnotations = new GetAnnotations(request.SlideImageId);

                return await new GetAnnotationsHandler(_annotationQueries, _mapper, _annotationQueryFilter,
                        _claimsPrincipalProvider, _annotationDbContext, _stringLocalizer)
                    .Handle(requestGetAnnotations, cancellationToken);
            }

            List<AnnotationShape> annotationList = await _annotationDbContext.Set<AnnotationShape>().AsNoTracking()
                .Where(annotation =>
                    annotation.SlideImageId == request.SlideImageId && annotation.ImportId == import.Id)
                .Include(e => e.SlideImage)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<AnnotationDto>>(annotationList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during persistence of data from .vpa file. Transaction was rolled back.");

            await transaction.RollbackAsync(cancellationToken);

            string message = _stringLocalizer["APPLICATION.ANNOTATIONS.IMPORT_VPA.PERSISTENCE"];
            throw new MessageOnly(message).ToApiException(HttpStatusCode.InternalServerError);
        }
    }

    private ImportEntity PersistImport(byte[] file, ImportVpa request)
    {
        var entity = new ImportEntity
        {
            File = file,
            FileName = request.FileName,
            CreatedBy = _claimsPrincipalProvider.Current.UserId,
            CreatedDate = DateTime.UtcNow
        };

        _annotationDbContext.Set<ImportEntity>().Add(entity);

        return entity;
    }

    private AnnotationsVpaMapping Deserialize(ImportVpa request)
    {
        using var streamReader = new StreamReader(request.FileToImport.OpenReadStream());
        var serializer = new XmlSerializer(typeof(AnnotationsVpaMapping));

        return (AnnotationsVpaMapping) serializer.Deserialize(streamReader);
    }

    private byte[] GetByte(ImportVpa request)
    {
        using var streamReader = new StreamReader(request.FileToImport.OpenReadStream());

        using var memoryStream = new MemoryStream();
        streamReader.BaseStream.CopyTo(memoryStream);
        byte[] file = memoryStream.ToArray();

        return file;
    }

    private void PersistPolygon(AnnotationsVpaMapping document, ImportVpa request, ImportEntity import)
    {
        //Older vpa files might have different structure
        if (document.Polygons == null)
        {
            return;
        }

        foreach (Polygon element in document.Polygons.Polygon)
        {
            var annotation = new AnnotationShape
            {
                Id = Guid.NewGuid(),
                SlideImageId = request.SlideImageId,
                Type = AnnotationType.Polygon,
                IsVisible = bool.Parse(element.IsVisible),
                Label = element.Description,
                Description = element.Description,
                Import = import
            };

            annotation.ConfigureGeometry(
                ConvertCoordinates(element.Points, true), _geometryFactory
            );
            annotation.IsCreated(_claimsPrincipalProvider.Current.UserId);

            _annotationDbContext.Set<AnnotationShape>().Add(annotation);
        }
    }

    private void PersistCountings(AnnotationsVpaMapping document, ImportVpa request, ImportEntity import)
    {
        //Older vpa files might have different structure
        if (document.Countings == null)
        {
            return;
        }

        foreach (Counting element in document.Countings.Counting)
        {
            var annotation = new AnnotationShape
            {
                Id = Guid.NewGuid(),
                SlideImageId = request.SlideImageId,
                Type = AnnotationType.Polygon,
                IsVisible = bool.Parse(element.IsVisible),
                Label = element.Description,
                Description = element.Description,
                Import = import
            };


            annotation.ConfigureGeometry(
                ConvertCoordinates(element.Points, true), _geometryFactory
            );
            annotation.IsCreated(_claimsPrincipalProvider.Current.UserId);

            _annotationDbContext.Set<AnnotationShape>().Add(annotation);

            foreach (Type species in element.Species.Type)
            {
                if (species.Points.Length > 0)
                {
                    CounterGroup counterGroup = new()
                    {
                        AnnotationId = annotation.Id,
                        Label = species.Name,
                        Description = species.Name
                    };
                    counterGroup.IsCreated(_claimsPrincipalProvider.Current.UserId);

                    foreach (string point in species.Points.Split(" "))
                    {
                        string[] coordinates = point.Split(",");
                        var counter = new Counter
                        {
                            Shape =
                                _geometryFactory.CreatePoint(
                                    new Coordinate(
                                        double.Parse(coordinates[0]),
                                        double.Parse(coordinates[1])
                                    ))
                        };
                        counterGroup.Counters.Add(counter);
                    }

                    _annotationDbContext.Set<CounterGroup>().Add(counterGroup);
                }
            }
        }
    }

    private void PersistPolyline(AnnotationsVpaMapping document, ImportVpa request, ImportEntity import)
    {
        //Older vpa files might have different structure
        if (document.Polylines == null)
        {
            return;
        }

        foreach (Polyline element in document.Polylines.Polyline)
        {
            var annotation = new AnnotationShape
            {
                Id = Guid.NewGuid(),
                SlideImageId = request.SlideImageId,
                Type = AnnotationType.Polyline,
                IsVisible = bool.Parse(element.IsVisible),
                Label = element.Description,
                Description = element.Description,
                Import = import
            };

            annotation.ConfigureGeometry(
                ConvertCoordinates(element.Points, false), _geometryFactory
            );
            annotation.IsCreated(_claimsPrincipalProvider.Current.UserId);

            _annotationDbContext.Set<AnnotationShape>().Add(annotation);
        }
    }

    private void PersistMarkers(AnnotationsVpaMapping document, ImportVpa request, ImportEntity import)
    {
        //Older vpa files might have different structure
        if (document.Markers == null)
        {
            return;
        }

        foreach (Marker element in document.Markers.Marker)
        {
            var annotation = new AnnotationShape
            {
                Id = Guid.NewGuid(),
                SlideImageId = request.SlideImageId,
                Type = AnnotationType.Marker,
                IsVisible = bool.Parse(element.IsVisible),
                Label = element.Description,
                Description = element.Description,
                Import = import
            };

            annotation.ConfigureGeometry(
                ConvertCoordinates(element.Points, false), _geometryFactory
            );
            annotation.IsCreated(_claimsPrincipalProvider.Current.UserId);

            _annotationDbContext.Set<AnnotationShape>().Add(annotation);
        }
    }

    private void PersistLines(AnnotationsVpaMapping document, ImportVpa request, ImportEntity import)
    {
        //Older vpa files might have different structure
        if (document.Lines == null)
        {
            return;
        }

        foreach (Line element in document.Lines.Line)
        {
            var annotation = new AnnotationShape
            {
                Id = Guid.NewGuid(),
                SlideImageId = request.SlideImageId,
                Type = AnnotationType.Line,
                IsVisible = bool.Parse(element.IsVisible),
                Label = element.Description,
                Description = element.Description,
                Import = import
            };

            annotation.ConfigureGeometry(
                ConvertCoordinates(element.Points, false), _geometryFactory
            );
            annotation.IsCreated(_claimsPrincipalProvider.Current.UserId);

            _annotationDbContext.Set<AnnotationShape>().Add(annotation);
        }
    }

    private void PersistCircles(AnnotationsVpaMapping document, ImportVpa request, ImportEntity import)
    {
        //Older vpa files might have different structure
        if (document.Circles == null)
        {
            return;
        }

        foreach (Circle element in document.Circles.Circle)
        {
            var annotation = new AnnotationShape
            {
                Id = Guid.NewGuid(),
                SlideImageId = request.SlideImageId,
                Type = AnnotationType.Circle,
                IsVisible = bool.Parse(element.IsVisible),
                Label = element.Description,
                Description = element.Description,
                Import = import
            };

            annotation.ConfigureGeometry(
                ConvertCoordinatesForCircle(element.Points), _geometryFactory
            );
            annotation.IsCreated(_claimsPrincipalProvider.Current.UserId);

            _annotationDbContext.Set<AnnotationShape>().Add(annotation);
        }
    }

    private void PersistRectangle(AnnotationsVpaMapping document, ImportVpa request, ImportEntity import)
    {
        //TODO-AC:remove after branch with Rectangles are merged into master. Older vpa files might have different structure
        if (document.Rectangles == null)
        {
            return;
        }

        foreach (Rectangle element in document.Rectangles.Rectangle)
        {
            var annotation = new AnnotationShape
            {
                Id = Guid.NewGuid(),
                SlideImageId = request.SlideImageId,
                Type = AnnotationType.Rectangular,
                IsVisible = bool.Parse(element.IsVisible),
                Label = element.Description,
                Description = element.Description,
                Import = import
            };

            annotation.ConfigureGeometry(
                ConvertCoordinates(element.Points, true), _geometryFactory
            );
            annotation.IsCreated(_claimsPrincipalProvider.Current.UserId);

            _annotationDbContext.Set<AnnotationShape>().Add(annotation);
        }
    }

    private static Coordinate[] ConvertCoordinates(string pointsFromVpa, bool extendForClosedShapes)
    {
        string[] vpaCoordinate = pointsFromVpa.Split(" ");

        Coordinate[] coordinates = extendForClosedShapes
            ? new Coordinate[vpaCoordinate.Length + 1]
            : new Coordinate[vpaCoordinate.Length];

        for (var i = 0; i < vpaCoordinate.Length; i++)
        {
            string[] values = vpaCoordinate[i].Split(",");
            coordinates[i] = new Coordinate(double.Parse(values[0]), double.Parse(values[1]));
        }

        if (extendForClosedShapes)
        {
            string[] values = vpaCoordinate[0].Split(",");
            coordinates[vpaCoordinate.Length] = new Coordinate(double.Parse(values[0]), double.Parse(values[1]));
        }

        return coordinates;
    }

    private static Coordinate[] ConvertCoordinatesForCircle(string pointsFromVpa)
    {
        string[] vpaCoordinate = pointsFromVpa.Split(" ");

        var coordinates = new Coordinate[2];

        string[] values = vpaCoordinate[0].Split(",");
        var p1 = new Point(double.Parse(values[0]), double.Parse(values[1]));

        values = vpaCoordinate[1].Split(",");
        var p2 = new Point(double.Parse(values[0]), double.Parse(values[1]));

        values = vpaCoordinate[2].Split(",");
        var p3 = new Point(double.Parse(values[0]), double.Parse(values[1]));

        MathExpression.GetCircleInformation(p1, p2, p3, out Point center, out double radius);

        coordinates[0] = new Coordinate(center.X, center.Y);
        coordinates[1] = new Coordinate(center.X, center.Y + radius);

        return coordinates;
    }
}