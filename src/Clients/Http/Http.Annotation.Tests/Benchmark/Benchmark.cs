using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Config;
using PreciPoint.Ims.Clients.Http.Annotation.Tests.Extensions;
using PreciPoint.Ims.Clients.Http.ImageManagement;
using PreciPoint.Ims.Clients.Http.WholeSlideImages;
using PreciPoint.Ims.Core.DataTransfer.Factories;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Enums;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using PreciPoint.Ims.Shared.DataTransferObjects.Upload;
using PreciPoint.Ims.Utils.TestUtils.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Benchmark;

[TestFixture]
[Order(1)]
[NonParallelizable]
[Category("Benchmark")]
public class Benchmark
{
    [OneTimeSetUp]
    public void SetUp()
    {
        _configuration = new JsonSettings().Configuration.Get<AnnotationTestConfig>();
        _httpClientFactory = new HttpClientFactory(_configuration.SecuritySystem);
        _apiUrl = $"{_configuration.AnnotationHost}/api";

        _adminImageManagementHttpClient = new ImageManagementHttpClient(_httpClientFactory.CreateAdminHttpClient(_configuration), _apiUrl);

        _adminAnnotationHttpClient = new AnnotationHttpClient(_httpClientFactory.CreateAdminHttpClient(_configuration), _apiUrl);
    }

    [OneTimeTearDown]
    public void PrintResults()
    {
        if (_summary is not null)
        {
            Assert.NotNull(_summary.ResultsDirectoryPath);
            FileAssert.Exists(_summary.LogFilePath);

            IEnumerable<string> filesPath = Directory.EnumerateFiles(_summary.ResultsDirectoryPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".md"));

            foreach (string filePath in filesPath)
            {
                FileAssert.Exists(filePath);

                string nameNoExt = Path.GetFileNameWithoutExtension(filePath);
                string fileAllText = File.ReadAllText(filePath);

                TestContext.Progress.WriteLine(
                    "########################################################################");

                TestContext.Progress.WriteLine($"RESULTS: {nameNoExt}");
                TestContext.Progress.WriteLine(fileAllText);

                TestContext.Progress.WriteLine("#");
                TestContext.Progress.WriteLine("#");
            }
        }

        _adminAnnotationHttpClient.Dispose();
        _adminImageManagementHttpClient.Dispose();
    }

    private Summary _summary;
    private const string RootFolder = "./files/Benchmark";
    private AnnotationTestConfig _configuration;
    private HttpClientFactory _httpClientFactory;
    private string _apiUrl;
    private ImageManagementHttpClient _adminImageManagementHttpClient;
    private AnnotationHttpClient _adminAnnotationHttpClient;
    private ApiResponse<SlideImageDto> _slideImage;

    //TODO-AC: benchmark in the current status will not work. need to restructure accordingly to slide image sent in upload.

    [Test]
    [Order(0)]
    public async Task I002_000UploadSlideImages()
    {
        _slideImage = await SuggestSlideImage();
        var wholeSlideImagesAdmin = new WholeSlideImagesHttpClient(
            _httpClientFactory.CreateAdminHttpClient(_configuration),
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
    public void B001_001LoadData()
    {
        IEnumerable<string> dirs = Directory.EnumerateDirectories(RootFolder);
        foreach (string dir in dirs)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(dir);

            foreach (string file in files)
            {
                string guidString = dir.Substring(dir.Length - 36);
                ApiListResponse<AnnotationDto> result = ImportFile(new Guid(guidString), file);

                if (result.Data.Count <= 0)
                {
                    throw new Exception("Annotation list is not loaded correctly");
                }
            }
        }
    }

    [Test]
    [Order(2)]
    public void B001_002GetAnnotations()
    {
        _summary = BenchmarkRunner.Run<JobGetAnnotations>(
            ManualConfig
                .Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));

        Assert.IsNotEmpty(_summary.Reports);
        Assert.IsNotEmpty(_summary.Reports[0].ExecuteResults);
        Assert.AreEqual(0, _summary.Reports[0].ExecuteResults[0].ExitCode);
        FileAssert.Exists(_summary.LogFilePath);
    }

    [Test]
    [Order(3)]
    public void B001_003GetAnnotationById()
    {
        _summary = BenchmarkRunner.Run<JobGetAnnotationById>(
            ManualConfig
                .Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));

        Assert.IsNotEmpty(_summary.Reports);
        Assert.IsNotEmpty(_summary.Reports[0].ExecuteResults);
        Assert.AreEqual(0, _summary.Reports[0].ExecuteResults[0].ExitCode);
        FileAssert.Exists(_summary.LogFilePath);
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
            .EnumerateFiles(Path.Combine(_configuration.ClientAccessPathRoot, _configuration.UploadFolder))
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

    private ApiListResponse<AnnotationDto> ImportFile(Guid slideImageId, string filePathName)
    {
        byte[] fileToImport = File.ReadAllBytes(filePathName);
        return _adminAnnotationHttpClient.AnnotationClient.ImportVpaFile(slideImageId, fileToImport, filePathName, true)
            .GetAwaiter().GetResult();
    }
}