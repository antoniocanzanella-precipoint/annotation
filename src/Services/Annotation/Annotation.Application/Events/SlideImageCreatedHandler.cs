using MediatR;
using Microsoft.Extensions.Logging;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Services.ImageManagement.DataTransferObjects.SlideImages;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Events;

public class SlideImageCreated : INotification
{
    public SlideImageCreated(SlideImageDto slideImage)
    {
        Dto = slideImage;
    }

    public SlideImageDto Dto { get; }
}

public class SlideImageCreatedHandler : NotificationHandler<SlideImageCreated>
{
    private readonly IDbContext _annotationDbContext;
    private readonly ILogger<SlideImageCreatedHandler> _logger;

    public SlideImageCreatedHandler(IDbContext annotationDbContext, ILogger<SlideImageCreatedHandler> logger)
    {
        _annotationDbContext = annotationDbContext;
        _logger = logger;
    }

    protected override void Handle(SlideImageCreated notification)
    {
        try
        {
            var slideImage = new SlideImage(notification.Dto.Id, notification.Dto.OwnedBy);

            _annotationDbContext.Set<SlideImage>().Add(slideImage);
            _annotationDbContext.SaveChanges();

            _logger.LogInformation(
                "Slide image '{SlideImageFileName}' with id '{SlideImageId}' created after event was triggered.",
                notification.Dto.FileName, notification.Dto.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "Slide image '{SlideImageFileName}' with id '{SlideImageId}' couldn't get created.",
                notification.Dto.FileName, notification.Dto.Id);
        }
    }
}