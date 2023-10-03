using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Color;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.FillColor;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.LineColor;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Path;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Polygon;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Position;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Radius;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Text;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Attribute.Width;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.ByteSerializer;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.CompositeLayer.Marker;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Counter;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Path;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Polygon;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Scatterplot;
using PreciPoint.Ims.Services.Annotation.Application.DeckGl.Serialization.Layer.Annotation.SingleLayer.Text;
using PreciPoint.Ims.Services.Annotation.Application.Filter;
using PreciPoint.Ims.Services.Annotation.Application.Infrastructure;
using PreciPoint.Ims.Services.Annotation.Domain.DeckGl.Layer;
using PreciPoint.Ims.Services.Annotation.Enums.DeckGl;
using PreciPoint.Ims.Services.Annotation.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PreciPoint.Ims.Services.Annotation.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, LocalizationConfig localizationConfig)
    {
        services.AddSingleton<AnnotationQueryFilter>();

        services.Configure<RequestLocalizationOptions>(requestLocalizationOptions =>
        {
            CultureInfo[] cultureInfos = localizationConfig.SupportedCultures.Select(supportedCulture => new CultureInfo(supportedCulture)).ToArray();
            requestLocalizationOptions.DefaultRequestCulture = new RequestCulture(cultureInfos.First());
            requestLocalizationOptions.SupportedCultures = cultureInfos;
            requestLocalizationOptions.SupportedUICultures = cultureInfos;
            requestLocalizationOptions.ApplyCurrentCultureToResponseHeaders = true;
        });

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddAutoMapper(typeof(DependencyInjection).GetTypeInfo().Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
        services.TryAddTransient<IStringLocalizer>(serviceProvider => serviceProvider.GetRequiredService<IStringLocalizer<Translations>>());
        services.AddLocalization();

        services.AddAnnotationSerializers();
        return services;
    }

    public static void AddAnnotationSerializers(this IServiceCollection services)
    {
        services.AddSingleton<IByteSerializer, FixedPointerByteSerializer>();

        services.AddAttributeSerializersFromAssembly(typeof(IAttributeSerializer).Assembly);
        services.AddLayerSerializers();

        services.AddSingleton<IDeckGlAnnotationSerializer, DeckGlAnnotationSerializer>(x =>
        {
            var serializerOptions = new DeckGlAnnotationSerializerOptions
            {
                DefaultSerializer = new Dictionary<DeckGlLayerType, IDeckGlAnnotationSingleLayerSerializer>
                {
                    { DeckGlLayerType.Scatterplot, x.GetRequiredService<AnnotationScatterplotLayerSerializer>() },
                    { DeckGlLayerType.Path, x.GetServices<IAnnotationPathLayerSerializer>().First() as AnnotationPathLayerSerializer },
                    { DeckGlLayerType.Text, x.GetServices<IAnnotationTextLayerSerializer>().ToList()[1] as AnnotationTextLayerSerializer },
                    { DeckGlLayerType.Counter, x.GetRequiredService<AnnotationCounterLayerSerializer>() },
                    { DeckGlLayerType.Polygon, x.GetRequiredService<AnnotationPolygonLayerSerializer>() }
                },
                CompositeLayerSerializer = new Dictionary<string, IDeckGlAnnotationCompositeLayerSerializer>
                {
                    { DeckGlLayerId.AnnotationsMarkerLayer.ToString(), x.GetRequiredService<AnnotationMarkerLayerSerializer>() }
                }
            };
            return new DeckGlAnnotationSerializer(serializerOptions);
        });
    }

    private static IServiceCollection AddLayerSerializers(this IServiceCollection services)
    {
        // path
        services.AddSingleton<IAnnotationPathLayerSerializer>(x => new AnnotationPathLayerSerializer(
            x.GetRequiredService<IByteSerializer>(),
            x.GetRequiredService<AnnotationVertexLenColorAttributeSerializer>(),
            x.GetRequiredService<AnnotationWidthAttributeSerializer>(),
            x.GetRequiredService<AnnotationPathAttributeSerializer>()
        ));

        // marker path
        services.AddSingleton<IAnnotationPathLayerSerializer>(x => new AnnotationPathLayerSerializer(
            x.GetRequiredService<IByteSerializer>(),
            x.GetRequiredService<AnnotationMarkerPathColorAttributeSerializer>(),
            x.GetRequiredService<AnnotationWidthAttributeSerializer>(),
            x.GetRequiredService<AnnotationPathAttributeSerializer>()
        ));

        // polygon
        services.AddSingleton(x => new AnnotationPolygonLayerSerializer(
            x.GetRequiredService<IByteSerializer>(),
            x.GetRequiredService<AnnotationPolygonAttributeSerializer>(),
            x.GetRequiredService<AnnotationVertexLenColorAttributeSerializer>(),
            x.GetRequiredService<AnnotationWidthAttributeSerializer>()
        ));

        // marker text
        services.AddSingleton<IAnnotationTextLayerSerializer>(x => new AnnotationTextLayerSerializer(
            x.GetRequiredService<IByteSerializer>(),
            x.GetRequiredService<AnnotationTextAttributeSerializer>(),
            x.GetRequiredService<AnnotationMarkerTextPositionAttributeSerializer>(),
            x.GetRequiredService<AnnotationTextColorAttributeSerializer>()
        ));

        // regular text
        services.AddSingleton<IAnnotationTextLayerSerializer>(x => new AnnotationTextLayerSerializer(
            x.GetRequiredService<IByteSerializer>(),
            x.GetRequiredService<AnnotationTextAttributeSerializer>(),
            x.GetRequiredService<AnnotationTextPositionAttributeSerializer>(),
            x.GetRequiredService<AnnotationTextColorAttributeSerializer>()
        ));

        // scatterplot
        services.AddSingleton(x => new AnnotationScatterplotLayerSerializer(
            x.GetRequiredService<IByteSerializer>(),
            x.GetRequiredService<AnnotationScatterPositionAttributeSerializer>(),
            x.GetRequiredService<AnnotationRadiusAttributeSerializer>(),
            x.GetRequiredService<AnnotationFillColorAttributeSerializer>(),
            x.GetRequiredService<AnnotationLineColorAttributeSerializer>()
        ));

        // counter 
        services.AddSingleton(x => new AnnotationCounterLayerSerializer(
            x.GetRequiredService<IByteSerializer>(),
            x.GetRequiredService<AnnotationCounterPositionSerializer>(),
            x.GetRequiredService<AnnotationMarkerPathColorAttributeSerializer>()
        ));

        // marker
        services.AddSingleton(x => new AnnotationMarkerLayerSerializer(
            x.GetServices<IAnnotationTextLayerSerializer>().First(),
            x.GetServices<IAnnotationPathLayerSerializer>().ToList()[1]
        ));
        return services;
    }

    private static IServiceCollection AddAttributeSerializersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton<AnnotationMarkerPathColorAttributeSerializer>();
        services.AddSingleton<AnnotationTextColorAttributeSerializer>();
        services.AddSingleton<AnnotationFillColorAttributeSerializer>();
        services.AddSingleton<AnnotationLineColorAttributeSerializer>();
        services.AddSingleton<AnnotationMarkerTextPositionAttributeSerializer>();
        services.AddSingleton<AnnotationPathAttributeSerializer>();
        services.AddSingleton<AnnotationPolygonAttributeSerializer>();
        services.AddSingleton<AnnotationRadiusAttributeSerializer>();
        services.AddSingleton<AnnotationScatterPositionAttributeSerializer>();
        services.AddSingleton<AnnotationTextAttributeSerializer>();
        services.AddSingleton<AnnotationTextPositionAttributeSerializer>();
        services.AddSingleton<AnnotationWidthAttributeSerializer>();
        services.AddSingleton<AnnotationVertexLenColorAttributeSerializer>();
        services.AddSingleton<AnnotationCounterPositionSerializer>();
        return services;
    }
}