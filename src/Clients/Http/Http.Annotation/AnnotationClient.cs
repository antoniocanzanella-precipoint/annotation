using PreciPoint.Ims.Core.DataTransfer.Http;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.Upload;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Clients.Http.Annotation;

/// <summary>
/// Annotation responsible client.
/// </summary>
public class AnnotationClient : AHttpApiClient
{
    private readonly AHttpClient _httpClient;

    internal AnnotationClient(AHttpClient httpClient, string apiEndpoint) : base(httpClient, apiEndpoint)
    {
        _httpClient = httpClient;
    }

    private string AnnotationsEndpoint => $"{ApiEndpoint}/Annotation";

    /// <summary>
    /// Retrieve an existing annotation by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier that differentiates each annotation from another one.</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    /// An api response with the requested annotation or some meta information if not found or there was some
    /// exception.
    /// </returns>
    public Task<ApiResponse<AnnotationDto>> GetAnnotationById(Guid id, CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/{id}";

        return HttpClient.GetJsonAsync<ApiResponse<AnnotationDto>>(requestUrl, null, cancellationToken);
    }

    /// <summary>
    /// Consumers can use it to retrieve annotation counter group by primary key.
    /// </summary>
    /// <param name="id">the unique id of a counter group</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    /// An api response with the requested counter group or some meta information if not found or there was some exception.
    /// </returns>
    public Task<ApiResponse<CounterGroupDto>> GetCounterGroupById(Guid id, CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/CounterGroups/{id}";

        return HttpClient.GetJsonAsync<ApiResponse<CounterGroupDto>>(requestUrl, null, cancellationToken);
    }

    /// <summary>
    /// Consumers can use it to retrieve annotations for a specific slide image.
    /// </summary>
    /// <param name="slideImageId">the unique id of a slide image</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    /// An api response with the requested annotations or some meta information if not found or there was some
    /// exception.
    /// </returns>
    public Task<ApiListResponse<AnnotationDto>> GetAnnotations(Guid slideImageId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/SlideImages/{slideImageId}";

        return HttpClient.GetJsonAsync<ApiListResponse<AnnotationDto>>(requestUrl, null, cancellationToken);
    }

    /// <summary>
    /// Retrieves the deckGl binary data for a given slideImage.
    /// </summary>
    /// <param name="slideImageId">The id of the slide image</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The object containing the headers and binary data</returns>
    public async Task<BinaryDataWithHeaderDto> GetAnnotationsDeckGl(Guid slideImageId, CancellationToken cancellationToken = default)
    {
        var client = _httpClient as MessagePackIdentityHttpClient;
        if (client == null)
        {
            throw new NotSupportedException($"Getting deckgl data, requires {nameof(MessagePackIdentityHttpClient)} httpClient");
        }

        var requestUrl = $"{AnnotationsEndpoint}/SlideImages/{slideImageId}/DeckGl";

        return await client.GetMessagePackAsync<BinaryDataWithHeaderDto>(requestUrl, cancellationToken);
    }

    /// <summary>
    /// Consumers can use it to retrieve all folders with related data for a specific slide image
    /// </summary>
    /// <param name="slideImageId">the unique id of a slide image</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    /// An api response with the requested annotations or some meta information if not found or there was some
    /// exception.
    /// </returns>
    public Task<ApiListResponse<FolderDto>> GetFolders(Guid slideImageId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/Folders/SlideImages/{slideImageId}";

        return HttpClient.GetJsonAsync<ApiListResponse<FolderDto>>(requestUrl, null, cancellationToken);
    }

    /// <summary>
    /// Consumers can use it to retrieve all counter group related to a specific annotation.
    /// </summary>
    /// <param name="annotationId">the unique id of an annotation</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    /// An api response with the requested counter groups or some meta information if not found or there was some
    /// exception.
    /// </returns>
    public Task<ApiListResponse<CounterGroupDto>> GetCounterGroups(Guid annotationId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/{annotationId}/CounterGroups";

        return HttpClient.GetJsonAsync<ApiListResponse<CounterGroupDto>>(requestUrl, null, cancellationToken);
    }

    /// <summary>
    /// Consumers can use it to retrieve global annotation permission based on context user
    /// </summary>
    /// <param name="slideImageId">the unique identifier of slide image</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>
    /// An api response with the requested annotation permissions.
    /// </returns>
    public Task<ApiResponse<AnnotationPermissionsDto>> GetAnnotationPermissions(Guid slideImageId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/SlideImages/{slideImageId}/annotation-permissions";

        return HttpClient.GetJsonAsync<ApiResponse<AnnotationPermissionsDto>>(requestUrl, null, cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to insert an annotation. all data will be flushed into db as is.
    /// </summary>
    /// <param name="annotationDto">the annotation</param>
    /// <param name="slideImageId">the unique id of slide image</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    public Task<ApiResponse<AnnotationDto>> InsertAnnotation(AnnotationDto annotationDto, Guid slideImageId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/SlideImages/{slideImageId}";
        return HttpClient.PostJsonAsync<ApiResponse<AnnotationDto>>(requestUrl, annotationDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to store annotations and folder in a single call.
    /// </summary>
    /// <param name="dto">the full structure that must be persisted</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    public Task<ApiResponse<GenericCudOperationDto>> StoreFoldersWithAnnotations(IList<FolderDto> dto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/Folders";
        return HttpClient.PostJsonAsync<ApiResponse<GenericCudOperationDto>>(requestUrl, dto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to insert a counter group
    /// </summary>
    /// <param name="counterGroupDto">
    ///     <see cref="CounterGroupDto" />
    /// </param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The counter group stored or some meta information if not found or there was some exception.</returns>
    public Task<ApiResponse<CounterGroupDto>> InsertCounterGroup(CounterGroupDto counterGroupDto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/CounterGroups";
        return HttpClient.PostJsonAsync<ApiResponse<CounterGroupDto>>(requestUrl, counterGroupDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Can be used to import a generic vpa file
    /// </summary>
    /// <param name="slideImageId">the slide image unique id</param>
    /// <param name="fileToImport">the byte content of the file to import</param>
    /// <param name="filename">the filename</param>
    /// <param name="reloadAllAnnotations">
    /// indicate if the response should contains only uploaded annotations or all
    /// annotations
    /// </param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>The full list of annotations or some meta information if not found or there was some exception.</returns>
    public async Task<ApiListResponse<AnnotationDto>> ImportVpaFile(Guid slideImageId, byte[] fileToImport, string filename, bool reloadAllAnnotations,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/import-vpa-file";
        using MultipartFormDataContent multipartFormDataContent = AssembleByteArrayContent(fileToImport, filename, slideImageId, reloadAllAnnotations);

        var response = await HttpClient.PostJsonAsync<ApiListResponse<AnnotationDto>>(requestUrl, multipartFormDataContent,
            cancellationToken: cancellationToken);
        return response;
    }

    /// <summary>
    /// consumers can use this method to update an annotation. all data will be flushed into db as is.
    /// </summary>
    /// <param name="annotationDto">the annotation</param>
    /// <param name="slideImageId">the unique id of slide image</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    public Task<ApiResponse<AnnotationDto>> UpdateAnnotation(AnnotationDto annotationDto, Guid slideImageId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/SlideImages/{slideImageId}";
        return HttpClient.PutJsonAsync<ApiResponse<AnnotationDto>>(requestUrl, annotationDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to update a counter group
    /// </summary>
    /// <param name="counterGroupDto">
    ///     <see cref="CounterGroupDto" />
    /// </param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The counter group stored or some meta information if not found or there was some exception.</returns>
    public Task<ApiResponse<CounterGroupDto>> UpdateCounterGroup(CounterGroupDto counterGroupDto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/CounterGroups";
        return HttpClient.PutJsonAsync<ApiResponse<CounterGroupDto>>(requestUrl, counterGroupDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to add generic numbers of coordinates to a specific annotation, starting from a
    /// defined
    /// index
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <param name="vertexUpdateDto">
    ///     <see cref="VertexUpdateDto" />
    /// </param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    public Task<ApiResponse<AnnotationDto>> InsertVertices(Guid annotationId, VertexUpdateDto vertexUpdateDto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/{annotationId}/insert-vertices";
        return HttpClient.PutJsonAsync<ApiResponse<AnnotationDto>>(requestUrl, vertexUpdateDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to set coordinates of a specified vertex.
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <param name="vertexUpdateDto">
    ///     <see cref="VertexUpdateDto" />
    /// </param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    public Task<ApiResponse<AnnotationDto>> SetVertexPosition(Guid annotationId, VertexUpdateDto vertexUpdateDto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/{annotationId}/set-vertex-position";
        return HttpClient.PutJsonAsync<ApiResponse<AnnotationDto>>(requestUrl, vertexUpdateDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to insert a list of counters into a specific counter group
    /// </summary>
    /// <param name="counterGroupDto">
    ///     <see cref="CounterGroupDto" />
    /// </param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="GenericCudOperationDto" />
    /// </returns>
    public Task<ApiResponse<GenericCudOperationDto>> InsertCounters(CounterGroupDto counterGroupDto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/CounterGroups/insert-counters";
        return HttpClient.PutJsonAsync<ApiResponse<GenericCudOperationDto>>(requestUrl, counterGroupDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to update a specific counter of a single counter group
    /// </summary>
    /// <param name="counterId">the unique counter id</param>
    /// <param name="x">the x coordinate</param>
    /// <param name="y">the y coordinate</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="GenericCudOperationDto" />
    /// </returns>
    public Task<ApiResponse<GenericCudOperationDto>> SetCounter(Guid counterId, double x, double y,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/CounterGroups/Counters/{counterId}/set-x/{x}/set-y/{y}";
        return HttpClient.PutJsonAsync<ApiResponse<GenericCudOperationDto>>(requestUrl, null, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this to set the radius of a Circle annotation. we assume that the radius is calculated on X axis.
    /// f.e. the center is (0,0) and the point on the circumference is (5,0) in this case the radius is also 5.
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <param name="radius">the new radius value</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="AnnotationDto" />
    /// </returns>
    public Task<ApiResponse<AnnotationDto>> SetRadius(Guid annotationId, double radius,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/{annotationId}/set-radius/{radius}";
        return HttpClient.PutJsonAsync<ApiResponse<AnnotationDto>>(requestUrl, null, null, cancellationToken);
    }

    /// <summary>
    /// Consumers can use this to set the color of a specific annotation. the api as no power into the meanings of these
    /// numbers. we act only like a storage.
    /// </summary>
    /// <param name="annotationDto">
    ///     <see cref="AnnotationDto" />
    /// </param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="AnnotationDto" />
    /// </returns>
    public Task<ApiResponse<AnnotationDto>> SetColor(AnnotationDto annotationDto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/set-color";
        return HttpClient.PutJsonAsync<ApiResponse<AnnotationDto>>(requestUrl, annotationDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this to translate a list of annotations with all related entities (eg counters)
    /// </summary>
    /// <param name="translateDto">
    ///     <see cref="TranslateDto" />
    /// </param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="AnnotationDto" />
    /// </returns>
    public Task<ApiListResponse<AnnotationDto>> SetTranslation(TranslateDto translateDto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/set-translation";
        return HttpClient.PutJsonAsync<ApiListResponse<AnnotationDto>>(requestUrl, translateDto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to massively update all annotations visibility for a specific slide image.
    /// Only annotation where the user has write permission will be modified.
    /// </summary>
    /// <param name="dto">
    ///     <see cref="SetAnnotationsVisibilityDto" />
    /// </param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="GenericCudOperationDto" />
    /// </returns>
    public Task<ApiResponse<GenericCudOperationDto>> SetAnnotationsVisibility(SetAnnotationsVisibilityDto dto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/set-annotations-visibility";

        return HttpClient.PutJsonAsync<ApiResponse<GenericCudOperationDto>>(requestUrl, dto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to set the global annotation permission. <see cref="AnnotationPermission" />
    /// </summary>
    /// <param name="dto">The new global annotation permission to be store</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    /// The stored global annotation permission.
    /// </returns>
    public Task<ApiResponse<AnnotationPermissionsDto>> SetAnnotationPermissions(AnnotationPermissionsDto dto,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/set-annotation-permissions";

        return HttpClient.PutJsonAsync<ApiResponse<AnnotationPermissionsDto>>(requestUrl, dto, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to delete a specified vertex.
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <param name="index">indicates the index of coordinates array</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    public Task<ApiResponse<AnnotationDto>> DeleteVertex(Guid annotationId, int index,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/{annotationId}/delete-vertex/{index}";
        return HttpClient.DeleteJsonAsync<ApiResponse<AnnotationDto>>(requestUrl, null, null, cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to delete a specified annotation.
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    public Task<ApiResponse<DeleteOperationDto>> DeleteAnnotation(Guid annotationId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/{annotationId}";
        return HttpClient.DeleteJsonAsync<ApiResponse<DeleteOperationDto>>(requestUrl, null, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to delete all annotation of a slide image.
    /// </summary>
    /// <param name="slideImageId">the annotation unique identifier</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    public Task<ApiResponse<DeleteOperationDto>> DeleteAnnotations(Guid slideImageId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/SlideImages/{slideImageId}";
        return HttpClient.DeleteJsonAsync<ApiResponse<DeleteOperationDto>>(requestUrl, null, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to delete a counter group.
    /// </summary>
    /// <param name="counterGroupId">the counter group unique identifier</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    public Task<ApiResponse<DeleteOperationDto>> DeleteCounterGroup(Guid counterGroupId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/CounterGroups/{counterGroupId}";
        return HttpClient.DeleteJsonAsync<ApiResponse<DeleteOperationDto>>(requestUrl, null, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to delete a single counter.
    /// </summary>
    /// <param name="counterId">the counter unique identifier</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    public Task<ApiResponse<DeleteOperationDto>> DeleteCounter(Guid counterId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/CounterGroups/Counters/{counterId}";
        return HttpClient.DeleteJsonAsync<ApiResponse<DeleteOperationDto>>(requestUrl, null, null,
            cancellationToken);
    }

    /// <summary>
    /// Consumers can use this method to delete a specified annotation.
    /// </summary>
    /// <param name="folderId">the folder unique identifier</param>
    /// <param name="cancellationToken">the cancellation token</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    public Task<ApiResponse<DeleteOperationDto>> DeleteFolder(Guid folderId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{AnnotationsEndpoint}/Folders/{folderId}";
        return HttpClient.DeleteJsonAsync<ApiResponse<DeleteOperationDto>>(requestUrl, null, null,
            cancellationToken);
    }

    private MultipartFormDataContent AssembleByteArrayContent(byte[] fileToImport, string fileName,
        Guid slideImageId,
        bool reloadAllAnnotations)
    {
        var imageContent = new ByteArrayContent(fileToImport);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml");

        var formDataContent =
            new MultipartFormDataContent("----PreciPointHttpUpload" + DateTimeOffset.Now.ToUnixTimeMilliseconds())
            {
                {
                    imageContent, VpaFileFormKeys.VpaFile, fileName
                },
                {
                    new StringContent(slideImageId.ToString()), VpaFileFormKeys.SlideImageId
                },
                {
                    new StringContent(reloadAllAnnotations.ToString()), VpaFileFormKeys.ReloadAllAnnotations
                }
            };

        return formDataContent;
    }
}