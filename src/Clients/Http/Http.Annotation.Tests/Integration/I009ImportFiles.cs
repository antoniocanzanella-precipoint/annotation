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
[Order(9)]
[NonParallelizable]
[Category("Integration")]
public class I009ImportFiles : ABaseTest
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

    [Test]
    [Order(0)]
    public async Task I009_000UploadSlideImages()
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
    public async Task I009_001VerifyVpaFiles()
    {
        const string fileName = "MergedWithRectangular.vpa";
        byte[] fileToImport = File.ReadAllBytes($"{Folder}/{fileName}");
        ApiListResponse<AnnotationDto> annotationsImported =
            await _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, fileToImport, fileName,
                false);

        Assert.AreEqual(14, annotationsImported.Data.Count);
    }

    [Test]
    [Order(2)]
    public async Task I009_002VerifyVpaFilesNotFull()
    {
        const string fileName = "WithOutRectangular.vpa";
        byte[] fileToImport = await File.ReadAllBytesAsync($"{Folder}/{fileName}");
        ApiListResponse<AnnotationDto> annotationsImported =
            await _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, fileToImport, fileName,
                false);

        Assert.AreEqual(12, annotationsImported.Data.Count);
    }

    [Test]
    [Order(3)]
    public async Task I009_003VerifyVpaEmptyFile()
    {
        const string fileName = "WithOutRectangular.vpa";
        byte[] fileToImport = await File.ReadAllBytesAsync($"{Folder}/{fileName}");
        ApiListResponse<AnnotationDto> annotationsImported =
            await _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, fileToImport, fileName,
                false);

        Assert.AreEqual(12, annotationsImported.Data.Count);
    }

    [Test]
    [Order(4)]
    public async Task I009_004VerifyVpaOnlyRectangular()
    {
        const string fileName = "MgSr_30d_20_10_CD11c.vpa";
        byte[] fileToImport = await File.ReadAllBytesAsync($"{Folder}/{fileName}");
        ApiListResponse<AnnotationDto> annotationsImported =
            await _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, fileToImport, fileName,
                false);

        Assert.AreEqual(764, annotationsImported.Data.Count);
    }

    [Test]
    [Order(5)]
    public async Task I009_005VerifyVpaForFrontEndTest()
    {
        const string fileName = "convallaria.vpa";
        byte[] fileToImport = await File.ReadAllBytesAsync($"{Folder}/{fileName}");
        ApiListResponse<AnnotationDto> annotationsImported =
            await _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, fileToImport, fileName,
                false);

        Assert.AreEqual(11, annotationsImported.Data.Count);
    }

    [Test]
    [Order(6)]
    public async Task I009_006VerifyTransactionRollBack()
    {
        const string fileName = "MustThrowException.vpa";
        byte[] brokenFileToImport = await File.ReadAllBytesAsync($"{Folder}/{fileName}");
        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, brokenFileToImport, fileName,
                false));
        Assert.NotNull(ex);

        const string fileNameValid = "convallaria.vpa";
        byte[] fileToImport = await File.ReadAllBytesAsync($"{Folder}/{fileNameValid}");
        ApiListResponse<AnnotationDto> annotationsImported =
            await _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, fileToImport,
                fileNameValid,
                false);

        Assert.AreEqual(11, annotationsImported.Data.Count);

        annotationsImported =
            await _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, fileToImport,
                fileNameValid,
                true);

        Assert.AreEqual(835, annotationsImported.Data.Count);
    }

    [Test]
    [Order(7)]
    public async Task I009_007VerifyVpaForFrontEndTest_MultipleTime()
    {
        for (var i = 0; i < 5; i++)
        {
            const string fileName = "convallaria.vpa";
            byte[] fileToImport = await File.ReadAllBytesAsync($"{Folder}/{fileName}");
            ApiListResponse<AnnotationDto> annotationsImported =
                await _annotationHttpClient_1.AnnotationClient.ImportVpaFile(_slideImage.Data.Id, fileToImport,
                    fileName, false);

            Assert.AreEqual(11, annotationsImported.Data.Count);
        }
    }

    [Test]
    [Order(999)]
    public async Task I009_999DeleteSlideImages()
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