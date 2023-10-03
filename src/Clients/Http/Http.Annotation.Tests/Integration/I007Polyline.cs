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
[Order(7)]
[NonParallelizable]
[Category("Integration")]
public class I007Polyline : ABaseTest
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
    private AnnotationDto _polyline;

    [Test]
    [Order(0)]
    public async Task I007_000UploadSlideImages()
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
    public async Task I007_001Verify_GetRequest()
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
    public async Task I007_002Verify_Insert()
    {
        _polyline = CreateAnnotation(AnnotationType.Polyline, AnnotationVisibility.Private);
        var coordinatesDto = new double[5][]
        {
            new double[2] { 3.3, 3 }, new double[2] { 5, 2.5 }, new double[2] { 8, 2 }, new double[2] { 8, 4 }, new double[2] { 7, 6 }
        };
        _polyline.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.InsertAnnotation(_polyline, _slideImage.Data.Id);
        _polyline = response.Data;
        _annotationId = _polyline.Id.Value;

        Assert.NotNull(_polyline);
        Assert.AreEqual(_annotationId, _polyline.Id.Value);
        Assert.AreEqual(AnnotationType.Polyline, _polyline.AnnotationType);
        Assert.AreEqual(coordinatesDto.Length, _polyline.Coordinates.Length);

        for (var i = 0; i < coordinatesDto.Length; i++)
        {
            Assert.AreEqual(coordinatesDto[i][0], _polyline.Coordinates[i][0]);
            Assert.AreEqual(coordinatesDto[i][1], _polyline.Coordinates[i][1]);
        }

        Assert.AreEqual(0, _polyline.Radius);
        Assert.Greater(_polyline.Length, 0);
        Assert.AreEqual(0, _polyline.Area);
    }

    [Test]
    [Order(3)]
    public async Task I007_003Verify_Update()
    {
        _polyline.Label += " Update";
        _polyline.Description += " Update";
        var coordinatesDto = new double[5][]
        {
            new double[2] { 3, 3 }, new double[2] { 5, 2 }, new double[2] { 8, 2 }, new double[2] { 8, 4 }, new double[2] { 7, 6 }
        };
        _polyline.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(_polyline, _slideImage.Data.Id);
        _polyline = response.Data;

        Assert.NotNull(_polyline);
        Assert.AreEqual(_annotationId, _polyline.Id.Value);
        Assert.IsTrue(_polyline.Label.EndsWith("Update"));
        Assert.IsTrue(_polyline.Description.EndsWith("Update"));
        Assert.AreEqual(coordinatesDto.Length, _polyline.Coordinates.Length);
        for (var i = 0; i < coordinatesDto.Length; i++)
        {
            Assert.AreEqual(coordinatesDto[i][0], _polyline.Coordinates[i][0]);
            Assert.AreEqual(coordinatesDto[i][1], _polyline.Coordinates[i][1]);
        }
    }

    [Test]
    [Order(4)]
    public async Task I007_004Verify_InsertVertices()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 6, 4 } }, Index = -1 };
        ApiResponse<AnnotationDto> response =
            await _annotationHttpClient_1.AnnotationClient.InsertVertices(_polyline.Id.Value, vertexUpdateDto);
        _polyline = response.Data;

        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _polyline.Coordinates[0][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _polyline.Coordinates[0][1]);

        vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3.5, 1.2 } }, Index = 1 };
        response = await _annotationHttpClient_1.AnnotationClient.InsertVertices(_polyline.Id.Value, vertexUpdateDto);
        _polyline = response.Data;

        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _polyline.Coordinates[2][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _polyline.Coordinates[2][1]);

        vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3.5, 1.2 } }, Index = _polyline.Coordinates.Length };
        response = await _annotationHttpClient_1.AnnotationClient.InsertVertices(_polyline.Id.Value, vertexUpdateDto);
        _polyline = response.Data;

        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _polyline.Coordinates[^1][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _polyline.Coordinates[^1][1]);
    }

    [Test]
    [Order(5)]
    public void I007_005Verify_InsertVertices_Fails()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 6, 4 } }, Index = -2 };

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.InsertVertices(_polyline.Id.Value, vertexUpdateDto));
        Assert.True(ex.Message.Contains(_polyline.Id.ToString()));
    }

    [Test]
    [Order(6)]
    public async Task I007_006Verify_SetVertexPosition()
    {
        int counterBeforeSetVertex = _polyline.Coordinates.Length;
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 2, 2 } }, Index = 2 };
        ApiResponse<AnnotationDto> response =
            await _annotationHttpClient_1.AnnotationClient.SetVertexPosition(_polyline.Id.Value, vertexUpdateDto);
        _polyline = response.Data;

        Assert.NotNull(_polyline);
        Assert.IsTrue(_polyline.Label.EndsWith("Update"));
        Assert.IsTrue(_polyline.Description.EndsWith("Update"));
        Assert.AreEqual(counterBeforeSetVertex, _polyline.Coordinates.Length);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _polyline.Coordinates[vertexUpdateDto.Index][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _polyline.Coordinates[vertexUpdateDto.Index][1]);
    }

    [Test]
    [Order(7)]
    public async Task I007_007Verify_DeleteVertex()
    {
        int pointsBeforeDelete = _polyline.Coordinates.Length;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.DeleteVertex(_polyline.Id.Value, 2);

        Assert.Greater(pointsBeforeDelete, response.Data.Coordinates.Length);
        Assert.AreEqual(pointsBeforeDelete - 1, response.Data.Coordinates.Length);
    }

    [Test]
    [Order(8)]
    public async Task I007_008Verify_GetAnnotations()
    {
        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, response.Data.Count);
        Assert.AreEqual(AnnotationType.Polyline, response.Data[0].AnnotationType);
    }

    [Test]
    [Order(9)]
    public void I007_009Verify_InsertCounters()
    {
        CounterGroupDto counterGroupDto = CreateCounterGroup(_annotationId);
        counterGroupDto.Counters =
            new double[2][] { new double[2] { 5, 6 }, new double[2] { 6, 6 } };

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.InsertCounters(counterGroupDto));
        Assert.True(ex.Message.Contains("cannot be null"));
    }

    [Test]
    [Order(10)]
    public async Task I007_010Verify_GetAnnotation()
    {
        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, response.Data.Count);
        Assert.Greater(1, response.Data[0].CounterGroups.Count);

        ApiResponse<AnnotationDto> record =
            await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(response.Data[0].Id.Value);
        Assert.NotNull(record);
        Assert.Greater(1, record.Data.CounterGroups.Count);
    }

    [Test]
    [Order(11)]
    public void I007_011Verify_SetRadius()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.SetRadius(_polyline.Id.Value, 20));
        Assert.True(ex.Message.Contains(_polyline.AnnotationType.ToString()));
    }

    [Test]
    [Order(12)]
    public async Task I007_012Verify_DeleteAnnotation()
    {
        ApiResponse<DeleteOperationDto> result = await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_polyline.Id.Value);
        Assert.Greater(result.Data.NumberOfEntityRemoved, 0);

        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, response.Data.Count);
    }

    [Test]
    [Order(999)]
    public async Task I007_999DeleteSlideImages()
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