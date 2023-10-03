using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.Annotation.API.Form;
using PreciPoint.Ims.Services.Annotation.Application.Authorization;
using PreciPoint.Ims.Services.Annotation.Application.Command;
using PreciPoint.Ims.Services.Annotation.Application.Queries;
using PreciPoint.Ims.Services.Annotation.Application.Queries.DeckGlGetAnnotations;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.DeckGl;
using PreciPoint.Ims.Services.Annotation.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.API.Controllers;

/// <summary>
/// Provides HTTP access to annotations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AnnotationController : AControllerBase
{
    private readonly ILogger<AnnotationController> _logger;

    /// <summary>
    /// Create an instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="logger"></param>
    public AnnotationController(ILogger<AnnotationController> logger, IMediator mediator) : base(mediator)
    {
        _logger = logger;
    }

    #region Get Request

    /// <summary>
    /// Consumers can use it to retrieve annotation by primary key
    /// </summary>
    /// <param name="id">the unique id of an annotation</param>
    /// <returns>
    /// An api response with the requested annotation or some meta information if not found or there was some exception.
    /// </returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AnnotationPolicy.ViewAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationDto>>> GetSlideImageAnnotationById(Guid id)
    {
        var request = new GetAnnotationById(id);

        AnnotationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use it to retrieve annotation counter group by primary key
    /// </summary>
    /// <param name="id">the unique id of a counter group</param>
    /// <returns>
    /// An api response with the requested counter group or some meta information if not found or there was some exception.
    /// </returns>
    [HttpGet("CounterGroups/{id:guid}")]
    [Authorize(Policy = AnnotationPolicy.ViewAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<CounterGroupDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<CounterGroupDto>>> GetCounterGroupById(Guid id)
    {
        var request = new GetAnnotationCounterGroupById(id);

        CounterGroupDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<CounterGroupDto>(result));
    }

    /// <summary>
    /// Consumers can use it to retrieve all annotations for a specific slide image
    /// </summary>
    /// <param name="slideImageId">the unique id of a slide image</param>
    /// <returns>
    /// An api response with the requested annotations or some meta information if not found or there was some exception.
    /// </returns>
    [HttpGet("SlideImages/{slideImageId:guid}")]
    [Authorize(Policy = AnnotationPolicy.ViewAnnotations)]
    [ProducesResponseType(typeof(ApiListResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiListResponse<AnnotationDto>>> GetAnnotations(Guid slideImageId)
    {
        var request = new GetAnnotations(slideImageId);

        IReadOnlyList<AnnotationDto> result = await Mediator.Send(request);

        return Ok(new ApiListResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Similar to SlideImages/{id}, but the response is made up of raw binary data, that can be consumed by
    /// deckGl without conversion.
    /// </summary>
    /// <param name="slideImageId">The id of the slide image</param>
    /// <returns>The raw binary data. Made up of a header and the actual deckGl data.</returns>
    [HttpGet("SlideImages/{slideImageId:guid}/DeckGl")]
    [Authorize(Policy = AnnotationPolicy.ViewAnnotations)]
    [Produces("application/x-msgpack")]
    public async Task<ActionResult> GetAnnotationsForDeckGl(Guid slideImageId)
    {
        var request = new GetAnnotationsDeckGl(slideImageId);

        BinaryDataWithHeaderDto result = await Mediator.Send(request);

        return Ok(result);
    }

    /// <summary>
    /// Consumers can use it to retrieve all folders with related data for a specific slide image
    /// </summary>
    /// <param name="slideImageId">the unique id of a slide image</param>
    /// <returns>
    /// An api response with the requested annotations or some meta information if not found or there was some exception.
    /// </returns>
    [HttpGet("Folders/SlideImages/{slideImageId:guid}")]
    [Authorize(Policy = AnnotationPolicy.ViewFolders)]
    [ProducesResponseType(typeof(ApiListResponse<FolderDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiListResponse<FolderDto>>> GetFolders(Guid slideImageId)
    {
        var request = new GetFolders(slideImageId);

        IReadOnlyList<FolderDto> result = await Mediator.Send(request);

        return Ok(new ApiListResponse<FolderDto>(result));
    }

    /// <summary>
    /// Consumers can use it to retrieve all counter group related to a specific annotation
    /// </summary>
    /// <param name="annotationId">the unique id of an annotation</param>
    /// <returns>
    /// An api response with the requested counter groups or some meta information if not found or there was some
    /// exception.
    /// </returns>
    [HttpGet("{annotationId:guid}/CounterGroups")]
    [Authorize(Policy = AnnotationPolicy.ViewAnnotations)]
    [ProducesResponseType(typeof(ApiListResponse<CounterGroupDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiListResponse<CounterGroupDto>>> GetCounterGroups(Guid annotationId)
    {
        var request = new GetAnnotationCounterGroups(annotationId);

        IReadOnlyList<CounterGroupDto> result = await Mediator.Send(request);

        return Ok(new ApiListResponse<CounterGroupDto>(result));
    }

    /// <summary>
    /// Consumers can use it to retrieve global annotation permission based on context user
    /// </summary>
    /// <param name="slideImageId">the unique identifier of slide image</param>
    /// <returns>
    /// An api response with the requested annotation permissions.
    /// </returns>
    [HttpGet("SlideImages/{slideImageId:guid}/annotation-permissions")]
    [Authorize(Policy = AnnotationPolicy.ViewAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationPermissionsDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationPermissionsDto>>> GetAnnotationPermissions(Guid slideImageId)
    {
        var request = new GetAnnotationPermissions(slideImageId);

        AnnotationPermissionsDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationPermissionsDto>(result));
    }

    #endregion

    #region Post Request

    /// <summary>
    /// Consumers can use this method to create a new annotation for a specific slide image
    /// </summary>
    /// <param name="annotationDto">the annotation definition</param>
    /// <param name="slideImageId">the unique id of a slide image</param>
    /// <returns>
    /// The annotation stored or some meta information if not found or there was some exception.
    /// </returns>
    [HttpPost("SlideImages/{slideImageId:guid}")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationDto>>> InsertAnnotation([FromBody] AnnotationDto annotationDto, Guid slideImageId)
    {
        var request = new UpsertAnnotation(slideImageId, annotationDto);

        AnnotationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to store annotations and folder in a single call.
    /// </summary>
    /// <param name="dto">
    ///     <see cref="FolderDto" />
    /// </param>
    /// <returns>
    /// The annotation stored or some meta information if not found or there was some exception.
    /// </returns>
    [HttpPost("Folders")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotationsByFolders)]
    [ProducesResponseType(typeof(ApiResponse<GenericCudOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<GenericCudOperationDto>>> StoreFoldersWithAnnotations([FromBody] IList<FolderDto> dto)
    {
        var request = new AddFoldersWithAnnotations(dto);

        GenericCudOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<GenericCudOperationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to insert a counter group
    /// </summary>
    /// <param name="counterGroupDto">
    ///     <see cref="CounterGroupDto" />
    /// </param>
    /// <returns>The counter group stored or some meta information if not found or there was some exception.</returns>
    [HttpPost("CounterGroups")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<CounterGroupDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<CounterGroupDto>>> InsertCounterGroup([FromBody] CounterGroupDto counterGroupDto)
    {
        var request = new UpsertCounterGroup(counterGroupDto);

        CounterGroupDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<CounterGroupDto>(result));
    }

    /// <summary>
    /// Can be used to import a generic vpa file
    /// </summary>
    /// <param name="vpaFileForm">
    ///     <see cref="VpaFileForm" />
    /// </param>
    /// <returns>
    /// The annotation stored with this import request or some meta information if not found or there was some
    /// exception.
    /// </returns>
    [HttpPost("import-vpa-file")]
    [Authorize(Policy = AnnotationPolicy.ManageImport)]
    [ProducesResponseType(typeof(ApiListResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiListResponse<AnnotationDto>>> ImportVpaFile([FromForm] VpaFileForm vpaFileForm)
    {
        _logger.LogDebug($"The vpa file is: {ReadAsString(vpaFileForm.VpaFile)}");
        var request = new ImportVpa(vpaFileForm.SlideImageId, vpaFileForm.VpaFile, vpaFileForm.VpaFile.FileName,
            vpaFileForm.ReloadAllAnnotations);

        IReadOnlyList<AnnotationDto> data = await Mediator.Send(request);

        return Ok(new ApiListResponse<AnnotationDto>(data));
    }

    #endregion

    #region Put Request

    /// <summary>
    /// Consumers can use this method to update an annotation for a specific slide image
    /// </summary>
    /// <param name="annotationDto">the annotation definition</param>
    /// <param name="slideImageId">the unique id of a slide image</param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    [HttpPut("SlideImages/{slideImageId:guid}")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationDto>>> UpdateAnnotation([FromBody] AnnotationDto annotationDto, Guid slideImageId)
    {
        var request = new UpsertAnnotation(slideImageId, annotationDto);

        AnnotationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to update a counter group
    /// </summary>
    /// <param name="counterGroupDto">
    ///     <see cref="CounterGroupDto" />
    /// </param>
    /// <returns>The counter group stored or some meta information if not found or there was some exception.</returns>
    [HttpPut("CounterGroups")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<CounterGroupDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<CounterGroupDto>>> UpdateCounterGroup([FromBody] CounterGroupDto counterGroupDto)
    {
        var request = new UpsertCounterGroup(counterGroupDto);

        CounterGroupDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<CounterGroupDto>(result));
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
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    [HttpPut("{annotationId:guid}/insert-vertices")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationDto>>> InsertVertices(Guid annotationId, [FromBody] VertexUpdateDto vertexUpdateDto)
    {
        var request = new AddAnnotationCoordinates(annotationId, vertexUpdateDto);

        AnnotationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to set coordinates of a specified vertex.
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <param name="vertexUpdateDto">
    /// during the set only one vertex can be updated. <see cref="VertexUpdateDto" />
    /// </param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    [HttpPut("{annotationId:guid}/set-vertex-position")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationDto>>> SetVertexPosition(Guid annotationId, [FromBody] VertexUpdateDto vertexUpdateDto)
    {
        var request = new UpdateAnnotationCoordinate(annotationId, vertexUpdateDto);

        AnnotationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to insert a list of counters into a specific counter group
    /// </summary>
    /// <param name="counterGroupDto">
    ///     <see cref="CounterGroupDto" />
    /// </param>
    /// <returns>
    ///     <see cref="GenericCudOperationDto" />
    /// </returns>
    [HttpPut("CounterGroups/insert-counters")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<GenericCudOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<GenericCudOperationDto>>> InsertCounters([FromBody] CounterGroupDto counterGroupDto)
    {
        var request = new AddAnnotationCounters(counterGroupDto);

        GenericCudOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<GenericCudOperationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to update a specific counter of a single counter group
    /// </summary>
    /// <param name="counterId">the unique counter id</param>
    /// <param name="x">the x coordinate</param>
    /// <param name="y">the y coordinate</param>
    /// <returns>
    ///     <see cref="GenericCudOperationDto" />
    /// </returns>
    [HttpPut("CounterGroups/Counters/{counterId:guid}/set-x/{x:double}/set-y/{y:double}")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<GenericCudOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<GenericCudOperationDto>>> SetCounter(Guid counterId, double x, double y)
    {
        var request = new UpdateAnnotationCounter(counterId, x, y);

        GenericCudOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<GenericCudOperationDto>(result));
    }

    /// <summary>
    /// Consumers can use this to set the radius of a Circle annotation. we assume that the radius is calculated on X axis.
    /// f.e. the center is (0,0) and the point on the circumference is (5,0) in this case the radius is also 5.
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <param name="radius">the new radius value</param>
    /// <returns>
    ///     <see cref="AnnotationDto" />
    /// </returns>
    [HttpPut("{annotationId:guid}/set-radius/{radius:double}")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationDto>>> SetRadius(Guid annotationId, double radius)
    {
        var request = new SetRadius(annotationId, radius);

        AnnotationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use this to set the color of a specific annotation. the api as no power into the meanings of these
    /// numbers. we act only like a storage.
    /// </summary>
    /// <param name="annotationDto">
    ///     <see cref="AnnotationDto" />
    /// </param>
    /// <returns>
    ///     <see cref="AnnotationDto" />
    /// </returns>
    [HttpPut("set-color")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationDto>>> SetColor([FromBody] AnnotationDto annotationDto)
    {
        var request = new SetColor(annotationDto);

        AnnotationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use this to translate a list of annotations with all related entities (eg counters)
    /// </summary>
    /// <param name="translateDto">
    ///     <see cref="TranslateDto" />
    /// </param>
    /// <returns>
    ///     <see cref="AnnotationDto" />
    /// </returns>
    [HttpPut("set-translation")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiListResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiListResponse<AnnotationDto>>> SetTranslation([FromBody] TranslateDto translateDto)
    {
        var request = new Translate(translateDto);

        IReadOnlyList<AnnotationDto> result = await Mediator.Send(request);

        return Ok(new ApiListResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to massively update all annotations visibility for a specific slide image.
    /// Only annotation where the user has write permission will be modified.
    /// </summary>
    /// <param name="dto">
    ///     <see cref="SetAnnotationsVisibilityDto" />
    /// </param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    [HttpPut("set-annotations-visibility")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<GenericCudOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<GenericCudOperationDto>>> SetAnnotationsVisibility([FromBody] SetAnnotationsVisibilityDto dto)
    {
        var request = new SetAnnotationsVisibility(dto);

        GenericCudOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<GenericCudOperationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to set the global annotation permission. <see cref="AnnotationPermission" />
    /// </summary>
    /// <param name="dto">The new global annotation permission to be store</param>
    /// <returns>
    /// The stored global annotation permission.
    /// </returns>
    [HttpPut("set-annotation-permissions")]
    [Authorize(Policy = AnnotationPolicy.ManageAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationPermissionsDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationPermissionsDto>>> SetAnnotationPermissions([FromBody] AnnotationPermissionsDto dto)
    {
        var request = new SetAnnotationPermissions(dto);

        AnnotationPermissionsDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationPermissionsDto>(result));
    }

    #endregion

    #region Delete Request

    /// <summary>
    /// Consumers can use this method to delete a specified vertex.
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <param name="index">The index of the vertex we want to delete.</param>
    /// <returns>The annotation stored or some meta information if not found or there was some exception.</returns>
    [HttpDelete("{annotationId:guid}/delete-vertex/{index:int}")]
    [Authorize(Policy = AnnotationPolicy.DeleteAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<AnnotationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<AnnotationDto>>> DeleteVertex(Guid annotationId, int index)
    {
        var request = new DeleteAnnotationCoordinate(annotationId, index);

        AnnotationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<AnnotationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to delete a specified annotation.
    /// </summary>
    /// <param name="annotationId">the annotation unique identifier</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    [HttpDelete("{annotationId:guid}")]
    [Authorize(Policy = AnnotationPolicy.DeleteAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<DeleteOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<DeleteOperationDto>>> DeleteAnnotation(Guid annotationId)
    {
        var request = new DeleteAnnotation(annotationId);

        DeleteOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<DeleteOperationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to delete a specified folder with all subfolders and related data.
    /// </summary>
    /// <param name="folderId">the folder unique identifier</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    [HttpDelete("Folders/{folderId:guid}")]
    [Authorize(Policy = AnnotationPolicy.DeleteFolders)]
    [ProducesResponseType(typeof(ApiResponse<DeleteOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<DeleteOperationDto>>> DeleteFolder(Guid folderId)
    {
        var request = new DeleteFolder(folderId);

        DeleteOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<DeleteOperationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to delete all annotation of a slide image.
    /// </summary>
    /// <param name="slideImageId">the annotation unique identifier</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    [HttpDelete("SlideImages/{slideImageId:guid}")]
    [Authorize(Policy = AnnotationPolicy.DeleteAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<DeleteOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<DeleteOperationDto>>> DeleteAnnotations(Guid slideImageId)
    {
        var request = new DeleteAnnotations(slideImageId);

        DeleteOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<DeleteOperationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to delete a counter group.
    /// </summary>
    /// <param name="counterGroupId">the counter group unique identifier</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    [HttpDelete("CounterGroups/{counterGroupId:guid}")]
    [Authorize(Policy = AnnotationPolicy.DeleteAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<DeleteOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<DeleteOperationDto>>> DeleteCounterGroup(Guid counterGroupId)
    {
        var request = new DeleteCounterGroup(counterGroupId);

        DeleteOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<DeleteOperationDto>(result));
    }

    /// <summary>
    /// Consumers can use this method to delete a single counter.
    /// </summary>
    /// <param name="counterId">the counter unique identifier</param>
    /// <returns>
    ///     <see cref="DeleteOperationDto" />
    /// </returns>
    [HttpDelete("CounterGroups/Counters/{counterId:guid}")]
    [Authorize(Policy = AnnotationPolicy.DeleteAnnotations)]
    [ProducesResponseType(typeof(ApiResponse<DeleteOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<DeleteOperationDto>>> DeleteCounter(Guid counterId)
    {
        var request = new DeleteCounter(counterId);

        DeleteOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<DeleteOperationDto>(result));
    }

    #endregion
}