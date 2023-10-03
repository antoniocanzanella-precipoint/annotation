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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Integration;

[TestFixture]
[Order(17)]
[NonParallelizable]
[Category("Integration")]
public class I017Folders : ABaseTest
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
    public async Task I017_000UploadSlideImages()
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
    public async Task I017_001VerifyInsertByFolderWithSimpleAiStructure()
    {
        IList<FolderDto> folders = CreateFolders();

        ApiResponse<GenericCudOperationDto> reply = await _annotationHttpClient_1.AnnotationClient.StoreFoldersWithAnnotations(folders);

        Assert.Greater(reply.Data.AffectedRows, 0);
    }

    [Test]
    [Order(2)]
    public async Task I017_002VerifyGetFolders()
    {
        ApiListResponse<FolderDto> reply = await _annotationHttpClient_1.AnnotationClient.GetFolders(_slideImage.Data.Id);
        Assert.AreEqual(1, reply.Data.Count);
        Assert.AreEqual(2, reply.Data.First().SubFolders.Count);
    }

    [Test]
    [Order(3)]
    public async Task I017_003VerifyDeleteFolders()
    {
        ApiListResponse<FolderDto> foldersReply = await _annotationHttpClient_1.AnnotationClient.GetFolders(_slideImage.Data.Id);
        ApiResponse<DeleteOperationDto> deleteReply =
            await _annotationHttpClient_1.AnnotationClient.DeleteFolder(foldersReply.Data.First().Id.Value);

        Assert.Greater(deleteReply.Data.NumberOfEntityRemoved, 0);

        Assert.AreEqual(0, (await _annotationHttpClient_1.AnnotationClient.GetFolders(_slideImage.Data.Id)).Data.Count);
    }

    private IList<FolderDto> CreateFolders()
    {
        return new List<FolderDto>
        {
            new()
            {
                Name = "root folder",
                Annotations = CreateRoiPolygon(),
                SubFolders = new List<FolderDto>
                {
                    new()
                    {
                        Name = "folder liv 1 group 1",
                        Annotations = CreateGroupOne(),
                        SubFolders = new List<FolderDto> { new() { Name = "folder liv 2 group 1", Annotations = CreateGroupTree() } }
                    },
                    new() { Name = "folder liv 1 group 2", Annotations = CreateGroupTwo() }
                }
            }
        };
    }

    private List<AnnotationDto> CreateRoiPolygon()
    {
        return new List<AnnotationDto>
        {
            new()
            {
                Label = "ROI",
                Confidence = 0,
                Color = new[] { 20, 130, 255, 255 },
                AnnotationType = AnnotationType.Polygon,
                Visibility = AnnotationVisibility.Public,
                Coordinates =
                    new double[8][]
                    {
                        new double[2] { 6, 4 }, new double[2] { 10, 2 }, new double[2] { 14, 4 }, new double[2] { 16, 8 }, new double[2] { 12, 10 },
                        new double[2] { 8, 10 }, new double[2] { 4, 8 }, new double[2] { 6, 4 }
                    },
                SlideImageId = _slideImage.Data.Id
            }
        };
    }

    private List<AnnotationDto> CreateGroupOne()
    {
        return new List<AnnotationDto>
        {
            new()
            {
                Label = "annota 1",
                Confidence = 1,
                Color = new[] { 20, 130, 255, 255 },
                AnnotationType = AnnotationType.Rectangular,
                Visibility = AnnotationVisibility.Public,
                Coordinates =
                    new double[5][] { new double[2] { 6, 8 }, new double[2] { 6, 6 }, new double[2] { 8, 6 }, new double[2] { 8, 8 }, new double[2] { 6, 8 } },
                SlideImageId = _slideImage.Data.Id
            },
            new()
            {
                Label = "annota 2",
                Confidence = 0.8,
                Color = new[] { 20, 130, 255, 255 },
                AnnotationType = AnnotationType.Rectangular,
                Visibility = AnnotationVisibility.Public,
                Coordinates =
                    new double[5][]
                    {
                        new double[2] { 12, 8 }, new double[2] { 12, 6 }, new double[2] { 14, 6 }, new double[2] { 14, 8 }, new double[2] { 12, 8 }
                    },
                SlideImageId = _slideImage.Data.Id
            }
        };
    }

    private List<AnnotationDto> CreateGroupTwo()
    {
        return new List<AnnotationDto>
        {
            new()
            {
                Label = "annota 3",
                Confidence = 1,
                Color = new[] { 20, 130, 255, 255 },
                AnnotationType = AnnotationType.Rectangular,
                Visibility = AnnotationVisibility.Public,
                Coordinates =
                    new double[5][]
                    {
                        new double[2] { 8, 6 }, new double[2] { 8, 4 }, new double[2] { 12, 4 }, new double[2] { 12, 6 }, new double[2] { 8, 6 }
                    },
                SlideImageId = _slideImage.Data.Id
            }
        };
    }

    private List<AnnotationDto> CreateGroupTree()
    {
        return new List<AnnotationDto>
        {
            new()
            {
                Label = "annota 3 group 1 liv 1",
                Confidence = 1,
                Color = new[] { 20, 130, 255, 255 },
                AnnotationType = AnnotationType.Rectangular,
                Visibility = AnnotationVisibility.Public,
                Coordinates =
                    new double[5][]
                    {
                        new double[2] { 8, 6 }, new double[2] { 8, 4 }, new double[2] { 12, 4 }, new double[2] { 12, 6 }, new double[2] { 8, 6 }
                    },
                SlideImageId = _slideImage.Data.Id
            }
        };
    }

    [Test]
    [Order(999)]
    public async Task I017_999DeleteSlideImages()
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