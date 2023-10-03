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
[Order(5)]
[NonParallelizable]
[Category("Integration")]
public class I005Circle : ABaseTest
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
    private AnnotationDto _circle;
    private CounterGroupDto _counterGroupDto;
    private int _actualCounter;

    [Test]
    [Order(0)]
    public async Task I005_000UploadSlideImages()
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
    public async Task I005_001Verify_GetRequest()
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
    public async Task I005_002Verify_Insert()
    {
        _circle = CreateAnnotation(AnnotationType.Circle, AnnotationVisibility.Private);
        var coordinatesDto =
            new double[2][] { new double[2] { 5.1, 2 }, new double[2] { 6, 2 } };
        _circle.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.InsertAnnotation(_circle, _slideImage.Data.Id);
        _circle = response.Data;
        _annotationId = _circle.Id.Value;

        Assert.NotNull(_circle);
        Assert.AreEqual(_annotationId, _circle.Id.Value);
        Assert.AreEqual(AnnotationType.Circle, _circle.AnnotationType);
        Assert.AreEqual(coordinatesDto.Length, _circle.Coordinates.Length);
        Assert.Greater(_circle.Radius, 0);
        Assert.AreEqual(0, _circle.Length);
        Assert.AreEqual(0, _circle.Area);
    }

    [Test]
    [Order(3)]
    public async Task I005_003Verify_Update()
    {
        _circle.Label += " Update";
        _circle.Description += " Update";
        var coordinatesDto =
            new double[2][] { new double[2] { 5.1, 2 }, new double[2] { 7, 2 } };
        _circle.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(_circle, _slideImage.Data.Id);
        _circle = response.Data;

        Assert.NotNull(_circle);
        Assert.AreEqual(_annotationId, _circle.Id.Value);
        Assert.IsTrue(_circle.Label.EndsWith("Update"));
        Assert.IsTrue(_circle.Description.EndsWith("Update"));
        Assert.AreEqual(coordinatesDto.Length, _circle.Coordinates.Length);
        Assert.AreEqual(coordinatesDto[0][0], _circle.Coordinates[0][0]);
        Assert.AreEqual(coordinatesDto[0][1], _circle.Coordinates[0][1]);
    }

    [Test]
    [Order(4)]
    public void I005_004Verify_InsertVertices()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 8, 6 } }, Index = 0 };

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.InsertVertices(_circle.Id.Value, vertexUpdateDto));
        Assert.True(ex.Message.Contains(_circle.Id.ToString()));
    }

    [Test]
    [Order(5)]
    public async Task I005_005Verify_SetRadius()
    {
        double[][] coordinatesBeforeUpdate = _circle.Coordinates;
        const int radius = 20;

        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.SetRadius(_circle.Id.Value, radius);
        _circle = response.Data;

        Assert.NotNull(_circle);
        Assert.AreEqual(2, _circle.Coordinates.Length);
        Assert.AreEqual(coordinatesBeforeUpdate[0][0], _circle.Coordinates[0][0]);
        Assert.AreEqual(coordinatesBeforeUpdate[0][1], _circle.Coordinates[0][1]);
        Assert.AreEqual(coordinatesBeforeUpdate[0][0] + radius, _circle.Coordinates[1][0]);
        Assert.AreEqual(coordinatesBeforeUpdate[0][1], _circle.Coordinates[1][1]);
        Assert.AreEqual(radius, _circle.Radius);
    }

    [Test]
    [Order(6)]
    public async Task I005_006Verify_SetVertexPosition()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 5, 2 } }, Index = 0 };

        ApiResponse<AnnotationDto> response =
            await _annotationHttpClient_1.AnnotationClient.SetVertexPosition(_circle.Id.Value, vertexUpdateDto);
        _circle = response.Data;

        Assert.NotNull(_circle);
        Assert.IsTrue(_circle.Label.EndsWith("Update"));
        Assert.IsTrue(_circle.Description.EndsWith("Update"));
        Assert.AreEqual(2, _circle.Coordinates.Length);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _circle.Coordinates[vertexUpdateDto.Index][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _circle.Coordinates[vertexUpdateDto.Index][1]);
    }

    [Test]
    [Order(7)]
    public void I005_007Verify_DeleteVertex()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.DeleteVertex(_circle.Id.Value, 0));
        Assert.True(ex.Message.Contains(_circle.Id.Value.ToString()));
    }

    [Test]
    [Order(8)]
    public async Task I005_008Verify_GetAnnotations()
    {
        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, response.Data.Count);
        Assert.AreEqual(AnnotationType.Circle, response.Data[0].AnnotationType);
    }

    [Test]
    [Order(9)]
    public async Task I005_009Verify_InsertCounterGroup()
    {
        var counterArray = new double[4][] { new double[2] { 5.5, 2.1 }, new double[2] { 4.5, 1.8 }, new double[2] { 4.1, 2 }, new double[2] { 20, 20 } };
        _counterGroupDto = CreateCounterGroup(_annotationId);
        _counterGroupDto.Counters = counterArray;

        ApiResponse<CounterGroupDto> counterGroupDtoResult =
            await _annotationHttpClient_1.AnnotationClient.InsertCounterGroup(_counterGroupDto);
        _counterGroupDto = counterGroupDtoResult.Data;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _circle = response.Data;

        Assert.NotNull(_circle);
        Assert.AreEqual(counterGroupDtoResult.Data.Counters.Length, counterArray.Length - 1);
        Assert.AreEqual(_circle.CounterGroups[0].Counters.Length, counterArray.Length - 1);
        Assert.AreEqual(counterGroupDtoResult.Data.Counters.Length, _counterGroupDto.Counters.Length);
        Assert.AreEqual(_circle.CounterGroups[0].Counters.Length, _counterGroupDto.Counters.Length);
    }

    [Test]
    [Order(10)]
    public async Task I005_010Verify_UpdateCounterGroup()
    {
        var counterArray = new double[4][] { new double[2] { 5.5, 2.1 }, new double[2] { 4.5, 1.8 }, new double[2] { 30, 20.22 }, new double[2] { 20, 20 } };
        _counterGroupDto.Label += " Update";
        _counterGroupDto.Counters = counterArray;

        ApiResponse<CounterGroupDto> counterGroupDtoResult =
            await _annotationHttpClient_1.AnnotationClient.UpdateCounterGroup(_counterGroupDto);
        _counterGroupDto = counterGroupDtoResult.Data;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _circle = response.Data;

        Assert.IsTrue(counterGroupDtoResult.Data.Label.EndsWith("Update"));
        Assert.AreEqual(counterGroupDtoResult.Data.Counters.Length, counterArray.Length - 2);
        Assert.AreEqual(_circle.CounterGroups[0].Counters.Length, counterArray.Length - 2);
        Assert.AreEqual(counterGroupDtoResult.Data.Counters.Length, _counterGroupDto.Counters.Length);
        Assert.AreEqual(_circle.CounterGroups[0].Counters.Length, _counterGroupDto.Counters.Length);
    }

    [Test]
    [Order(11)]
    public async Task I005_011Verify_InsertCounters()
    {
        var counterArray = new double[2][] { new double[2] { 5.2, 1.6 }, new double[2] { 12, 12 } };
        int countersCount = _counterGroupDto.Counters.Length;
        CounterGroupDto counterGroupRequest = CreateCounterGroup(_annotationId);
        counterGroupRequest.Id = _counterGroupDto.Id;
        counterGroupRequest.Counters = counterArray;


        ApiResponse<GenericCudOperationDto> counterGroupDtoResult =
            await _annotationHttpClient_1.AnnotationClient.InsertCounters(counterGroupRequest);
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _circle = response.Data;

        _actualCounter = countersCount + counterArray.Length - 1;
        Assert.Greater(_actualCounter, 0);
        Assert.AreEqual(_actualCounter, _circle.CounterGroups[0].Counters.Length);
    }

    [Test]
    [Order(12)]
    public async Task I005_012Verify_SetCounter()
    {
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _circle = response.Data;

        Guid counterId = _circle.CounterGroups[0].CounterIds[0];
        ApiResponse<GenericCudOperationDto> counter = await _annotationHttpClient_1.AnnotationClient.SetCounter(counterId, 4.9, 2.2);

        ApiResponse<CounterGroupDto> response2 = await _annotationHttpClient_1.AnnotationClient.GetCounterGroupById(_counterGroupDto.Id.Value);
        _counterGroupDto = response2.Data;

        int index = _counterGroupDto.CounterIds.ToList().FindIndex(e => e == counterId);
        Assert.Greater(_actualCounter, 0);
        Assert.AreEqual(_actualCounter, _counterGroupDto.Counters.Length);
        Assert.AreEqual(4.9, _counterGroupDto.Counters[index][0]);
        Assert.AreEqual(2.2, _counterGroupDto.Counters[index][1]);

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.SetCounter(counterId, 410, 2.2));
        Assert.True(ex.Message.Contains(counterId.ToString()));
    }

    [Test]
    [Order(13)]
    public async Task I005_013Verify_DeleteCounterGroup()
    {
        ApiResponse<DeleteOperationDto> result = await _annotationHttpClient_1.AnnotationClient.DeleteCounterGroup(_counterGroupDto.Id.Value);
        Assert.Greater(result.Data.NumberOfEntityRemoved, 0);

        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _circle = response.Data;

        Assert.AreEqual(0, _circle.CounterGroups.Count);
    }

    [Test]
    [Order(14)]
    public async Task I005_014Verify_DeleteAnnotation()
    {
        ApiResponse<DeleteOperationDto> result = await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_circle.Id.Value);
        Assert.Greater(result.Data.NumberOfEntityRemoved, 0);

        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, response.Data.Count);
    }

    [Test]
    [Order(999)]
    public async Task I005_999DeleteSlideImages()
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