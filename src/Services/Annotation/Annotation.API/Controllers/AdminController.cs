using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Services.Annotation.Application.Authorization;
using PreciPoint.Ims.Services.Annotation.Application.Command;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using System.Net;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.API.Controllers;

/// <summary>
/// Provides HTTP ADMIN access to annotation.
/// </summary>
[Microsoft.AspNetCore.Components.Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AdminController : AControllerBase
{
    /// <summary>
    /// Create an instance
    /// </summary>
    /// <param name="mediator"></param>
    public AdminController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Consumers can use this to bootstrap the synchronization of tables
    /// </summary>
    /// <returns></returns>
    [HttpPut("synchronize")]
    [Authorize(Policy = AnnotationPolicy.AdminSynchronization)]
    [ProducesResponseType(typeof(ApiResponse<GenericCudOperationDto>), (int) HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResponse<GenericCudOperationDto>>> Synchronize()
    {
        var request = new Synchronize();

        GenericCudOperationDto result = await Mediator.Send(request);

        return Ok(new ApiResponse<GenericCudOperationDto>(result));
    }
}