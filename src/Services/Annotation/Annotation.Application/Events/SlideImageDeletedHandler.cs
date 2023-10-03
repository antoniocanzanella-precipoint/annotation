using MediatR;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using System;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Application.Events;

public class SlideImageDeleted : INotification
{
    public SlideImageDeleted(SlideImageDto dto)
    {
        Dto = dto;
    }

    public SlideImageDto Dto { get; }
}

public class SlideImageDeletedHandler : NotificationHandler<SlideImageDeleted>
{
    private readonly IDbContext _annotationDbContext;
    private readonly ApplicationConfig _appConfig;
    private readonly ILogger<SlideImageDeletedHandler> _logger;

    public SlideImageDeletedHandler(ILogger<SlideImageDeletedHandler> logger, ApplicationConfig appConfig,
        IDbContext annotationDbContext)
    {
        _logger = logger;
        _appConfig = appConfig;
        _annotationDbContext = annotationDbContext;
    }

    protected override void Handle(SlideImageDeleted notification)
    {
        try
        {
            if (notification.Dto.CreatedAt != null)
            {
                SlideImage slideImage = _annotationDbContext.Set<SlideImage>()
                    .FirstOrDefault(e => e.SlideImageId == notification.Dto.Id);

                if (slideImage != null)
                {
                    _annotationDbContext.Set<SlideImage>().Remove(slideImage);
                    _annotationDbContext.SaveChanges();

                    _logger.LogInformation(
                        "Slide image '{SlideImageFileName}' with id '{SlideImageId}' deleted after event was triggered.",
                        notification.Dto.FileName, notification.Dto.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "Can't delete slide image '{SlideImageFileName}' with id '{SlideImageId}' cause wasn't found on db.",
                        notification.Dto.FileName, notification.Dto.Id);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Can't delete slide image '{SlideImageFileName}' with id '{SlideImageId}' as an upload is still running.",
                    notification.Dto.FileName, notification.Dto.Id);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "Slide image '{SlideImageFileName}' with id '{SlideImageId}' couldn't get deleted.",
                notification.Dto.FileName, notification.Dto.Id);
        }
    }
}