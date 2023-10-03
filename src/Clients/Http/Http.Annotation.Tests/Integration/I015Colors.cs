using FluentAssertions;
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
[Order(16)]
[NonParallelizable]
[Category("Integration")]
public class I015Colors : ABaseTest
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
    public async Task I015_000UploadSlideImages()
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
    public async Task I015_001Verify_Insert()
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
    [Order(2)]
    public async Task I015_002Verify_Update()
    {
        var colors = new[] { 255, 255, 255, 255 };
        _marker.Color = colors;
        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.SetColor(_marker);
        _marker = response.Data;

        Assert.NotNull(_marker);
        Assert.AreEqual(_annotationId, _marker.Id.Value);
        Assert.AreEqual(_marker.Color.Length, colors.Length);


        colors = new[] { 255, 255, 255 };
        _marker.Color = colors;
        response = await _annotationHttpClient_1.AnnotationClient.SetColor(_marker);
        _marker = response.Data;

        Assert.NotNull(_marker);
        Assert.AreEqual(_annotationId, _marker.Id.Value);
        Assert.AreEqual(_marker.Color.Length, colors.Length);


        colors = Array.Empty<int>();
        _marker.Color = colors;
        response = await _annotationHttpClient_1.AnnotationClient.SetColor(_marker);
        _marker = response.Data;

        Assert.NotNull(_marker);
        Assert.AreEqual(_annotationId, _marker.Id.Value);
        Assert.AreEqual(_marker.Color.Length, colors.Length);
    }

    [Test]
    [Order(3)]
    public async Task I015_003Verify_InsertInvalidColor()
    {
        _marker.Color = new[] { 255 };
        Func<Task> fn = async () => { await _annotationHttpClient_1.AnnotationClient.SetColor(_marker); };
        await fn.Should().ThrowAsync<ApiException>();


        _marker.Color = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 1337, 420, 69, 69 };
        fn = async () => { await _annotationHttpClient_1.AnnotationClient.SetColor(_marker); };
        await fn.Should().ThrowAsync<ApiException>();
    }

    [Test]
    [Order(4)]
    public async Task I015_004Verify_DeleteAnnotation()
    {
        ApiResponse<DeleteOperationDto> result = await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_marker.Id.Value);
        Assert.Greater(result.Data.NumberOfEntityRemoved, 0);

        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, response.Data.Count);
    }

    [Test]
    [Order(999)]
    public async Task I015_999DeleteSlideImages()
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