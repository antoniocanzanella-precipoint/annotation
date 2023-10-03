using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;

namespace PreciPoint.Ims.Services.Annotation.API.Controllers;

/// <summary>
/// Define the root property of generic controller
/// </summary>
public class AControllerBase : ControllerBase
{
    /// <summary>
    /// the mediator implementation from IoC container
    /// </summary>
    protected readonly IMediator Mediator;

    /// <summary>
    /// Init
    /// </summary>
    /// <param name="mediator"></param>
    public AControllerBase(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Utility for debug
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    protected string ReadAsString(IFormFile file)
    {
        file.OpenReadStream();
        var result = new StringBuilder();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            while (reader.Peek() >= 0)
            {
                result.AppendLine(reader.ReadLine());
            }
        }

        return result.ToString();
    }
}