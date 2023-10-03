using MediatR;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using System;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Application.Events;

public class SlideImageUpdated : INotification
{
    public SlideImageUpdated(SlideImageDto dto)
    {
        Dto = dto;
    }

    public SlideImageDto Dto { get; }
}

public class SlideImageUpdatedHandler : NotificationHandler<SlideImageUpdated>
{
    private readonly IDbContext _annotationDbContext;
    private readonly ILogger<SlideImageUpdatedHandler> _logger;

    public SlideImageUpdatedHandler(ILogger<SlideImageUpdatedHandler> logger, IDbContext annotationDbContext)
    {
        _logger = logger;
        _annotationDbContext = annotationDbContext;
    }

    protected override void Handle(SlideImageUpdated notification)
    {
        try
        {
            if (notification.Dto.CreatedAt != null)
            {
                SlideImage slideImage = _annotationDbContext.Set<SlideImage>()
                    .FirstOrDefault(e => e.SlideImageId == notification.Dto.Id);

                if (slideImage != null)
                {
                    slideImage.Update(notification.Dto.OwnedBy);
                    _annotationDbContext.Set<SlideImage>().Update(slideImage);
                    _annotationDbContext.SaveChanges();

                    _logger.LogInformation(
                        "Slide image '{SlideImageFileName}' with id '{SlideImageId}' updated after event was triggered.",
                        notification.Dto.FileName, notification.Dto.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "Can't update slide image '{SlideImageFileName}' with id '{SlideImageId}' cause wasn't found on db.",
                        notification.Dto.FileName, notification.Dto.Id);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Can't update slide image '{SlideImageFileName}' with id '{SlideImageId}' as an upload is still running.",
                    notification.Dto.FileName, notification.Dto.Id);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "Slide image '{SlideImageFileName}' with id '{SlideImageId}' couldn't get updated.",
                notification.Dto.FileName, notification.Dto.Id);
        }
    }
}