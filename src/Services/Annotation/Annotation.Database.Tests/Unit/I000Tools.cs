using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Database.Tests.Config;
using PreciPoint.Ims.Services.Annotation.Database.Tests.Mocks;
using PreciPoint.Ims.Utils.TestUtils.Config;
using System;
using System.Collections.Generic;

namespace PreciPoint.Ims.Services.Annotation.Database.Tests.Unit;

internal class I000Tools
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
        services.AddSingleton(GetAppConfig());
        services.AddLogging(o => o.AddDebug());

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

    //[Test]
    public void I000_001CreateDb()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

        //dbContext.GetDatabase().EnsureDeleted();
        //dbContext.GetDatabase().EnsureCreated();
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