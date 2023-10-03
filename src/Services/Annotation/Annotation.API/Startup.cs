using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using PreciPoint.Ims.Core.Authorization.Config;
using PreciPoint.Ims.Core.Authorization.Enums;
using PreciPoint.Ims.Core.Authorization.Requirements;
using PreciPoint.Ims.Core.Converters;
using PreciPoint.Ims.Core.DataTransferObjects.Responses;
using PreciPoint.Ims.Core.HealthCheck.Extensions;
using PreciPoint.Ims.Core.JwtBearer.Keycloak.Extensions;
using PreciPoint.Ims.Core.Middlewares.Exceptions;
using PreciPoint.Ims.Core.OpenApi.Extensions;
using PreciPoint.Ims.Core.OpenApi.SchemaFilters;
using PreciPoint.Ims.Services.Annotation.API.Hubs;
using PreciPoint.Ims.Services.Annotation.Application;
using PreciPoint.Ims.Services.Annotation.Application.Authorization;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Database;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Infrastructure;
using PreciPoint.Ims.Services.Annotation.Infrastructure.Messaging;
using PreciPoint.Ims.Services.Annotation.MessagePack.Resolver;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PreciPoint.Ims.Services.Annotation.API;

/// <summary>
/// The configuration class of the application
/// </summary>
public class Startup
{
    private readonly string _apiDocumentName;
    private readonly string _apiVersion;
    private readonly IConfiguration _configuration;
    private readonly int _httpPort;
    private readonly string _liveHealthCheck;
    private readonly string _openApiTitle;
    private readonly string _readyHealthCheck;
    private readonly string _startupConditionsHealthCheck;

    /// <summary>
    /// Responsible for service configuration and library usage.
    /// </summary>
    /// <param name="configuration">Central configuration object used by different injected classes.</param>
    public Startup(IConfiguration configuration)
    {
        _openApiTitle = "PreciPoint IMS Annotation API";
        _apiVersion = "v1";
        _apiDocumentName = "openapi";
        _configuration = configuration;
        _liveHealthCheck = "/health/live";
        _readyHealthCheck = "/health/ready";
        _startupConditionsHealthCheck = "startup_condition";
        _httpPort = HealthCheckExtensions.GetManagementPort();
    }

    /// <summary>
    /// Configure service
    /// </summary>
    /// <param name="services">the service collection</param>
    public void ConfigureServices(IServiceCollection services)
    {
        var applicationConfig = _configuration.Get<ApplicationConfig>();

        services.Configure<ApplicationConfig>(_configuration);
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<ApplicationConfig>>().Value);
        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<ApplicationConfig>().OAuth2);
        PostgreSqlConfig postgreSqlConfig = _configuration.Get<ApplicationConfig>().Databases.PostgreSql;

        services.AddKeycloakFromHeaderOrQuery(_configuration.Get<ApplicationConfig>().OAuth2,
            ClaimsPrincipalStrategy.Uncached);

        services.AddSignalR(hubOptions =>
            {
                hubOptions.MaximumReceiveMessageSize =
                    _configuration.Get<ApplicationConfig>().MaximumReceiveMessageSizeInByte;
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.PayloadSerializerOptions.Converters.Add(new Iso8601DateTimeConverter());
            });

        services.AddResponseCompression(opts =>
        {
            opts.EnableForHttps = true;
            opts.Providers.Clear();
            opts.Providers.Add<GzipCompressionProvider>();
        });

        services.AddApplication(applicationConfig.LocalizationConfig);
        services.AddDatabase(postgreSqlConfig.ParametersToConnectionString());
        services.AddInfrastructure();

        services.AddAutoMapper(typeof(Startup).GetTypeInfo().Assembly);

        services
            .AddMvcCore()
            .AddMvcOptions(x =>
            {
                IFormatterResolver resolver = CompositeResolver.Create(
                    AnnotationDtoFormatterResolver.Instance,
                    ContractlessStandardResolver.Instance
                );
                MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
                var formatter = new MessagePackOutputFormatter(options);
                x.OutputFormatters.Add(formatter);
            })
            .AddApiExplorer()
            .AddCors()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new Iso8601DateTimeConverter());
                options.JsonSerializerOptions.Converters.Add(new Iso8601NullableDateTimeConverter());
            });

        AddAuthorization(services);

        services.AddHealthChecks()
            .AddDbContextCheck<AnnotationDbContext>(
                ConfigureHealthCheckName(nameof(AnnotationDbContext)))
            .AddCheck<SlideImageSubscriber>(ConfigureHealthCheckName(nameof(SlideImageSubscriber)));

        services.AddSwaggerGen(configure =>
        {
            OAuth2Config oAuth2Config = _configuration.Get<ApplicationConfig>().OAuth2;
            configure.SwaggerDoc(_apiVersion, new OpenApiInfo
            {
                Title = _openApiTitle,
                Version = _apiVersion,
                Contact = new OpenApiContact { Name = "PreciPoint GmbH", Email = "johannes.mueller@precipoint.de" },
                Description = "Web API to handle microscope slide images."
            });

            new List<string>
            {
                typeof(Startup).GetTypeInfo().Assembly.GetName().Name,
                typeof(MetaResponse).GetTypeInfo().Assembly.GetName().Name,
                typeof(AnnotationDto).GetTypeInfo().Assembly.GetName().Name
            }.ForEach(documentationFile =>
                configure.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{documentationFile}.xml")));

            const string securitySchemeId = "oauth2";
            configure.AddSecurityDefinition(securitySchemeId,
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            Scopes = new Dictionary<string, string> { { "swagger", "Interact via the swagger browser UI." } },
                            AuthorizationUrl = new Uri(oAuth2Config.AuthorizationUrl, UriKind.Absolute),
                            TokenUrl = new Uri(oAuth2Config.TokenUrl, UriKind.Absolute)
                        }
                    }
                });
            configure.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = securitySchemeId } },
                    Enumerable.Empty<string>().ToList()
                }
            });
            // Use method name as operation id to make open api typescript generator happy.
            configure.CustomOperationIds(apiDescription =>
                apiDescription.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null);
            configure.SchemaFilter<HttpStatusCodeSchemaFilter>();
        });

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("de-DE"), new CultureInfo("it-IT") };
            options.DefaultRequestCulture = new RequestCulture("en-US", "en-US");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.ApplyCurrentCultureToResponseHeaders = true;
        });
    }

    /// <summary>
    /// configure application
    /// </summary>
    /// <param name="app">the application instance</param>
    /// <param name="logger">the logger instance</param>
    public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
    {
        var applicationConfig = app.ApplicationServices.GetRequiredService<ApplicationConfig>();

        var options = new JsonSerializerOptions { WriteIndented = true };
        logger.LogInformation(JsonSerializer.Serialize(applicationConfig, options));


        app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value);
        const string annotationJson = "annotations.json";
        app.UseRouting();
        app.UseSwagger(configure =>
        {
            configure.RouteTemplate = _apiDocumentName + "/{documentName}/" + annotationJson;
            configure.PreSerializeFilters.Add((swaggerOptions, httpRequest) =>
            {
                OpenApiServer openApiServer = httpRequest.ExtractOpenApiServer();
                swaggerOptions.Servers = new List<OpenApiServer> { openApiServer };
            });
        });
        app.UseSwaggerUI(configure =>
        {
            configure.RoutePrefix = _apiDocumentName;
            configure.SwaggerEndpoint($"/{_apiDocumentName}/{_apiVersion}/{annotationJson}", _openApiTitle);
            configure.OAuthClientId(applicationConfig.OAuth2.Swagger.ClientId);
            configure.OAuthClientSecret(applicationConfig.OAuth2.Swagger.ClientSecret);
            configure.OAuthRealm(applicationConfig.OAuth2.Realm);
            configure.OAuthAppName(applicationConfig.OAuth2.Jwt.ClientId);
            configure.OAuthUseBasicAuthenticationWithAccessCodeGrant();
        });

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseCors(corsPolicyBuilder => corsPolicyBuilder
            .WithOrigins(applicationConfig.DomainsForCors.ToArray())
            .AllowCredentials()
            .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete)
            .WithHeaders("X-Requested-With", "X-SignalR-User-Agent", HeaderNames.ContentType, HeaderNames.Accept,
                HeaderNames.Authorization)
            .WithExposedHeaders("X-SignalR-User-Agent")
            .SetIsOriginAllowedToAllowWildcardSubdomains());

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseResponseCompression();
        app.UseStaticFiles();

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<AnnotationHub>("/signalR/annotation");

                endpoints
                    .MapHealthChecks(_liveHealthCheck, new HealthCheckOptions { Predicate = _ => false })
                    .RequireHost($"*:{_httpPort}");

                endpoints
                    .MapHealthChecks(_readyHealthCheck, new HealthCheckOptions
                    {
                        Predicate = healthCheck =>
                            healthCheck.Name.EndsWith(_startupConditionsHealthCheck)
                    })
                    .RequireHost($"*:{_httpPort}");
            }
        );
    }

    private void AddAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AnnotationPolicy.ViewAnnotations,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.QueryAnnotations)); });

            options.AddPolicy(AnnotationPolicy.ViewFolders,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.QueryFolders)); });

            options.AddPolicy(AnnotationPolicy.ManageAnnotations,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.ManageAnnotations)); });

            options.AddPolicy(AnnotationPolicy.ManageForeignAnnotations,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.ManageForeignAnnotations)); });

            options.AddPolicy(AnnotationPolicy.ManageAnnotationsByFolders,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.ManageAnnotationsByFolders)); });

            options.AddPolicy(AnnotationPolicy.DeleteAnnotations,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.DeleteAnnotations)); });

            options.AddPolicy(AnnotationPolicy.DeleteForeignAnnotations,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.DeleteForeignAnnotations)); });

            options.AddPolicy(AnnotationPolicy.DeleteFolders,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.DeleteFolders)); });

            options.AddPolicy(AnnotationPolicy.ManageImport,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.ManageImport)); });

            options.AddPolicy(AnnotationPolicy.AdminSynchronization,
                policy => { policy.Requirements.Add(new ResourceAccessRequirement(Roles.AdminSynchronization)); });
        });
    }

    private string ConfigureHealthCheckName(string prefix)
    {
        return $"{prefix}_{_startupConditionsHealthCheck}";
    }
}