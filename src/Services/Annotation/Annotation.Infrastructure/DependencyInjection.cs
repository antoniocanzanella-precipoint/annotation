using Microsoft.Extensions.DependencyInjection;
using PreciPoint.Ims.Core.BackgroundProcessing.Services;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Infrastructure.Factories;
using PreciPoint.Ims.Services.Annotation.Infrastructure.Messaging;
using PreciPoint.Ims.Services.Annotation.Infrastructure.Services;

namespace PreciPoint.Ims.Services.Annotation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<SlideImageSubscriber>();
        services.AddHostedService<HostedServiceStarter<SlideImageSubscriber>>();
        services.AddSingleton<IHttpClientFactory, HttpIdentifiedClientFactory>();
        services.AddScoped<ISlideImageRepo, SlideImageRepo>();
        services.AddHostedService<ImageManagementConnector>();

        return services;
    }
}