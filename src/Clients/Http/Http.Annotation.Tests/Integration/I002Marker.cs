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
[Order(2)]
[NonParallelizable]
[Category("Integration")]
public class I002Marker : ABaseTest
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
    private AnnotationDto _marker;

    [Test]
    [Order(0)]
    public async Task I002_000UploadSlideImages()
    {
        _slideImage = await SuggestSlideImage();
        var wholeSlideImagesAdmin = new WholeSlideImagesHttpClient(HttpClientFactory.CreateAdminHttpClient(Configuration),
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
    public async Task I002_001Verify_GetRequest()
    {
        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, result.Data.Count);

        var id = Guid.NewGuid();
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.GetAnnotationById(id));
        Assert.True(ex.Message.Contains(id.ToString()));
    }

    [Test]
    [Order(2)]
    public async Task I002_002Verify_Insert()
    {
        _marker = CreateAnnotation(AnnotationType.Marker, AnnotationVisibility.Private);
        var coordinatesDto =
            new double[2][] { new double[2] { 2.2, 4.4 }, new double[2] { 3.3, 5.5 } };
        _marker.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.InsertAnnotation(_marker, _slideImage.Data.Id);
        _marker = response.Data;
        _annotationId = _marker.Id.Value;

        Assert.NotNull(_marker);
        Assert.AreEqual(_annotationId, _marker.Id.Value);
        Assert.AreEqual(AnnotationType.Marker, _marker.AnnotationType);
        Assert.AreEqual(coordinatesDto.Length, _marker.Coordinates.Length);
        Assert.AreEqual(0, _marker.Radius);
        Assert.AreEqual(0, _marker.Length);
        Assert.AreEqual(0, _marker.Area);
    }

    [Test]
    [Order(3)]
    public async Task I002_003Verify_Update()
    {
        _marker.Label += " Update";
        _marker.Description += " Update";
        var coordinatesDto =
            new double[2][] { new double[2] { 5, 7 }, new double[2] { 8, 10 } };
        _marker.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(_marker, _slideImage.Data.Id);
        _marker = response.Data;

        Assert.NotNull(_marker);
        Assert.AreEqual(_annotationId, _marker.Id.Value);
        Assert.IsTrue(_marker.Label.EndsWith("Update"));
        Assert.IsTrue(_marker.Description.EndsWith("Update"));
        Assert.AreEqual(coordinatesDto.Length, _marker.Coordinates.Length);
        Assert.AreEqual(coordinatesDto[0][0], _marker.Coordinates[0][0]);
        Assert.AreEqual(coordinatesDto[0][1], _marker.Coordinates[0][1]);
        Assert.AreEqual(coordinatesDto[1][0], _marker.Coordinates[1][0]);
        Assert.AreEqual(coordinatesDto[1][1], _marker.Coordinates[1][1]);
    }

    [Test]
    [Order(4)]
    public void I002_004Verify_InsertVertices()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 8, 6 } }, Index = 0 };

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.InsertVertices(_marker.Id.Value, vertexUpdateDto));
        Assert.True(ex.Message.Contains(_marker.Id.ToString()));
    }

    [Test]
    [Order(5)]
    public async Task I002_005Verify_SetVertexPosition()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3, 4 } }, Index = 1 };

        ApiResponse<AnnotationDto> response =
            await _annotationHttpClient_1.AnnotationClient.SetVertexPosition(_marker.Id.Value, vertexUpdateDto);
        _marker = response.Data;

        Assert.NotNull(_marker);
        Assert.IsTrue(_marker.Label.EndsWith("Update"));
        Assert.IsTrue(_marker.Description.EndsWith("Update"));
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto.Length + 1, _marker.Coordinates.Length);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _marker.Coordinates[vertexUpdateDto.Index][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _marker.Coordinates[vertexUpdateDto.Index][1]);
    }

    [Test]
    [Order(6)]
    public void I002_006Verify_DeleteVertex()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.DeleteVertex(_marker.Id.Value, 1));
        Assert.True(ex.Message.Contains(_marker.Id.Value.ToString()));
    }

    [Test]
    [Order(7)]
    public async Task I002_007Verify_GetAnnotations()
    {
        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, response.Data.Count);
        Assert.AreEqual(AnnotationType.Marker, response.Data[0].AnnotationType);
    }

    [Test]
    [Order(8)]
    public void I002_008Verify_InsertCounters()
    {
        CounterGroupDto counterGroupDto = CreateCounterGroup(_annotationId);
        counterGroupDto.Counters =
            new double[2][] { new double[2] { 5, 6 }, new double[2] { 5, 6 } };

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.InsertCounters(counterGroupDto));
        Assert.True(ex.Message.Contains("cannot be null"));
    }

    [Test]
    [Order(9)]
    public void I002_009Verify_SetRadius()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.SetRadius(_marker.Id.Value, 20));
        Assert.True(ex.Message.Contains(_marker.AnnotationType.ToString()));
    }

    [Test]
    [Order(10)]
    public async Task I002_010Verify_DeleteAnnotation()
    {
        ApiResponse<DeleteOperationDto> result = await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_marker.Id.Value);
        Assert.Greater(result.Data.NumberOfEntityRemoved, 0);

        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, response.Data.Count);
    }

    [Test]
    [Order(999)]
    public async Task I002_999DeleteSlideImages()
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