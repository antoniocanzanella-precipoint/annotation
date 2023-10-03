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
[Order(11)]
[NonParallelizable]
[Category("Integration")]
public class I011AccessLevel : ABaseTest
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
    private Guid _annotationId;

    [Test]
    [Order(0)]
    public async Task I011_000UploadSlideImages()
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
    public async Task I011_001VerifyPrivateAccess_Annotation()
    {
        AnnotationDto polygon = await CreateGenericPolygon(_annotationHttpClient_1, AnnotationVisibility.Private, _slideImage.Data.Id);
        _annotationId = polygon.Id.Value;

        #region UPDATE

        polygon.Label += " Update";

        ApiResponse<AnnotationDto> response = await _annotationHttpClient_1.AnnotationClient.UpdateAnnotation(polygon, _slideImage.Data.Id);
        polygon = response.Data;
        Assert.IsTrue(polygon.Label.EndsWith("Update"));

        var ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient_2.AnnotationClient.UpdateAnnotation(polygon, _slideImage.Data.Id));
        Assert.True(ex.HttpStatusCode == HttpStatusCode.Unauthorized);

        #endregion

        #region ADD COORDINATES

        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new[] { new[] { 3.5, 1.2 } }, Index = 1 };
        polygon = (await _annotationHttpClient_1.AnnotationClient.InsertVertices(polygon.Id.Value, vertexUpdateDto))
            .Data;

        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], polygon.Coordinates[2][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], polygon.Coordinates[2][1]);
        ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.InsertVertices(polygon.Id.Value, vertexUpdateDto));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        #endregion

        #region UPDATE COORDINATE

        vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3.5, 1.2 } }, Index = 0 };
        polygon = (await _annotationHttpClient_1.AnnotationClient.SetVertexPosition(polygon.Id.Value, vertexUpdateDto)).Data;

        Assert.NotNull(polygon);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient_2.AnnotationClient.SetVertexPosition(polygon.Id.Value, vertexUpdateDto));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        #endregion

        #region DELETE COORDINATES

        polygon = (await _annotationHttpClient_1.AnnotationClient.DeleteVertex(polygon.Id.Value, 2)).Data;

        Assert.AreEqual(7, polygon.Coordinates.Length);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient_2.AnnotationClient.DeleteVertex(polygon.Id.Value, 2));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        #endregion

        #region VERIFY RETRIEVE BY SLIDE IMAGE ID

        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, result.Data.Count);

        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, result.Data.Count);

        #endregion

        #region DELETE ANNOTATION AND VERIFY

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient_2.AnnotationClient.DeleteAnnotation(_annotationId));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_annotationId);

        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, result.Data.Count);

        ex = Assert.ThrowsAsync<ApiException>(() => _annotationHttpClient_1.AnnotationClient.GetCounterGroups(_annotationId));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        #endregion
    }

    [Test]
    [Order(2)]
    public async Task I011_002VerifyPublicAccess_Annotation()
    {
        AnnotationDto polygon =
            await CreateGenericPolygon(_annotationHttpClient_1, AnnotationVisibility.Public, _slideImage.Data.Id);
        _annotationId = polygon.Id.Value;

        #region UPDATE

        polygon.Label += " Update";

        polygon = (await _annotationHttpClient_2.AnnotationClient.UpdateAnnotation(polygon, _slideImage.Data.Id)).Data;

        Assert.IsTrue(polygon.Label.EndsWith("Update"));

        #endregion

        #region ADD COORDINATES

        var vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3.5, 1.2 } }, Index = 1 };
        polygon = (await _annotationHttpClient_2.AnnotationClient.InsertVertices(polygon.Id.Value, vertexUpdateDto))
            .Data;

        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][0], polygon.Coordinates[2][0]);
        Assert.AreEqual(vertexUpdateDto.CoordinatesDto[0][1], polygon.Coordinates[2][1]);

        #endregion

        #region UPDATE COORDINATE

        vertexUpdateDto = new VertexUpdateDto { CoordinatesDto = new double[1][] { new double[2] { 3.5, 1.2 } }, Index = 0 };
        polygon = (await _annotationHttpClient_2.AnnotationClient.SetVertexPosition(polygon.Id.Value,
            vertexUpdateDto)).Data;

        Assert.NotNull(polygon);

        #endregion

        #region DELETE COORDINATES

        polygon = (await _annotationHttpClient_2.AnnotationClient.DeleteVertex(polygon.Id.Value, 2)).Data;

        Assert.AreEqual(7, polygon.Coordinates.Length);

        #endregion

        #region VERIFY RETRIEVE BY SLIDE IMAGE ID

        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, result.Data.Count);

        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(1, result.Data.Count);

        #endregion

        #region DELETE ANNOTATION AND VERIFY

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.DeleteAnnotation(_annotationId));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_annotationId);

        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, result.Data.Count);

        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.GetCounterGroups(_annotationId));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        #endregion
    }

    [Test]
    [Order(3)]
    public async Task I011_003VerifyPrivateAccess_AnnotationCounter()
    {
        AnnotationDto polygon =
            await CreateGenericPolygon(_annotationHttpClient_1, AnnotationVisibility.Private, _slideImage.Data.Id);
        _annotationId = polygon.Id.Value;

        #region INSERT COUNTER GROUP

        CounterGroupDto counterGroupDto = await CreateGenericCounter(_annotationId, _annotationHttpClient_1);

        Assert.NotNull(counterGroupDto);

        #endregion

        #region UPDATE COUNTER GROUP

        counterGroupDto.Label += " Up";

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.UpdateCounterGroup(counterGroupDto));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        counterGroupDto = (await _annotationHttpClient_1.AnnotationClient.UpdateCounterGroup(counterGroupDto)).Data;

        Assert.IsTrue(counterGroupDto.Label.EndsWith("Up"));

        #endregion

        #region ADD COUNTERS

        var counterArray = new double[2][] { new double[2] { 4.44, 2.55 }, new double[2] { 12, 12 } };
        CounterGroupDto counterGroupRequest = CreateCounterGroup(_annotationId);
        counterGroupRequest.Id = counterGroupDto.Id;
        counterGroupRequest.Counters = counterArray;

        ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.InsertCounters(counterGroupRequest));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        GenericCudOperationDto genericCudOperationDto =
            (await _annotationHttpClient_1.AnnotationClient.InsertCounters(counterGroupRequest)).Data;

        Assert.Greater(genericCudOperationDto.AffectedRows, 0);

        #endregion

        #region UPDATE COUNTER

        ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.SetCounter(counterGroupDto.CounterIds[0], 3, 5));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        ApiResponse<GenericCudOperationDto> counterResponse = await _annotationHttpClient_1.AnnotationClient.SetCounter(
            counterGroupDto.CounterIds[0],
            counterGroupDto.Counters[0][0] + 0.1, counterGroupDto.Counters[0][1] + 0.1);

        Assert.Greater(counterResponse.Data.AffectedRows, 0);

        #endregion

        #region DELETE COUNTER AND COUNTER GROUP

        ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.DeleteCounter(counterGroupDto.CounterIds[0]));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.DeleteCounterGroup(counterGroupDto.Id.Value));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        Assert.DoesNotThrowAsync(() =>
            _annotationHttpClient_1.AnnotationClient.DeleteCounter(counterGroupDto.CounterIds[0]));
        Assert.DoesNotThrowAsync(() =>
            _annotationHttpClient_1.AnnotationClient.DeleteCounterGroup(counterGroupDto.Id.Value));

        #endregion

        #region DELETE ANNOTATION AND VERIFY

        ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.DeleteAnnotation(_annotationId));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_annotationId);

        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, result.Data.Count);

        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.GetCounterGroups(_annotationId));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        #endregion
    }

    [Test]
    [Order(4)]
    public async Task I011_004VerifyPublicAccess_AnnotationCounter()
    {
        AnnotationDto polygon =
            await CreateGenericPolygon(_annotationHttpClient_1, AnnotationVisibility.Public, _slideImage.Data.Id);
        _annotationId = polygon.Id.Value;

        #region INSERT COUNTER GROUP

        CounterGroupDto counterGroupDto = await CreateGenericCounter(_annotationId, _annotationHttpClient_2);

        Assert.NotNull(counterGroupDto);

        #endregion

        #region UPDATE COUNTER GROUP

        counterGroupDto.Label += " Up";

        counterGroupDto = (await _annotationHttpClient_2.AnnotationClient.UpdateCounterGroup(counterGroupDto)).Data;

        Assert.IsTrue(counterGroupDto.Label.EndsWith("Up"));

        #endregion

        #region ADD COUNTERS

        var counterArray = new double[2][] { new double[2] { 4.44, 2.55 }, new double[2] { 12, 12 } };
        CounterGroupDto counterGroupRequest = CreateCounterGroup(_annotationId);
        counterGroupRequest.Id = counterGroupDto.Id;
        counterGroupRequest.Counters = counterArray;

        GenericCudOperationDto genericCudOperationDto =
            (await _annotationHttpClient_2.AnnotationClient.InsertCounters(counterGroupRequest)).Data;

        Assert.Greater(genericCudOperationDto.AffectedRows, 0);

        #endregion

        #region UPDATE COUNTER

        ApiResponse<GenericCudOperationDto> counterResponse = await _annotationHttpClient_2.AnnotationClient.SetCounter(
            counterGroupDto.CounterIds[0],
            counterGroupDto.Counters[0][0] + 0.1, counterGroupDto.Counters[0][1] + 0.1);

        Assert.Greater(counterResponse.Data.AffectedRows, 0);

        #endregion

        #region DELETE COUNTER AND COUNTER GROUP

        Assert.DoesNotThrowAsync(() =>
            _annotationHttpClient_2.AnnotationClient.DeleteCounter(counterGroupDto.CounterIds[0]));

        #endregion

        #region DELETE ANNOTATION AND VERIFY

        var ex = Assert.ThrowsAsync<ApiException>(() =>
            _annotationHttpClient_2.AnnotationClient.DeleteAnnotation(_annotationId));
        Assert.AreEqual(HttpStatusCode.Unauthorized, ex.HttpStatusCode);

        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(_annotationId);

        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, result.Data.Count);

        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_2.AnnotationClient.GetCounterGroups(_annotationId));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(0, result.Data.Count);

        ex = Assert.ThrowsAsync<ApiException>(
            () => _annotationHttpClient_1.AnnotationClient.GetCounterGroups(_annotationId));
        Assert.AreEqual(HttpStatusCode.NotFound, ex.HttpStatusCode);

        #endregion
    }

    [Test]
    [Order(5)]
    public async Task I011_005Verify_AllAccess()
    {
        AnnotationDto polygonPrivate =
            await CreateGenericPolygon(_annotationHttpClient_1, AnnotationVisibility.Private, _slideImage.Data.Id);
        AnnotationDto polygonPublic =
            await CreateGenericPolygon(_annotationHttpClient_1, AnnotationVisibility.Public, _slideImage.Data.Id);
        AnnotationDto polygonReadOnly =
            await CreateGenericPolygon(_annotationHttpClient_1, AnnotationVisibility.ReadOnly, _slideImage.Data.Id);
        AnnotationDto polygonEditable =
            await CreateGenericPolygon(_annotationHttpClient_1, AnnotationVisibility.Editable, _slideImage.Data.Id);

        ApiListResponse<AnnotationDto> result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(4, result.Data.Count);

        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);
        Assert.AreEqual(3, result.Data.Count);

        await _annotationHttpClient_2.AnnotationClient.SetAnnotationsVisibility(new SetAnnotationsVisibilityDto
        {
            Visibility = AnnotationVisibility.Private, SlideImageId = _slideImage.Data.Id
        });
        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);

        Assert.AreEqual(0, result.Data.Where(e => e.Visibility == AnnotationVisibility.Private).ToList().Count);
        Assert.AreEqual(1, result.Data.Where(e => e.Visibility == AnnotationVisibility.Public).ToList().Count);
        Assert.AreEqual(1, result.Data.Where(e => e.Visibility == AnnotationVisibility.ReadOnly).ToList().Count);
        Assert.AreEqual(1, result.Data.Where(e => e.Visibility == AnnotationVisibility.Editable).ToList().Count);
        foreach (AnnotationDto annotation in result.Data)
        {
            if (annotation.Visibility == AnnotationVisibility.Public)
            {
                Assert.IsTrue(annotation.CanEdit);
                Assert.IsFalse(annotation.CanDelete);
                Assert.IsFalse(annotation.CanManageVisibility);
            }
            else if (annotation.Visibility == AnnotationVisibility.ReadOnly)
            {
                Assert.IsFalse(annotation.CanEdit);
                Assert.IsFalse(annotation.CanDelete);
                Assert.IsFalse(annotation.CanManageVisibility);
            }
            else if (annotation.Visibility == AnnotationVisibility.Editable)
            {
                Assert.IsTrue(annotation.CanEdit);
                Assert.IsFalse(annotation.CanDelete);
                Assert.IsFalse(annotation.CanManageVisibility);
            }
        }

        await _annotationHttpClient_2.AnnotationClient.SetAnnotationsVisibility(new SetAnnotationsVisibilityDto
        {
            Visibility = AnnotationVisibility.Public, SlideImageId = _slideImage.Data.Id
        });
        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);

        Assert.AreEqual(0, result.Data.Where(e => e.Visibility == AnnotationVisibility.Private).ToList().Count);
        Assert.AreEqual(1, result.Data.Where(e => e.Visibility == AnnotationVisibility.Public).ToList().Count);
        Assert.AreEqual(1, result.Data.Where(e => e.Visibility == AnnotationVisibility.ReadOnly).ToList().Count);
        Assert.AreEqual(1, result.Data.Where(e => e.Visibility == AnnotationVisibility.Editable).ToList().Count);

        await _annotationHttpClient_1.AnnotationClient.SetAnnotationsVisibility(new SetAnnotationsVisibilityDto
        {
            Visibility = AnnotationVisibility.Private, SlideImageId = _slideImage.Data.Id
        });
        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);

        Assert.AreEqual(4, result.Data.Where(e => e.Visibility == AnnotationVisibility.Private).ToList().Count);
        Assert.AreEqual(0, result.Data.Where(e => e.Visibility == AnnotationVisibility.Public).ToList().Count);
        Assert.AreEqual(0, result.Data.Where(e => e.Visibility == AnnotationVisibility.ReadOnly).ToList().Count);
        Assert.AreEqual(0, result.Data.Where(e => e.Visibility == AnnotationVisibility.Editable).ToList().Count);
        foreach (AnnotationDto annotation in result.Data)
        {
            Assert.IsTrue(annotation.CanEdit);
            Assert.IsTrue(annotation.CanDelete);
            Assert.IsTrue(annotation.CanManageVisibility);
        }

        await _annotationHttpClient_2.AnnotationClient.SetAnnotationsVisibility(new SetAnnotationsVisibilityDto
        {
            Visibility = AnnotationVisibility.Private, SlideImageId = _slideImage.Data.Id
        });
        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);

        Assert.AreEqual(0, result.Data.Count);

        await _annotationHttpClient_1.AnnotationClient.SetAnnotationsVisibility(new SetAnnotationsVisibilityDto
        {
            Visibility = AnnotationVisibility.ReadOnly, SlideImageId = _slideImage.Data.Id
        });
        result = await _annotationHttpClient_1.AnnotationClient.GetAnnotations(_slideImage.Data.Id);

        Assert.AreEqual(0, result.Data.Where(e => e.Visibility == AnnotationVisibility.Private).ToList().Count);
        Assert.AreEqual(0, result.Data.Where(e => e.Visibility == AnnotationVisibility.Public).ToList().Count);
        Assert.AreEqual(4, result.Data.Where(e => e.Visibility == AnnotationVisibility.ReadOnly).ToList().Count);
        Assert.AreEqual(0, result.Data.Where(e => e.Visibility == AnnotationVisibility.Editable).ToList().Count);

        await _annotationHttpClient_2.AnnotationClient.SetAnnotationsVisibility(new SetAnnotationsVisibilityDto
        {
            Visibility = AnnotationVisibility.Private, SlideImageId = _slideImage.Data.Id
        });
        result = await _annotationHttpClient_2.AnnotationClient.GetAnnotations(_slideImage.Data.Id);

        Assert.AreEqual(4, result.Data.Count);


        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(polygonPublic.Id.Value);
        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(polygonPrivate.Id.Value);
        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(polygonReadOnly.Id.Value);
        await _annotationHttpClient_1.AnnotationClient.DeleteAnnotation(polygonEditable.Id.Value);
    }

    [Test]
    [Order(999)]
    public async Task I011_999DeleteSlideImages()
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