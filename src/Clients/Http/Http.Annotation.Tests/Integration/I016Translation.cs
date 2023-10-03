using NUnit.Framework;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Extensions;
using PreciPoint.Ims.Clients.Http.ImageManagement;
using PreciPoint.Ims.Clients.Http.WholeSlideImages;
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
public class I016Translation : ABaseTest
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

    [Test]
    [Order(0)]
    public async Task I016_000UploadSlideImages()
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
    public async Task I016_001Verify_Insert()
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


        var counterArray = new double[3][] { new double[2] { 4, 3 }, new double[2] { 4, 4 }, new double[2] { 4, 10 } };
        _counterGroupDto = CreateCounterGroup(_annotationId);
        _counterGroupDto.Counters = counterArray;

        ApiResponse<CounterGroupDto> counterGroupDtoResult =
            await _annotationHttpClient_1.AnnotationClient.InsertCounterGroup(_counterGroupDto);
        _counterGroupDto = counterGroupDtoResult.Data;
        response = await _annotationHttpClient_1.AnnotationClient.GetAnnotationById(_annotationId);
        _polygon = response.Data;
    }

    [Test]
    [Order(2)]
    public async Task I016_002Verify_Translation()
    {
        var translateDto = new TranslateDto
        {
            AnnotationIds = new[] { _polygon.Id.Value },
            DeltaX = 1.5,
            DeltaY = 2
        };

        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.SetTranslation(translateDto);
        AnnotationDto polygonTranslated = response.Data[0];

        for (var i = 0; i < _polygon.Coordinates.Length; i++)
        {
            Assert.AreEqual(_polygon.Coordinates[i][0] + translateDto.DeltaX, polygonTranslated.Coordinates[i][0]);
            Assert.AreEqual(_polygon.Coordinates[i][1] + translateDto.DeltaY, polygonTranslated.Coordinates[i][1]);
        }

        for (var i = 0; i < _polygon.CounterGroups.Count; i++)
        for (var j = 0; j < _polygon.CounterGroups[i].Counters.Length; j++)
        {
            Assert.AreEqual(_polygon.CounterGroups[i].Counters[j][0] + translateDto.DeltaX,
                polygonTranslated.CounterGroups[i].Counters[j][0]);
            Assert.AreEqual(_polygon.CounterGroups[i].Counters[j][1] + translateDto.DeltaY,
                polygonTranslated.CounterGroups[i].Counters[j][1]);
        }
    }

    [Test]
    [Order(3)]
    public async Task I016_003Verify_DeleteAnnotation()
    {
        ApiResponse<DeleteOperationDto> result = await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_polygon.Id.Value);
        Assert.Greater(result.Data.NumberOfEntityRemoved, 0);

        ApiListResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, response.Data.Count);
    }

    [Test]
    [Order(999)]
    public async Task I016_999DeleteSlideImages()
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