using NUnit.Framework;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Extensions;
using PreciPoint.Ims.Clients.Http.ImageManagement;
using PreciPoint.Ims.Clients.Http.WholeSlideImages;
using PreciPoint.Ims.Core.DataTransferObjects.Exceptions;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using PreciPoint.Ims.Shared.DataTransferObjects.Upload;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Integration;

[TestFixture]
[Order(8)]
[NonParallelizable]
[Category("Integration")]
public class I008Polygon : ABaseTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _apiUrl = $"{Configuration.AnnotationHost}/api";
        var apiUrl = $"{Configuration.ImageManagementHost}/api";

        _annotationHttpClient_1 = new AnnotationHttpClient(HttpClientFactory.CreateUserHttpClient(Configuration), _apiUrl);
        _annotationHttpClient_2 = new AnnotationHttpClient(HttpClientFactory.CreateUser2HttpClient(Configuration), _apiUrl);
        _adminAnnotationHttpClient = new AnnotationHttpClient(HttpClientFactory.CreateAdminHttpClient(Configuration), _apiUrl);
        _adminImageManagementHttpClient = new ImageManagementHttpClient(HttpClientFactory.CreateAdminHttpClient(Configuration), apiUrl);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _annotationHttpClient_1.Dispose();
        _annotationHttpClient_2.Dispose();
        _adminAnnotationHttpClient.Dispose();
        _adminImageManagementHttpClient.Dispose();
    }

    private ImageManagementHttpClient _adminImageManagementHttpClient;
    private string _apiUrl;
    private AnnotationHttpClient _adminAnnotationHttpClient;
    private AnnotationHttpClient _annotationHttpClient_1;
    private AnnotationHttpClient _annotationHttpClient_2;
    private ApiResponse<SlideImageDto> _slideImage;
    private Guid _annotationId = Guid.Empty;
    private AnnotationDto _polygon;
    private CounterGroupDto _counterGroupDto;
    private int _actualCounter;

    [Test]
    [Order(0)]
    public async Task I008_000UploadSlideImages()
    {
        _slideImage = await SuggestSlideImage();
        var wholeSlideImagesAdmin = new WholeSlideImagesHttpClient(
            HttpClientFactory.CreateAdminHttpClient(Configuration),
            _slideImage.Data.Storage.ClientAccessPathRoot + "/api");

        ApiResponse<UploadProgressDto<SlideImageDto>> uploadProgress = await wholeSlideImagesAdmin.UploadClient
            .UploadWholeSlideImage(GetFileToUploadAbsPath(), _slideImage.Data.Id, new NullProgressReporter());

        Assert.NotNull(uploadProgress.Data.Entity.CreatedAt);

        ApiResponse<AnnotationPermissionsDto> result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        result.Data.Permission = AnnotationPermission.Draw;
        await _adminAnnotationHttpClient.AnnotationClient.SetAnnotationPermissions(result.Data);
    }

    [Test]
    [Order(1)]
    public async Task I008_001Verify_GetRequest()
    {
        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, result.Data.Count());

        var id = Guid.NewGuid();
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.GetAnnotationById(id));
        Assert.True(ex.Message.Contains(id.ToString()));
    }

    [Test]
    [Order(2)]
    public async Task I008_002Verify_Insert()
    {
        _polygon = CreateAnnotation(AnnotationType.Polygon, AnnotationVisibility.Private);
        var coordinatesDto = new double[7][]
        {
            new double[2] { 3.3, 3 }, new double[2] { 5, 2.5 }, new double[2] { 7.5, 2.5 }, new double[2] { 8, 4 }, new double[2] { 7, 6 },
            new double[2] { 6, 4 }, new double[2] { 3.3, 3 }
        };
        _polygon.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.InsertAnnotation(_polygon, _slideImage.Data.Id);
        _polygon = response.Data;
        _annotationId = _polygon.Id.Value;

        Assert.NotNull(_polygon);
        Assert.AreEqual(_annotationId, _polygon.Id.Value);
        Assert.AreEqual(AnnotationType.Polygon, _polygon.AnnotationType);
        Assert.AreEqual(coordinatesDto.Length, _polygon.Coordinates.Length);

        for (var i = 0; i < coordinatesDto.Length; i++)
        {
            Assert.AreEqual(coordinatesDto[i][0], _polygon.Coordinates[i][0]);
            Assert.AreEqual(coordinatesDto[i][1], _polygon.Coordinates[i][1]);
        }

        Assert.AreEqual(0, _polygon.Radius);
        Assert.Greater(_polygon.Length, 0);
        Assert.Greater(_polygon.Area, 0);
    }

    [Test]
    [Order(3)]
    public async Task I008_003Verify_Update()
    {
        _polygon.Label += " Update";
        _polygon.Description += " Update";
        var coordinatesDto = new double[7][]
        {
            new double[2] { 3, 3 }, new double[2] { 5, 2 }, new double[2] { 7.5, 2.5 }, new double[2] { 8, 4 }, new double[2] { 7, 6 },
            new double[2] { 6, 4 }, new double[2] { 3, 3 }
        };
        _polygon.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(_polygon, _slideImage.Data.Id);
        _polygon = response.Data;

        Assert.NotNull(_polygon);
        Assert.AreEqual(_annotationId, _polygon.Id.Value);
        Assert.IsTrue(_polygon.Label.EndsWith("Update"));
        Assert.IsTrue(_polygon.Description.EndsWith("Update"));
        Assert.AreEqual(coordinatesDto.Length, _polygon.Coordinates.Length);
        for (var i = 0; i < coordinatesDto.Length; i++)
        {
            Assert.AreEqual(coordinatesDto[i][0], _polygon.Coordinates[i][0]);
            Assert.AreEqual(coordinatesDto[i][1], _polygon.Coordinates[i][1]);
        }
    }

    [Test]
    [Order(4)]
    public async Task I008_004Verify_InsertVertices()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3.5, 1.2 } }, Index = 1 };
        ApiResponse<AnnotationDto> response =
            await _annotationHttpClient_1.AnnotationClient.InsertVertices(_polygon.Id.Value, vertexUpdateDto);
        _polygon = response.Data;

        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _polygon.Coordinates[2][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _polygon.Coordinates[2][1]);

        vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 4, 8 } }, Index = _polygon.Coordinates.Length - 2 };
        response = await _annotationHttpClient_1.AnnotationClient.InsertVertices(_polygon.Id.Value, vertexUpdateDto);
        _polygon = response.Data;

        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _polygon.Coordinates[^2][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _polygon.Coordinates[^2][1]);
    }

    [Test]
    [Order(5)]
    public void I008_005Verify_InsertVertices_Fails()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 6, 4 } }, Index = -1 };

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.InsertVertices(_polygon.Id.Value, vertexUpdateDto));
        Assert.True(ex.Message.Contains(_polygon.Id.ToString()));

        vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3.5, 1.2 } }, Index = _polygon.Coordinates.Length };

        ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.InsertVertices(_polygon.Id.Value, vertexUpdateDto));
        Assert.True(ex.Message.Contains(_polygon.Id.ToString()));
    }

    [Test]
    [Order(6)]
    public async Task I008_006Verify_SetVertexPosition()
    {
        int counterBeforeSetVertex = _polygon.Coordinates.Length;
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 1, 1 } }, Index = 0 };
        ApiResponse<AnnotationDto> response =
            await _annotationHttpClient_1.AnnotationClient.SetVertexPosition(_polygon.Id.Value, vertexUpdateDto);
        _polygon = response.Data;

        Assert.NotNull(_polygon);
        Assert.IsTrue(_polygon.Label.EndsWith("Update"));
        Assert.IsTrue(_polygon.Description.EndsWith("Update"));
        Assert.AreEqual(counterBeforeSetVertex, _polygon.Coordinates.Length);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _polygon.Coordinates[vertexUpdateDto.Index][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _polygon.Coordinates[vertexUpdateDto.Index][1]);
    }

    [Test]
    [Order(7)]
    public async Task I008_007Verify_DeleteVertex()
    {
        int pointsBeforeDelete = _polygon.Coordinates.Length;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.DeleteVertex(_polygon.Id.Value, 2);

        Assert.Greater(pointsBeforeDelete, response.Data.Coordinates.Length);
        Assert.AreEqual(pointsBeforeDelete - 1, response.Data.Coordinates.Length);
    }

    [Test]
    [Order(8)]
    public async Task I008_008Verify_GetAnnotations()
    {
        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, response.Data.Count);
        Assert.AreEqual(AnnotationType.Polygon, response.Data[0].AnnotationType);
    }

    [Test]
    [Order(9)]
    public async Task I008_009Verify_InsertCounterGroup()
    {
        var counterArray = new double[3][] { new double[2] { 4, 3 }, new double[2] { 4, 4 }, new double[2] { 4, 10 } };
        _counterGroupDto = CreateCounterGroup(_annotationId);
        _counterGroupDto.Counters = counterArray;

        ApiResponse<CounterGroupDto> counterGroupDtoResult =
            await _annotationHttpClient_1.AnnotationClient.InsertCounterGroup(_counterGroupDto);
        _counterGroupDto = counterGroupDtoResult.Data;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _polygon = response.Data;

        Assert.NotNull(_polygon);
        Assert.AreEqual(counterGroupDtoResult.Data.Counters.Length, counterArray.Length - 1);
        Assert.AreEqual(_polygon.CounterGroups[0].Counters.Length, counterArray.Length - 1);
        Assert.AreEqual(counterGroupDtoResult.Data.Counters.Length, _counterGroupDto.Counters.Length);
        Assert.AreEqual(_polygon.CounterGroups[0].Counters.Length, _counterGroupDto.Counters.Length);
    }

    [Test]
    [Order(10)]
    public async Task I008_010Verify_UpdateCounterGroup()
    {
        var counterArray = new double[3][] { new double[2] { 4, 3 }, new double[2] { 4, 11 }, new double[2] { 4, 10 } };
        _counterGroupDto.Label += " Update";
        _counterGroupDto.Counters = counterArray;

        ApiResponse<CounterGroupDto> counterGroupDtoResult =
            await _annotationHttpClient_1.AnnotationClient.UpdateCounterGroup(_counterGroupDto);
        _counterGroupDto = counterGroupDtoResult.Data;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _polygon = response.Data;

        Assert.IsTrue(counterGroupDtoResult.Data.Label.EndsWith("Update"));
        Assert.AreEqual(counterGroupDtoResult.Data.Counters.Length, counterArray.Length - 2);
        Assert.AreEqual(_polygon.CounterGroups[0].Counters.Length, counterArray.Length - 2);
        Assert.AreEqual(counterGroupDtoResult.Data.Counters.Length, _counterGroupDto.Counters.Length);
        Assert.AreEqual(_polygon.CounterGroups[0].Counters.Length, _counterGroupDto.Counters.Length);
    }

    [Test]
    [Order(11)]
    public async Task I008_011Verify_InsertCounters()
    {
        var counterArray = new double[2][] { new double[2] { 4.44, 2.55 }, new double[2] { 12, 12 } };
        int countersCount = _counterGroupDto.Counters.Length;
        CounterGroupDto counterGroupRequest = CreateCounterGroup(_annotationId);
        counterGroupRequest.Id = _counterGroupDto.Id;
        counterGroupRequest.Counters = counterArray;


        ApiResponse<GenericCudOperationDto> counterGroupDtoResult =
            await _annotationHttpClient_1.AnnotationClient.InsertCounters(counterGroupRequest);
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _polygon = response.Data;

        _actualCounter = countersCount + counterArray.Length - 1;
        Assert.Greater(_actualCounter, 0);
        Assert.AreEqual(_actualCounter, _polygon.CounterGroups[0].Counters.Length);
    }

    [Test]
    [Order(12)]
    public async Task I008_012Verify_SetCounter()
    {
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _polygon = response.Data;

        Guid counterId = _polygon.CounterGroups[0].CounterIds[0];
        ApiResponse<GenericCudOperationDto> counter = await _annotationHttpClient_1.AnnotationClient.SetCounter(counterId, 3.5, 2.9);

        ApiResponse<CounterGroupDto> response2 = await _annotationHttpClient_1.AnnotationClient.GetCounterGroupById(_counterGroupDto.Id.Value);
        _counterGroupDto = response2.Data;

        int index = _counterGroupDto.CounterIds.ToList().FindIndex(e => e == counterId);
        Assert.Greater(_actualCounter, 0);
        Assert.AreEqual(_actualCounter, _counterGroupDto.Counters.Length);
        Assert.AreEqual(3.5, _counterGroupDto.Counters[index][0]);
        Assert.AreEqual(2.9, _counterGroupDto.Counters[index][1]);

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.SetCounter(counterId, 410, 2.2));
        Assert.True(ex.Message.Contains(counterId.ToString()));
    }

    [Test]
    [Order(13)]
    public async Task I008_013Verify_GetAnnotation()
    {
        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, response.Data.Count);
        Assert.GreaterOrEqual(1, response.Data[0].CounterGroups.Count);

        ApiResponse<AnnotationDto> record =
            await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(response.Data[0].Id.Value);
        Assert.NotNull(record);
        Assert.GreaterOrEqual(1, record.Data.CounterGroups.Count);
    }

    [Test]
    [Order(14)]
    public void I008_014Verify_SetRadius()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.SetRadius(_polygon.Id.Value, 20));
        Assert.True(ex.Message.Contains(_polygon.AnnotationType.ToString()));
    }

    [Test]
    [Order(15)]
    public async Task I008_015Verify_DeleteAnnotation()
    {
        ApiResponse<DeleteOperationDto> result = await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_polygon.Id.Value);
        Assert.Greater(result.Data.NumberOfEntityRemoved, 0);

        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, response.Data.Count);
    }

    [Test]
    [Order(999)]
    public async Task I008_999DeleteSlideImages()
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