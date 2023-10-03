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
[Order(4)]
[NonParallelizable]
[Category("Integration")]
public class I004Line : ABaseTest
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
    private AnnotationDto _line;

    [Test]
    [Order(0)]
    public async Task I004_000UploadSlideImages()
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
    public async Task I004_001Verify_GetRequest()
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
    public async Task I004_002Verify_Insert()
    {
        _line = CreateAnnotation(AnnotationType.Line, AnnotationVisibility.Private);
        var coordinatesDto =
            new double[2][] { new double[2] { 2.2, 4.4 }, new double[2] { 3.3, 5.5 } };
        _line.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.InsertAnnotation(_line, _slideImage.Data.Id);
        _line = response.Data;
        _annotationId = _line.Id.Value;

        Assert.NotNull(_line);
        Assert.AreEqual(_annotationId, _line.Id.Value);
        Assert.AreEqual(AnnotationType.Line, _line.AnnotationType);
        Assert.AreEqual(coordinatesDto.Length, _line.Coordinates.Length);
        Assert.AreEqual(0, _line.Radius);
        Assert.Greater(_line.Length, 0);
        Assert.AreEqual(0, _line.Area);
    }

    [Test]
    [Order(3)]
    public async Task I004_003Verify_Update()
    {
        _line.Label += " Update";
        _line.Description += " Update";
        var coordinatesDto =
            new double[2][] { new double[2] { 5, 7 }, new double[2] { 8, 10 } };
        _line.Coordinates = coordinatesDto;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(_line, _slideImage.Data.Id);
        _line = response.Data;

        Assert.NotNull(_line);
        Assert.AreEqual(_annotationId, _line.Id.Value);
        Assert.IsTrue(_line.Label.EndsWith("Update"));
        Assert.IsTrue(_line.Description.EndsWith("Update"));
        Assert.AreEqual(coordinatesDto.Length, _line.Coordinates.Length);
        Assert.AreEqual(coordinatesDto[0][0], _line.Coordinates[0][0]);
        Assert.AreEqual(coordinatesDto[0][1], _line.Coordinates[0][1]);
        Assert.AreEqual(coordinatesDto[1][0], _line.Coordinates[1][0]);
        Assert.AreEqual(coordinatesDto[1][1], _line.Coordinates[1][1]);
    }

    [Test]
    [Order(4)]
    public void I004_004Verify_InsertVertices()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 8, 6 } }, Index = 0 };

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.InsertVertices(_line.Id.Value, vertexUpdateDto));
        Assert.True(ex.Message.Contains(_line.Id.ToString()));
    }

    [Test]
    [Order(5)]
    public async Task I004_005Verify_SetVertexPosition()
    {
        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3, 4 } }, Index = 1 };

        ApiResponse<AnnotationDto> response =
            await _annotationHttpClient_1.AnnotationClient.SetVertexPosition(_line.Id.Value, vertexUpdateDto);
        _line = response.Data;

        Assert.NotNull(_line);
        Assert.IsTrue(_line.Label.EndsWith("Update"));
        Assert.IsTrue(_line.Description.EndsWith("Update"));
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto.Length + 1, _line.Coordinates.Length);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], _line.Coordinates[vertexUpdateDto.Index][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], _line.Coordinates[vertexUpdateDto.Index][1]);
    }

    [Test]
    [Order(6)]
    public void I004_006Verify_DeleteVertex()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.DeleteVertex(_line.Id.Value, 1));
        Assert.True(ex.Message.Contains(_line.Id.Value.ToString()));
    }

    [Test]
    [Order(7)]
    public async Task I004_007Verify_GetAnnotations()
    {
        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, response.Data.Count);
        Assert.AreEqual(AnnotationType.Line, response.Data[0].AnnotationType);
    }

    [Test]
    [Order(8)]
    public void I004_008Verify_InsertCounters()
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
    public void I004_009Verify_SetRadius()
    {
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.SetRadius(_line.Id.Value, 20));
        Assert.True(ex.Message.Contains(_line.AnnotationType.ToString()));
    }

    [Test]
    [Order(10)]
    public async Task I004_010Verify_DeleteAnnotation()
    {
        ApiResponse<DeleteOperationDto> result = await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_line.Id.Value);
        Assert.Greater(result.Data.NumberOfEntityRemoved, 0);

        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, response.Data.Count);
    }

    [Test]
    [Order(999)]
    public async Task I004_999DeleteSlideImages()
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