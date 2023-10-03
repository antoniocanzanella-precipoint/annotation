using Microsoft.AspNetCore.SignalR;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects.HubInterfaces;

namespace PreciPoint.Ims.Services.Annotation.API.Hubs;

/// <summary>
/// SignalR annotation Hub
/// </summary>
public class AnnotationHub : Hub<IAnnotationSignalRClient> { }