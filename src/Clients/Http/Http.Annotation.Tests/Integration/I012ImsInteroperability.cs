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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Integration;

[TestFixture]
[Order(12)]
[NonParallelizable]
[Category("Integration")]
internal class I012ImsInteroperability : ABaseTest
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
    private AnnotationDto _annotationDto;

    [Test]
    [Order(0)]
    public async Task I012_001UploadSlideImages()
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
    public async Task I012_002DeleteSlideImagesIntoIms()
    {
        _annotationDto = await CreateAnnotation(_slideImage.Data.Id);

        await _adminImageManagementHttpClient.SlideImageClient.DeleteSlideImage(_slideImage.Data.Id);
    }

    [Test]
    [Order(3)]
    public async Task I012_003VerifyDeleteIntoAnnotation()
    {
        //need wait for event sync
        var counter = 0;
        do
        {
            try
            {
                await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
            }
            catch (Exception)
            {
                break;
            }

            counter++;
            Thread.Sleep(1000);
        } while (counter < 30);

        var ex = Assert.ThrowsAsync<ApiException>(
            () => _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);
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

    private async Task<AnnotationDto> CreateAnnotation(Guid slideImageId)
    {
        AnnotationDto polygon = CreateAnnotation(AnnotationType.Polygon, AnnotationVisibility.Private);
        var coordinatesDto =
            new double[7][]
            {
                new double[2] { 3.3, 3 }, new double[2] { 5, 2.5 }, new double[2] { 7.5, 2.5 }, new double[2] { 8, 4 }, new double[2] { 7, 6 },
                new double[2] { 6, 4 }, new double[2] { 3.3, 3 }
            };
        polygon.Coordinates = coordinatesDto;
        return (await _annotationHttpClient_1.AnnotationClient.InsertAnnotation(polygon, slideImageId)).Data;
    }

    private async Task<CounterGroupDto> CreateAnnotationCounter(Guid annotationId)
    {
        CounterGroupDto annotationCounterDto = CreateCounterGroup(annotationId);
        var counters =
            new double[3][] { new double[2] { 4, 3 }, new double[2] { 4, 4 }, new double[2] { 4, 10 } };
        annotationCounterDto.Counters = counters;
        return (await _annotationHttpClient_1.AnnotationClient.InsertCounterGroup(annotationCounterDto)).Data;
    }
}