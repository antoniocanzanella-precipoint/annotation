using BenchmarkDotNet.Attributes;
using FluentAssertions;
using IdentityModel.Client;
using NUnit.Framework;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Extensions;
using PreciPoint.Ims.Clients.Http.ImageManagement;
using PreciPoint.Ims.Clients.Http.WholeSlideImages;
using PreciPoint.Ims.Core.DataTransfer.Config;
using PreciPoint.Ims.Core.DataTransfer.Mapping;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Core.IdentityModel.Tokens;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using PreciPoint.Ims.Shared.DataTransferObjects.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Integration;

[TestFixture]
[Order(18)]
[NonParallelizable]
[Category("Integration")]
public class I018DeckGl : ABaseTest
{
    [OneTimeSetUp]
    public void SetUp()
    {
        AssertionOptions.AssertEquivalencyUsing(x =>
        {
            x.WithStrictOrdering();
            return x;
        });

        _apiUrl = $"{Configuration.AnnotationHost}/api";
        var apiUrlIms = $"{Configuration.ImageManagementHost}/api";

        var tokenReq = new PasswordTokenRequestUser
        {
            UserName = Configuration.User01.UserName,
            Password = Configuration.User01.Password,
            ClientId = Configuration.User01.ClientId
        };

        var passwordTokenRequest = new PasswordTokenRequest
        {
            Address = Configuration.SecuritySystem.TokenUrl,
            UserName = tokenReq.UserName,
            Password = tokenReq.Password,
            ClientId = tokenReq.ClientId,
            ClientSecret = Configuration.SecuritySystem.ClientSecrets[tokenReq.ClientId],
            Scope = Configuration.SecuritySystem.Scope
        };


        var httpClient = new MessagePackIdentityHttpClient(new TokenRequestService(passwordTokenRequest), new KeycloakUserInfoMapper());

        _annotationHttpClient = new AnnotationHttpClient(httpClient, _apiUrl);
        _adminImageManagementHttpClient = new ImageManagementHttpClient(HttpClientFactory.CreateAdminHttpClient(Configuration), apiUrlIms);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _annotationHttpClient.Dispose();
    }

    [TearDown]
    public async Task TearDown()
    {
        await Cleanup();
    }

    private string _apiUrl;
    private readonly Random _random = new(1337);

    private AnnotationHttpClient _annotationHttpClient;
    private ImageManagementHttpClient _adminImageManagementHttpClient;
    private ApiResponse<SlideImageDto> _slideImage;

    private async Task<List<AnnotationDto>> InsertPoints(int amountOfEntries)
    {
        var annotas = new List<AnnotationDto>();
        for (var i = 0; i < amountOfEntries; i++)
        {
            AnnotationDto dto = CreateAnnotation(AnnotationType.Point, AnnotationVisibility.Private);
            dto.Coordinates =
                new[] { new[] { _random.NextDouble(), _random.NextDouble() } };

            ApiResponse<AnnotationDto> response = await _annotationHttpClient.AnnotationClient.InsertAnnotation(dto, _slideImage.Data.Id);
            annotas.Add(response.Data);
        }

        return annotas;
    }

    private async Task<List<AnnotationDto>> InsertCircles(int amountOfEntries)
    {
        var annotas = new List<AnnotationDto>();
        for (var i = 0; i < amountOfEntries; i++)
        {
            AnnotationDto dto = CreateAnnotation(AnnotationType.Circle, AnnotationVisibility.Private);
            dto.Coordinates =
                new[] { new[] { _random.NextDouble(), _random.NextDouble() }, new[] { _random.NextDouble(), _random.NextDouble() } };

            ApiResponse<AnnotationDto> response = await _annotationHttpClient.AnnotationClient.InsertAnnotation(dto, _slideImage.Data.Id);
            annotas.Add(response.Data);
        }

        return annotas;
    }

    private async Task<List<AnnotationDto>> InsertLines(int amountOfEntries)
    {
        var annotas = new List<AnnotationDto>();
        for (var i = 0; i < amountOfEntries; i++)
        {
            AnnotationDto dto = CreateAnnotation(AnnotationType.Line, AnnotationVisibility.Private);
            dto.Coordinates =
                new[] { new[] { _random.NextDouble(), _random.NextDouble() }, new[] { _random.NextDouble(), _random.NextDouble() } };

            ApiResponse<AnnotationDto> response = await _annotationHttpClient.AnnotationClient.InsertAnnotation(dto, _slideImage.Data.Id);
            annotas.Add(response.Data);
        }

        return annotas;
    }

    private async Task<IReadOnlyList<AnnotationDto>> InsertPolyLines(int amountOfEntries)
    {
        var ids = new List<AnnotationDto>();
        for (var i = 0; i < amountOfEntries; i++)
        {
            AnnotationDto dto = CreateAnnotation(AnnotationType.Polyline, AnnotationVisibility.Private);
            dto.Coordinates =
                new[]
                {
                    new[] { _random.NextDouble(), _random.NextDouble() }, new[] { _random.NextDouble(), _random.NextDouble() },
                    new[] { _random.NextDouble(), _random.NextDouble() }, new[] { _random.NextDouble(), _random.NextDouble() },
                    new[] { _random.NextDouble(), _random.NextDouble() }
                }; // 5 Vertices

            ApiResponse<AnnotationDto> response = await _annotationHttpClient.AnnotationClient.InsertAnnotation(dto, _slideImage.Data.Id);
            ids.Add(response.Data);
        }

        return ids;
    }

    private async Task<List<AnnotationDto>> InsertPolygons(int amountOfEntries)
    {
        var ids = new List<AnnotationDto>();
        for (var i = 0; i < amountOfEntries; i++)
        {
            AnnotationDto dto = CreateAnnotation(AnnotationType.Polygon, AnnotationVisibility.Private);
            double[] start = new[] { _random.NextDouble(), _random.NextDouble() };
            dto.Coordinates =
                new[]
                {
                    start, new[] { _random.NextDouble(), _random.NextDouble() }, new[] { _random.NextDouble(), _random.NextDouble() },
                    new[] { _random.NextDouble(), _random.NextDouble() }, new[] { _random.NextDouble(), _random.NextDouble() },
                    new[] { _random.NextDouble(), _random.NextDouble() }, start
                }; // 7 vertices

            ApiResponse<AnnotationDto> result = await _annotationHttpClient.AnnotationClient.InsertAnnotation(dto, _slideImage.Data.Id);
            ids.Add(result.Data);
        }

        return ids;
    }

    private async Task<IReadOnlyList<AnnotationDto>> InsertRectangle(int amountOfEntries)
    {
        var ids = new List<AnnotationDto>();
        for (var i = 0; i < amountOfEntries; i++)
        {
            AnnotationDto dto = CreateAnnotation(AnnotationType.Rectangular, AnnotationVisibility.Private);
            double[] start = new[] { _random.NextDouble(), _random.NextDouble() };
            dto.Coordinates =
                new[]
                {
                    start, new[] { _random.NextDouble(), _random.NextDouble() }, new[] { _random.NextDouble(), _random.NextDouble() },
                    new[] { _random.NextDouble(), _random.NextDouble() }, start
                }; // 5 vertices
            ApiResponse<AnnotationDto> result = await _annotationHttpClient.AnnotationClient.InsertAnnotation(dto, _slideImage.Data.Id);
            ids.Add(result.Data);
        }

        return ids;
    }

    private async Task<IReadOnlyList<Guid>> InsertCounterGroup(Guid annotationId)
    {
        var ids = new List<Guid>();
        CounterGroupDto counterGroupDto = CreateCounterGroup(annotationId);
        double[][] counterArray = new[]
        {
            new[] { _random.NextDouble(), _random.NextDouble() }, new[] { _random.NextDouble(), _random.NextDouble() },
            new[] { _random.NextDouble(), _random.NextDouble() }
        };
        counterGroupDto.Counters = counterArray;
        ApiResponse<CounterGroupDto> response = await _annotationHttpClient.AnnotationClient.InsertCounterGroup(counterGroupDto);
        ids.AddRange(response.Data.CounterIds);

        return ids;
    }

    private async Task<IReadOnlyList<AnnotationDto>> InsertMarker(int amountOfEntries)
    {
        var annotas = new List<AnnotationDto>();
        for (var i = 0; i < amountOfEntries; i++)
        {
            AnnotationDto marker = CreateAnnotation(AnnotationType.Marker, AnnotationVisibility.Private);
            marker.Coordinates = new[]
            {
                new[] { _random.NextDouble() * 100, _random.NextDouble() * 100 }, new[] { _random.NextDouble() * 100, _random.NextDouble() * 100 }
            };
            ApiResponse<AnnotationDto> response = await _annotationHttpClient.AnnotationClient.InsertAnnotation(marker, _slideImage.Data.Id);
            annotas.Add(response.Data);
        }

        return annotas;
    }

    private DeckGlLayerType AnnotationTypeToLayerType(AnnotationType type)
    {
        switch (type)
        {
            case AnnotationType.Marker:
                return DeckGlLayerType.Composite;
            case AnnotationType.Point:
            case AnnotationType.Circle:
                return DeckGlLayerType.Scatterplot;
            case AnnotationType.Line:
            case AnnotationType.Polyline:
                return DeckGlLayerType.Path;
            case AnnotationType.Rectangular:
            case AnnotationType.Polygon:
                return DeckGlLayerType.Polygon;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private string AnnotationTypeToId(DeckGlLayerType type, AnnotationType annotationType)
    {
        switch (type)
        {
            case DeckGlLayerType.Scatterplot:
                return DeckGlLayerId.AnnotationsCircleLayer.ToString();
            case DeckGlLayerType.Path:

                if (annotationType == AnnotationType.Polygon || annotationType == AnnotationType.Rectangular)
                {
                    return DeckGlLayerId.AnnotationsPolygonLayer.ToString();
                }

                return DeckGlLayerId.AnnotationsPolyLineLayer.ToString();
            case DeckGlLayerType.Text:
            case DeckGlLayerType.Counter:
            case DeckGlLayerType.Composite:
                return DeckGlLayerId.AnnotationsMarkerLayer.ToString();
            case DeckGlLayerType.Polygon:
                return DeckGlLayerId.AnnotationsPolygonLayer.ToString();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void AssertLayerHeader(LayerHeaderDto header, IReadOnlyList<AnnotationDto> annotations, int offset,
        bool isSubLayer, DeckGlLayerType type, string headerId)
    {
        AssertBaseLayerHeaderProperties(header, annotations, offset, type, headerId, isSubLayer);

        int vertexCount;
        // special edge case, because marker path layer does not need start indices, because of MC magic :)
        if (headerId.StartsWith(DeckGlLayerId.AnnotationsMarkerLayer.ToString()) && type == DeckGlLayerType.Path)
        {
            header.StartIndices.Should().BeNull();
            vertexCount = header.AmountOfEntries;
        }
        else if (HasStartIndices(header.Type))
        {
            header.StartIndices.Length.Should().Be(header.AmountOfEntries);
            if (header.Type == DeckGlLayerType.Text)
            {
                vertexCount = annotations.Sum(x => x.Label.EnumerateRunes().Count());
            }
            else
            {
                vertexCount = annotations.Sum(x => x.Coordinates.Length);
            }
        }
        else
        {
            header.StartIndices.Should().BeNull();
            vertexCount = header.AmountOfEntries;
        }

        header.VertexCount.Should().Be(vertexCount);

        AssertAttributeHeaders(header);
    }

    private void AssertAttributeHeaders(LayerHeaderDto header)
    {
        var attrHeaderOffset = 0;
        foreach (KeyValuePair<DeckGlDataAccessor, AttributeHeaderDto> attrHeader in header.AttributeHeaders)
        {
            attrHeader.Key.Should().Be(attrHeader.Value.DataAccessor);

            AssertAttributeHeader(attrHeader.Value, header.Id, attrHeaderOffset);

            attrHeaderOffset += attrHeader.Value.TotalSizeInBytes;
        }
    }

    private void AssertAttributeHeader(AttributeHeaderDto header, string headerId, int offset)
    {
        switch (header.DataAccessor)
        {
            case DeckGlDataAccessor.GetPosition:
                header.Size.Should().Be(2);
                header.DataType.Should().Be(PrimitiveDataType.Float);
                header.IsNormalized.Should().Be(false);
                header.SizeOfDataType.Should().Be(sizeof(float));
                header.Offset.Should().Be(offset);
                break;
            case DeckGlDataAccessor.GetRadius:
                header.Size.Should().Be(1);
                header.DataType.Should().Be(PrimitiveDataType.Float);
                header.IsNormalized.Should().Be(false);
                header.SizeOfDataType.Should().Be(sizeof(float));
                header.Offset.Should().Be(offset);
                break;
            case DeckGlDataAccessor.GetFillColor:
            case DeckGlDataAccessor.GetColor:
            case DeckGlDataAccessor.GetLineColor:
                header.Size.Should().Be(4);
                header.DataType.Should().Be(PrimitiveDataType.UInt8);
                header.IsNormalized.Should().Be(true);
                header.SizeOfDataType.Should().Be(sizeof(byte));
                header.Offset.Should().Be(offset);
                break;
            case DeckGlDataAccessor.GetPath:
                if (headerId.StartsWith(DeckGlLayerId.AnnotationsMarkerLayer.ToString()))
                {
                    // we have marker
                    header.Size.Should().Be(4);
                }
                else
                {
                    header.Size.Should().Be(2);
                }

                header.DataType.Should().Be(PrimitiveDataType.Float);
                header.IsNormalized.Should().Be(false);
                header.SizeOfDataType.Should().Be(sizeof(float));
                header.Offset.Should().Be(offset);
                break;
            case DeckGlDataAccessor.GetWidth:
                header.Size.Should().Be(1);
                header.DataType.Should().Be(PrimitiveDataType.UInt8);
                header.IsNormalized.Should().Be(false);
                header.SizeOfDataType.Should().Be(sizeof(byte));
                header.Offset.Should().Be(offset);
                break;
            case DeckGlDataAccessor.GetPolygon:
                header.Size.Should().Be(2);
                header.DataType.Should().Be(PrimitiveDataType.Float);
                header.IsNormalized.Should().Be(false);
                header.SizeOfDataType.Should().Be(sizeof(float));
                header.Offset.Should().Be(offset);
                break;
            case DeckGlDataAccessor.GetLineWidth:
            case DeckGlDataAccessor.GetElevation:
                // there should never be this accessor, because its never used
                Assert.Fail($"{header.DataAccessor} should never appear!");
                break;
            case DeckGlDataAccessor.GetText:
                header.Size.Should().Be(1);
                header.DataType.Should().Be(PrimitiveDataType.UInt32);
                header.IsNormalized.Should().Be(false);
                header.SizeOfDataType.Should().Be(sizeof(uint));
                header.Offset.Should().Be(offset);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool HasStartIndices(DeckGlLayerType type)
    {
        return type == DeckGlLayerType.Path || type == DeckGlLayerType.Polygon || type == DeckGlLayerType.Text;
    }

    private void AssertBaseLayerHeaderProperties(BaseLayerHeaderDto header, IReadOnlyList<AnnotationDto> annotations,
        int offset, DeckGlLayerType type, string headerId, bool isSubLayer = false)
    {
        AnnotationDto annota = annotations.First();

        header.Type.Should().Be(type);
        header.AmountOfEntries.Should().Be(annotations.Count);
        header.Offset.Should().Be(offset);
        header.TotalSizeInBytes.Should().BeGreaterThan(0);
        header.Id.Should().Be(headerId);
        // sublayer should not contain some attributes, because it is already present in composite parent header
        if (isSubLayer)
        {
            header.AnnotationTypes.Should().BeNull();
            header.AnnotationDescriptions.Should().BeNull();
            header.AnnotationLabels.Should().BeNull();
            header.AnnotationIds.Should().BeNull();
            header.CounterGroupIds.Should().BeNull();
            header.PermissionFlags.Should().BeNull();
            header.CounterIds.Should().BeNull();
        }
        else
        {
            // if the same annotation type is present, it is compressed to 1 entry in the header
            if (annotations.Select(x => x.AnnotationType).Distinct().Count() == 1)
            {
                header.AnnotationTypes.Should()
                    .BeEquivalentTo(new[] { annota.AnnotationType });
            }
            else
            {
                header.AnnotationTypes.Should().BeEquivalentTo(annotations.Select(x => x.AnnotationType));
            }

            header.AnnotationDescriptions.Should().BeEquivalentTo(annotations.Select(x => x.Description));
            header.AnnotationLabels.Should().BeEquivalentTo(annotations.Select(x => x.Label));
            header.AnnotationIds.Should().BeEquivalentTo(annotations.Select(x => x.Id));
            List<List<Guid?>> counterGroupIds = annotations.Select(x => { return x.CounterGroups.Select(x => x.Id).ToList(); })
                .ToList();
            bool allEmpty = counterGroupIds.Count(x => x.Count > 0) == 0;
            if (allEmpty)
            {
                header.CounterGroupIds.Should().BeNull();
            }
            else
            {
                header.CounterGroupIds.Should()
                    .BeEquivalentTo(counterGroupIds);
            }

            header.PermissionFlags.Should()
                .BeEquivalentTo(annotations.Select(BuildPermissionFlagsFromAnnotaDto).ToList());
            Dictionary<Guid, IReadOnlyList<Guid>> counterGroups = annotations.SelectMany(x => x.CounterGroups)
                .ToDictionary(x => x.Id.Value, x => x.CounterIds);
            if (counterGroups.Count == 0)
            {
                header.CounterIds.Should().BeNull();
            }
            else
            {
                header.CounterIds.Should()
                    .BeEquivalentTo(counterGroups);
            }
        }
    }

    private AnnotationPermissionFlags BuildPermissionFlagsFromAnnotaDto(AnnotationDto dto)
    {
        var flags = AnnotationPermissionFlags.None;

        if (dto.CanDelete)
        {
            flags |= AnnotationPermissionFlags.CanDelete;
        }

        if (dto.CanEdit)
        {
            flags |= AnnotationPermissionFlags.CanEdit;
        }

        if (dto.CanManageVisibility)
        {
            flags |= AnnotationPermissionFlags.CanManageVisibility;
        }

        return flags;
    }

    private void AssertHeader(BaseLayerHeaderDto header, IReadOnlyList<object> annotations, int offset)
    {
        if (header.Type == DeckGlLayerType.Composite)
        {
            header.Should().BeOfType<CompositeLayerHeaderDto>();
            AssertMarkerLayer(header as CompositeLayerHeaderDto, annotations.Cast<AnnotationDto>().ToList(), offset);
        }
        else if (header.Type == DeckGlLayerType.Counter)
        {
            List<CounterGroupDto> counters = annotations.Cast<CounterGroupDto>().ToList();
            header.Should().BeOfType<LayerHeaderDto>();
            AssertCounterLayer(header as LayerHeaderDto, counters, offset);
        }
        else
        {
            List<AnnotationDto> annota = annotations.Cast<AnnotationDto>().ToList();
            header.Should().BeOfType<LayerHeaderDto>();
            DeckGlLayerType type = AnnotationTypeToLayerType(annota.First().AnnotationType);
            string headerId = AnnotationTypeToId(type, annota.First().AnnotationType);
            AssertLayerHeader(header as LayerHeaderDto, annota, offset, false, type, headerId);
        }
    }

    private void AssertCounterLayer(LayerHeaderDto header, List<CounterGroupDto> counters, int offset)
    {
        header.StartIndices.Should().BeNull();
        header.VertexCount.Should().Be(header.AmountOfEntries);
        header.Id.Should().Be(DeckGlLayerId.AnnotationsCounterLayer.ToString());
        header.Offset.Should().Be(offset);
        header.Type.Should().Be(DeckGlLayerType.Counter);
        header.AnnotationDescriptions.Should().BeEquivalentTo(counters.Select(x => x.Description));
        header.AnnotationIds.Should().BeEquivalentTo(counters.SelectMany(x => x.CounterIds));
        header.AnnotationLabels.Should().BeEquivalentTo(counters.Select(x => x.Label));
        header.AnnotationTypes.Should().BeNull();
        List<List<Guid>> counterGroupIds = counters.Select(x => new List<Guid> { x.Id!.Value }).ToList();
        header.CounterGroupIds.Should().BeEquivalentTo(counterGroupIds);
        Dictionary<Guid, List<Guid>> counterIdsDict = counters.ToDictionary(x => x.Id!.Value, x => x.CounterIds.ToList());
        header.CounterIds.Should().BeEquivalentTo(counterIdsDict);
        header.PermissionFlags.Should().BeNull();
        header.AmountOfEntries.Should().Be(header.AnnotationIds.Count);
        header.TotalSizeInBytes.Should().BeGreaterThan(0);

        AssertAttributeHeaders(header);
    }

    private void AssertMarkerLayer(CompositeLayerHeaderDto header, IReadOnlyList<AnnotationDto> annotations,
        int offset)
    {
        var markerId = DeckGlLayerId.AnnotationsMarkerLayer.ToString();
        AssertBaseLayerHeaderProperties(header, annotations, offset, DeckGlLayerType.Composite, markerId);

        header.CustomLayerType.Should().Be(DeckGlCustomLayerType.Marker);
        BaseLayerHeaderDto textLayer = header.CompositeLayerHeaders.First(x => x.Type == DeckGlLayerType.Text);
        BaseLayerHeaderDto pathLayer = header.CompositeLayerHeaders.First(x => x.Type == DeckGlLayerType.Path);
        int indexOf = header.CompositeLayerHeaders.IndexOf(textLayer);
        textLayer.Should().NotBeNull();
        pathLayer.Should().NotBeNull();
        string textLayerId = markerId + "-text-layer-id";
        string pathLayerId = markerId + "-path-layer-id";
        // textLayer is first
        if (indexOf == 0)
        {
            AssertLayerHeader(textLayer as LayerHeaderDto, annotations, 0, true, DeckGlLayerType.Text, textLayerId);
            AssertLayerHeader(pathLayer as LayerHeaderDto, annotations, textLayer.TotalSizeInBytes, true,
                DeckGlLayerType.Path, pathLayerId);
        }
        else
        {
            AssertLayerHeader(pathLayer as LayerHeaderDto, annotations, 0, true, DeckGlLayerType.Path, pathLayerId);
            AssertLayerHeader(textLayer as LayerHeaderDto, annotations, pathLayer.TotalSizeInBytes, true,
                DeckGlLayerType.Text, textLayerId);
        }
    }

    [Test]
    [Order(0)]
    public async Task I018_000UploadSlideImages()
    {
        _slideImage = await SuggestSlideImage();
        var wholeSlideImagesAdmin = new WholeSlideImagesHttpClient(
            HttpClientFactory.CreateAdminHttpClient(Configuration),
            _slideImage.Data.Storage.ClientAccessPathRoot + "/api");

        ApiResponse<UploadProgressDto<SlideImageDto>> uploadProgress =
            await wholeSlideImagesAdmin.UploadClient.UploadWholeSlideImage(GetFileToUploadAbsPath(),
                _slideImage.Data.Id, new NullProgressReporter());

        Assert.NotNull(uploadProgress.Data.Entity.CreatedAt);
    }

    private void AssertBinaryDataWithHeaders(BinaryDataWithHeaderDto dto,
        params IReadOnlyList<object>[] annotationDtos)
    {
        dto.Headers.Count.Should().Be(annotationDtos.Length);
        dto.Data.Length.Should().Be(dto.Headers.Sum(x => x.TotalSizeInBytes));

        var offset = 0;
        for (var i = 0; i < annotationDtos.Length; i++)
        {
            BaseLayerHeaderDto header = dto.Headers[i];
            IReadOnlyList<object> annotas = annotationDtos[i];
            AssertHeader(header, annotas, offset);
            offset += header.TotalSizeInBytes;
        }
    }

    [Test]
    [Order(1)]
    public async Task I018_001EmptyAnnotations()
    {
        BinaryDataWithHeaderDto response = await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);
        response.Headers.Should().BeEmpty();
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Order(2)]
    public async Task I018_002OnlyPoints_On_CircleLayer()
    {
        var amountOfEntries = 10;
        List<AnnotationDto> annotas = await InsertPoints(amountOfEntries);

        BinaryDataWithHeaderDto response =
            await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);

        BaseLayerHeaderDto header = response.Headers.First();

        // we order the inserted annotations to match the order they are present in the header to make comparing easier
        annotas = annotas.OrderBy(x => header.AnnotationIds.IndexOf(x.Id!.Value)).ToList();

        AssertBinaryDataWithHeaders(response, annotas);
    }

    [Test]
    [Order(3)]
    public async Task I018_003OnlyCircle_On_CircleLayer()
    {
        var amountOfEntries = 10;
        List<AnnotationDto> circles = await InsertCircles(amountOfEntries);

        BinaryDataWithHeaderDto response =
            await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);


        BaseLayerHeaderDto header = response.Headers.First();

        // we order the inserted annotations to match the order they are present in the header to make comparing easier
        circles = circles.OrderBy(x => header.AnnotationIds.IndexOf(x.Id!.Value)).ToList();

        AssertBinaryDataWithHeaders(response, circles);
    }

    [Test]
    [Order(4)]
    public async Task I018_004Point_And_Circle_On_CircleLayer()
    {
        var amountOfPoints = 5;
        var amountOfCircles = 6;
        List<AnnotationDto> points = await InsertPoints(amountOfPoints);
        List<AnnotationDto> circles = await InsertCircles(amountOfCircles);

        BinaryDataWithHeaderDto response =
            await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);

        BaseLayerHeaderDto header = response.Headers.First();

        // we order the inserted annotations to match the order they are present in the header to make comparing easier
        List<AnnotationDto> annotas = points.Concat(circles).OrderBy(x => header.AnnotationIds.IndexOf(x.Id!.Value)).ToList();

        AssertBinaryDataWithHeaders(response, annotas);
    }

    [Test]
    [Order(5)]
    public async Task I018_005Line_On_PolyLineLayer()
    {
        var amountOfEntries = 10;
        List<AnnotationDto> lines = await InsertLines(amountOfEntries);

        BinaryDataWithHeaderDto response =
            await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);

        BaseLayerHeaderDto header = response.Headers.First();

        // we order the inserted annotations to match the order they are present in the header to make comparing easier
        lines = lines.OrderBy(x => header.AnnotationIds.IndexOf(x.Id!.Value)).ToList();

        AssertBinaryDataWithHeaders(response, lines);
    }

    [Test]
    [Order(6)]
    public async Task I018_006PolyLine_On_PolyLineLayer()
    {
        var amountOfEntries = 10;
        IReadOnlyList<AnnotationDto> polyLines = await InsertPolyLines(amountOfEntries);

        BinaryDataWithHeaderDto response =
            await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);

        BaseLayerHeaderDto header = response.Headers.First();

        // we order the inserted annotations to match the order they are present in the header to make comparing easier
        polyLines = polyLines.OrderBy(x => header.AnnotationIds.IndexOf(x.Id!.Value)).ToList();
        AssertBinaryDataWithHeaders(response, polyLines);
    }

    [Test]
    [Order(7)]
    public async Task I018_007Polygon_Without_Counters_On_PolygonLayer()
    {
        var amountOfEntries = 10;
        List<AnnotationDto> polygons = await InsertPolygons(amountOfEntries);

        BinaryDataWithHeaderDto response = await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);

        BaseLayerHeaderDto header = response.Headers.First();

        // we order the inserted annotations to match the order they are present in the header to make comparing easier
        polygons = polygons.OrderBy(x => header.AnnotationIds.IndexOf(x.Id!.Value)).ToList();

        AssertBinaryDataWithHeaders(response, polygons);
    }

    [Test]
    [Order(8)]
    public async Task I018_008PolygonWithCountersOnPathLayer()
    {
        AnnotationDto annota =
            await CreateGenericPolygon(_annotationHttpClient, AnnotationVisibility.Public, _slideImage.Data.Id);
        CounterGroupDto counter = await CreateGenericCounter(annota.Id!.Value, _annotationHttpClient);

        annota.CounterGroups = new List<CounterGroupDto> { counter };

        BinaryDataWithHeaderDto response = await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);

        if (response.Headers.First().Type == DeckGlLayerType.Counter)
        {
            AssertBinaryDataWithHeaders(response, new[] { counter }, new[] { annota });
        }
        else
        {
            AssertBinaryDataWithHeaders(response, new[] { annota }, new[] { counter });
        }
    }

    [Test]
    [Order(9)]
    public async Task I018_009RectangleWithoutCountersOnPathLayer()
    {
        var amountOfEntries = 10;
        IReadOnlyList<AnnotationDto> rectangles = await InsertRectangle(amountOfEntries);

        BinaryDataWithHeaderDto response = await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);
        BaseLayerHeaderDto header = response.Headers.First();

        // we order the inserted annotations to match the order they are present in the header to make comparing easier
        rectangles = rectangles.OrderBy(x => header.AnnotationIds.IndexOf(x.Id!.Value)).ToList();

        AssertBinaryDataWithHeaders(response, rectangles);
    }

    [Test]
    [Order(12)]
    public async Task I018_012MarkerOnMarkerLayer()
    {
        var amountOfEntries = 10;
        IReadOnlyList<AnnotationDto> markers = await InsertMarker(amountOfEntries);

        BinaryDataWithHeaderDto response =
            await _annotationHttpClient.AnnotationClient.GetAnnotationsDeckGl(_slideImage.Data.Id);

        BaseLayerHeaderDto header = response.Headers.First();

        // we order the inserted annotations to match the order they are present in the header to make comparing easier
        markers = markers.OrderBy(x => header.AnnotationIds.IndexOf(x.Id!.Value)).ToList();

        AssertBinaryDataWithHeaders(response, markers);
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _annotationHttpClient.AnnotationClient.DeleteAnnotations(_slideImage.Data.Id);
    }

    [Test]
    [Order(999)]
    public async Task I018_999DeleteSlideImages()
    {
        SlideImageClient slideImageClient = _adminImageManagementHttpClient.SlideImageClient;
        await slideImageClient.DeleteSlideImage(_slideImage.Data.Id);
    }

    private string GetFileToUploadAbsPath()
    {
        return Directory
            .EnumerateFiles(Path.Combine(Configuration.ClientAccessPathRoot, Configuration.UploadFolder))
            .First();
    }

    private async Task<ApiResponse<SlideImageDto>> SuggestSlideImage()
    {
        string absolutePathToFile = GetFileToUploadAbsPath();

        string fileNameWithSuffix = absolutePathToFile.Split(Path.DirectorySeparatorChar).Last();
        string[] fileNameParts = fileNameWithSuffix.Split('.');
        string suffix = fileNameParts.Last();
        string fileName = fileNameParts.First();
        var fileInfo = new FileInfo(absolutePathToFile);

        await _adminImageManagementHttpClient.RetrieveUserInfo();

        ApiResponse<SlideImageDto> slideImageResponse = await _adminImageManagementHttpClient.SlideImageClient
            .SlideImageLocationSuggestion(fileName, suffix, null, _adminImageManagementHttpClient.UserInfo.UserId,
                fileInfo.Length);

        Assert.NotNull(slideImageResponse.Data.FileSize);

        return slideImageResponse;
    }

    private class NullProgressReporter : IProgress<ApiResponse<UploadProgressDto<SlideImageDto>>>
    {
        public void Report(ApiResponse<UploadProgressDto<SlideImageDto>> uploadProgress) { }
    }
}