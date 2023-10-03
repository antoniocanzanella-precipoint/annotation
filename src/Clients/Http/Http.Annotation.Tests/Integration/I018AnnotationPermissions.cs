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
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation.Tests.Integration;

[TestFixture]
[Order(18)]
[NonParallelizable]
[Category("Integration")]
internal class I018AnnotationPermissions : ABaseTest
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
        _annotationAnonymousHttpClient = new AnnotationHttpClient(HttpClientFactory.CreateAnonymousHttpClient(), _apiUrl);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _annotationHttpClient_1.Dispose();
        _annotationHttpClient_2.Dispose();
        _adminAnnotationHttpClient.Dispose();
        _adminImageManagementHttpClient.Dispose();
        _annotationAnonymousHttpClient.Dispose();
    }

    private ImageManagementHttpClient _adminImageManagementHttpClient;
    private string _apiUrl;
    private AnnotationHttpClient _adminAnnotationHttpClient;
    private AnnotationHttpClient _annotationHttpClient_1;
    private AnnotationHttpClient _annotationHttpClient_2;
    private AnnotationHttpClient _annotationAnonymousHttpClient;
    private ApiResponse<SlideImageDto> _slideImage;
    private ApiResponse<AnnotationDto> _annotation;

    [Test]
    [Order(0)]
    public async Task I018_000UploadSlideImages()
    {
        _slideImage = await SuggestSlideImage();
        var wholeSlideImagesAdmin = new WholeSlideImagesHttpClient(
            HttpClientFactory.CreateAdminHttpClient(Configuration),
            _slideImage.Data.Storage.ClientAccessPathRoot + "/api");

        ApiResponse<UploadProgressDto<SlideImageDto>> uploadProgress = await wholeSlideImagesAdmin.UploadClient
            .UploadWholeSlideImage(GetFileToUploadAbsPath(), _slideImage.Data.Id, new NullProgressReporter());

        Assert.NotNull(uploadProgress.Data.Entity.CreatedAt);
    }

    [Test]
    [Order(1)]
    public async Task I018_001VerifyPermissionDefault()
    {
        ApiResponse<AnnotationPermissionsDto> result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsTrue(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreEqual(_adminAnnotationHttpClient.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsFalse(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationHttpClient_1.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsFalse(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationHttpClient_2.UserInfo.UserId, result.Data.OwnerId);
    }

    [Test]
    [Order(2)]
    public async Task I018_002VerifyPermission_Disabled()
    {
        ApiResponse<AnnotationPermissionsDto> result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsTrue(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreEqual(_adminAnnotationHttpClient.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsFalse(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationHttpClient_1.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsFalse(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationHttpClient_2.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsFalse(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationAnonymousHttpClient.UserInfo.UserId, result.Data.OwnerId);

        var ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.SetAnnotationPermissions(result.Data));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);

        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_2.AnnotationClient.SetAnnotationPermissions(result.Data));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Test]
    [Order(3)]
    public async Task I018_003VerifyPermission_View()
    {
        ApiResponse<AnnotationPermissionsDto> result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        result.Data.Permission = AnnotationPermission.View;
        result = await _adminAnnotationHttpClient.AnnotationClient.SetAnnotationPermissions(result.Data);

        Assert.AreEqual(AnnotationPermission.View, result.Data.Permission);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsTrue(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreEqual(_adminAnnotationHttpClient.UserInfo.UserId, result.Data.OwnerId);

        result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsTrue(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreEqual(_adminAnnotationHttpClient.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationHttpClient_1.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationHttpClient_2.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationAnonymousHttpClient.UserInfo.UserId, result.Data.OwnerId);

        var ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.SetAnnotationPermissions(result.Data));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);

        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_2.AnnotationClient.SetAnnotationPermissions(result.Data));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Test]
    [Order(4)]
    public async Task I018_004VerifyPermission_Draw()
    {
        ApiResponse<AnnotationPermissionsDto> result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        result.Data.Permission = AnnotationPermission.Draw;
        result = await _adminAnnotationHttpClient.AnnotationClient.SetAnnotationPermissions(result.Data);

        Assert.AreEqual(AnnotationPermission.Draw, result.Data.Permission);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsTrue(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreEqual(_adminAnnotationHttpClient.UserInfo.UserId, result.Data.OwnerId);

        result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsTrue(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreEqual(_adminAnnotationHttpClient.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationHttpClient_1.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationHttpClient_2.UserInfo.UserId, result.Data.OwnerId);

        result = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsFalse(result.Data.CanChangeAccess);
        Assert.IsFalse(result.Data.CanDrawOnSlide);
        Assert.AreNotEqual(_annotationAnonymousHttpClient.UserInfo.UserId, result.Data.OwnerId);

        var ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.SetAnnotationPermissions(result.Data));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);

        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_2.AnnotationClient.SetAnnotationPermissions(result.Data));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Test]
    [Order(5)]
    public async Task I018_005AnnotationPermission_Disabled_And_Annotations_Private()
    {
        ApiResponse<AnnotationPermissionsDto> result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        result.Data.Permission = AnnotationPermission.Disabled;
        result = await _adminAnnotationHttpClient.AnnotationClient.SetAnnotationPermissions(result.Data);

        Assert.AreEqual(AnnotationPermission.Disabled, result.Data.Permission);
        Assert.IsTrue(result.Data.CanView);
        Assert.IsTrue(result.Data.CanChangeAccess);
        Assert.IsTrue(result.Data.CanDrawOnSlide);
        Assert.AreEqual(_adminAnnotationHttpClient.UserInfo.UserId, result.Data.OwnerId);

        _annotation =
            await CreateAnnotation(_adminAnnotationHttpClient, _slideImage.Data.Id, AnnotationVisibility.Private);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        var ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Test]
    [Order(6)]
    public async Task I018_006AnnotationPermission_Disabled_And_Annotations_Public()
    {
        _annotation.Data.Visibility = AnnotationVisibility.Public;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        var ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Test]
    [Order(7)]
    public async Task I018_007AnnotationPermission_Disabled_And_Annotations_ReadOnly()
    {
        _annotation.Data.Visibility = AnnotationVisibility.ReadOnly;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        var ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Test]
    [Order(8)]
    public async Task I018_008AnnotationPermission_Disabled_And_Annotations_Editable()
    {
        _annotation.Data.Visibility = AnnotationVisibility.Editable;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        var ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);
    }

    [Test]
    [Order(9)]
    public async Task I018_009AnnotationPermission_View_And_Annotations_Private()
    {
        ApiResponse<AnnotationPermissionsDto> result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        result.Data.Permission = AnnotationPermission.View;
        result = await _adminAnnotationHttpClient.AnnotationClient.SetAnnotationPermissions(result.Data);

        _annotation.Data.Visibility = AnnotationVisibility.Private;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);
    }

    [Test]
    [Order(10)]
    public async Task I018_010AnnotationPermission_View_And_Annotations_Public()
    {
        _annotation.Data.Visibility = AnnotationVisibility.Public;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    [Test]
    [Order(11)]
    public async Task I018_011AnnotationPermission_View_And_Annotations_ReadOnly()
    {
        _annotation.Data.Visibility = AnnotationVisibility.ReadOnly;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    [Test]
    [Order(12)]
    public async Task I018_012AnnotationPermission_View_And_Annotations_Editable()
    {
        _annotation.Data.Visibility = AnnotationVisibility.Editable;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    [Test]
    [Order(13)]
    public async Task I018_013AnnotationPermission_Draw_And_Annotations_Private()
    {
        ApiResponse<AnnotationPermissionsDto> result = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotationPermissions(_slideImage.Data.Id);
        result.Data.Permission = AnnotationPermission.Draw;
        result = await _adminAnnotationHttpClient.AnnotationClient.SetAnnotationPermissions(result.Data);

        _annotation.Data.Visibility = AnnotationVisibility.Private;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);
    }

    [Test]
    [Order(14)]
    public async Task I018_014AnnotationPermission_Draw_And_Annotations_Public()
    {
        _annotation.Data.Visibility = AnnotationVisibility.Public;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    [Test]
    [Order(15)]
    public async Task I018_015AnnotationPermission_Draw_And_Annotations_ReadOnly()
    {
        _annotation.Data.Visibility = AnnotationVisibility.ReadOnly;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    [Test]
    [Order(16)]
    public async Task I018_016AnnotationPermission_Draw_And_Annotations_Editable()
    {
        _annotation.Data.Visibility = AnnotationVisibility.Editable;
        await _adminAnnotationHttpClient.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    [Test]
    [Order(17)]
    public async Task I018_017DeleteAnnotations()
    {
        await _adminAnnotationHttpClient.AnnotationClient.DeleteAnnotations(_slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);
    }

    [Test]
    [Order(18)]
    public async Task I018_018AnnotationPermission_Draw_And_Annotations_Private_And_OwnerIsNormalUser()
    {
        _annotation =
            await CreateAnnotation(_annotationHttpClient_1, _slideImage.Data.Id, AnnotationVisibility.Private);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, annotations.Data.Count);
    }

    [Test]
    [Order(19)]
    public async Task I018_019AnnotationPermission_Draw_And_Annotations_ReadOnly_And_OwnerIsNormalUser()
    {
        _annotation.Data.Visibility = AnnotationVisibility.ReadOnly;
        await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    [Test]
    [Order(20)]
    public async Task I018_020AnnotationPermission_Draw_And_Annotations_Editable_And_OwnerIsNormalUser()
    {
        _annotation.Data.Visibility = AnnotationVisibility.Editable;
        await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    [Test]
    [Order(21)]
    public async Task I018_021AnnotationPermission_Draw_And_Annotations_Public_And_OwnerIsNormalUser()
    {
        _annotation.Data.Visibility = AnnotationVisibility.Public;
        await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(_annotation.Data, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> annotations = await _adminAnnotationHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsTrue(annotations.Data.First().CanManageVisibility);
        Assert.IsTrue(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsTrue(annotations.Data.First().CanEdit);

        annotations = await _annotationAnonymousHttpClient.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, annotations.Data.Count);
        Assert.IsFalse(annotations.Data.First().CanManageVisibility);
        Assert.IsFalse(annotations.Data.First().CanDelete);
        Assert.IsFalse(annotations.Data.First().CanEdit);
    }

    //TODO-AC: improve permission checking on Commands.

    private async Task<ApiResponse<AnnotationDto>> CreateAnnotation(AnnotationHttpClient annotationHttpClient,
        Guid slideImageId, AnnotationVisibility annotationVisibility)
    {
        AnnotationDto polygon = CreateAnnotation(AnnotationType.Polygon, annotationVisibility);
        var coordinatesDto =
            new double[7][]
            {
                new double[2] { 3.3, 3 }, new double[2] { 5, 2.5 }, new double[2] { 7.5, 2.5 }, new double[2] { 8, 4 }, new double[2] { 7, 6 },
                new double[2] { 6, 4 }, new double[2] { 3.3, 3 }
            };
        polygon.Coordinates = coordinatesDto;
        return await annotationHttpClient.AnnotationClient.InsertAnnotation(polygon, slideImageId);
    }

    [Test]
    [Order(999)]
    public async Task I018_999DeleteSlideImages()
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