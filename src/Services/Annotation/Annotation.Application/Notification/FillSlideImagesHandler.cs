using MediatR;
using PreciPoint.Ims.Services.Annotation.Application.Command;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Application.Notification;

public class FillSlideImages : INotification { }

public class FillSlideImagesHandler : INotificationHandler<FillSlideImages>
{
    private readonly IMediator _mediator;

    public FillSlideImagesHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(FillSlideImages notification, CancellationToken cancellationToken)
    {
        return _mediator.Send(new Synchronize(), cancellationToken);
    }
}