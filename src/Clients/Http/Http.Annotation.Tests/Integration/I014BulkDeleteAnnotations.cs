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
[Order(14)]
[NonParallelizable]
[Category("Integration")]
public class I014BulkDeleteAnnotations : ABaseTest
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
    private AnnotationDto _polygonPrivate;
    private AnnotationDto _polygonPublic;

    [Test]
    [Order(0)]
    public async Task I014_000UploadSlideImages()
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
    public async Task I014_001CreateAnnotations()
    {
        _polygonPrivate = CreatePolygon(AnnotationType.Polygon, AnnotationVisibility.Private);
        _polygonPrivate =
            (await _annotationHttpClient_1.AnnotationClient.InsertAnnotation(_polygonPrivate, _slideImage.Data.Id))
            .Data;

        _polygonPublic = CreatePolygon(AnnotationType.Polygon, AnnotationVisibility.Public);
        _polygonPublic =
            (await _annotationHttpClient_1.AnnotationClient.InsertAnnotation(_polygonPublic, _slideImage.Data.Id)).Data;

        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(2, result.Data.Count);
    }

    [Test]
    [Order(2)]
    public async Task I014_002DifferentUserDeleteAnnotations()
    {
        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, result.Data.Count);

        Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.DeleteAnnotation(_slideImage.Data.Id));

        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, result.Data.Count);
    }

    [Test]
    [Order(3)]
    public async Task I014_003DeleteAllAnnotations()
    {
        ApiListResponse<AnnotationDto> resultUser2 = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, resultUser2.Data.Count);
        ApiListResponse<AnnotationDto> resultUser1 = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(2, resultUser1.Data.Count);

        _polygonPublic = CreatePolygon(AnnotationType.Polygon, AnnotationVisibility.Public);
        await _annotationHttpClient_2.AnnotationClient.InsertAnnotation(_polygonPublic, _slideImage.Data.Id);

        resultUser2 = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(2, resultUser2.Data.Count);
        resultUser1 = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(3, resultUser1.Data.Count);

        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotations(_slideImage.Data.Id);

        resultUser2 = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, resultUser2.Data.Count);
        resultUser1 = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, resultUser1.Data.Count);
    }

    private AnnotationDto CreatePolygon(AnnotationType annotationType, AnnotationVisibility annotationVisibility)
    {
        AnnotationDto annotation = CreateAnnotation(annotationType, annotationVisibility);
        var coordinatesDto =
            new double[7][]
            {
                new double[2] { 3.3, 3 }, new double[2] { 5, 2.5 }, new double[2] { 7.5, 2.5 }, new double[2] { 8, 4 }, new double[2] { 7, 6 },
                new double[2] { 6, 4 }, new double[2] { 3.3, 3 }
            };
        annotation.Coordinates = coordinatesDto;

        return annotation;
    }

    [Test]
    [Order(999)]
    public async Task I014_999DeleteSlideImages()
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