using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NUnit.Framework;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Core.DataTransferObjects.Exceptions;
using PreciPoint.Ims.Services.Annotation.Application;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Application.Queries;
using PreciPoint.Ims.Services.Annotation.Database.Tests.Config;
using PreciPoint.Ims.Services.Annotation.Database.Tests.Mocks;
using PreciPoint.Ims.Services.Annotation.Localization;
using PreciPoint.Ims.Utils.TestUtils.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace PreciPoint.Ims.Services.Annotation.Database.Tests.Unit;

internal class I001Localization
{
    private DatabaseTestConfig _config;
    private IServiceProvider _serviceProvider;

    [OneTimeSetUp]
    public void Setup()
    {
        _config = new JsonSettings().Configuration.Get<DatabaseTestConfig>();

        var services = new ServiceCollection();

        services.AddApplication(GetAppConfig().LocalizationConfig);
        services.AddDatabase(_config.ConnectionString);
        services.AddSingleton<IClaimsPrincipalProvider, ClaimsPrincipalProviderMock>();
        services.AddSingleton(new ApplicationConfig());
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider(true);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        switch (_serviceProvider)
        {
            case null:
                return;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

    [Test]
    [Order(1)]
    public void I001_001VerifyMessage()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var invalidGuid = Guid.NewGuid();
        Assert.ThrowsAsync<ApiException>(() => mediator.Send(new GetAnnotationById(invalidGuid)));
    }

    [Test]
    [Order(2)]
    public void I001_002VerifyGermanMessage()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var ci = new CultureInfo("de-DE");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        var invalidGuid = Guid.NewGuid();
        var ex = Assert.ThrowsAsync<ApiException>(() => mediator.Send(new GetAnnotationById(invalidGuid)));
        Assert.True(ex.Message.Contains($"mit der ID {invalidGuid.ToString()}"));
    }

    [Test]
    [Order(3)]
    public void I001_003VerifyItalianMessage()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        var handler = new GetAnnotationByIdHandler(
            scope.ServiceProvider.GetRequiredService<IAnnotationQueries>(),
            scope.ServiceProvider.GetRequiredService<IStringLocalizer<Translations>>(),
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IDbContext>(),
            scope.ServiceProvider.GetRequiredService<IClaimsPrincipalProvider>());

        var ci = new CultureInfo("it-IT");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        var invalidGuid = Guid.NewGuid();
        var ex = Assert.Throws<ApiException>(() =>
            handler.Handle(new GetAnnotationById(invalidGuid), CancellationToken.None).GetAwaiter().GetResult());
        Assert.True(ex.Message.Contains($"con ID {invalidGuid.ToString()}"));

        ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        invalidGuid = Guid.NewGuid();
        ex = Assert.Throws<ApiException>(() =>
            handler.Handle(new GetAnnotationById(invalidGuid), CancellationToken.None).GetAwaiter().GetResult());
        Assert.True(ex.Message.Contains($"with ID {invalidGuid.ToString()}"));
    }

    [Test]
    [Order(4)]
    public void I001_004VerifyDefaultMessage()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        var invalidGuid = Guid.NewGuid();
        var ex = Assert.ThrowsAsync<ApiException>(() => mediator.Send(new GetAnnotationById(invalidGuid)));
        Assert.True(ex.Message.Contains($"with ID {invalidGuid.ToString()}"));
    }

    private ApplicationConfig GetAppConfig()
    {
        return new ApplicationConfig
        {
            PerformanceBehaviour = new PerformanceBehaviour { LongRunningTriggerMilliseconds = 1000 },
            LocalizationConfig = new LocalizationConfig
            {
                SupportedCultures = new List<string>
                {
                    "en",
                    "de",
                    "it"
                }
            }
        };
    }
}