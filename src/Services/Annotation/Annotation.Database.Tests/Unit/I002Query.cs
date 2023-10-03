using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PreciPoint.Ims.Core.Authorization.Providers;
using PreciPoint.Ims.Services.Annotation.Application;
using PreciPoint.Ims.Services.Annotation.Application.Command;
using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Application.Queries;
using PreciPoint.Ims.Services.Annotation.Database.Tests.Config;
using PreciPoint.Ims.Services.Annotation.Database.Tests.Mocks;
using PreciPoint.Ims.Services.Annotation.DataTransferObjects;
using PreciPoint.Ims.Services.Annotation.Domain.Model;
using PreciPoint.Ims.Utils.TestUtils.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Database.Tests.Unit;

internal class I002Query
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

    [Test]
    public void I002_001VerifyQuery()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var dto = new TranslateDto
        {
            AnnotationIds = new[] { new Guid("c41c195f-1948-4e2e-b807-e8651d4ba4e7") },
            DeltaX = 10,
            DeltaY = 10
        };
        var request = new Translate(dto);

        Assert.DoesNotThrowAsync(() => mediator.Send(request));
    }

    [Test]
    public void I002_002VerifyQueryCTE()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

        Guid[] ids = Enumerable.Range(0, 10).Select(x => Guid.NewGuid()).ToArray();

        var folders = new Folder[]
        {
            new()
            {
                Id = ids[0],
                Name = "FolderName 0",
                DisplayOder = 0
            },
            new()
            {
                Id = ids[1],
                Name = "FolderName liv 1",
                ParentFolderId = ids[0],
                DisplayOder = 0
            },
            new()
            {
                Id = ids[2],
                Name = "FolderName liv 1",
                ParentFolderId = ids[0],
                DisplayOder = 0
            },
            new()
            {
                Id = ids[3],
                Name = "FolderName liv 2",
                ParentFolderId = ids[2],
                DisplayOder = 0
            },
            new()
            {
                Id = ids[4],
                Name = "FolderName liv 2",
                ParentFolderId = ids[2],
                DisplayOder = 0
            },
            new()
            {
                Id = ids[5],
                Name = "FolderName liv 3",
                ParentFolderId = ids[4],
                DisplayOder = 0
            },
            new()
            {
                Id = ids[6],
                Name = "FolderName liv 3",
                ParentFolderId = ids[4],
                DisplayOder = 0
            }
        };

        dbContext.Set<Folder>().AddRange(folders);
        dbContext.SaveChanges();

        var queryResolver = scope.ServiceProvider.GetRequiredService<IRawQueryResolver>();

        List<Folder> foldersDb = dbContext.Set<Folder>().FromSqlRaw(queryResolver.GetFolderBelowQuery(), ids[0]).ToList();
        Assert.AreEqual(7, foldersDb.Count);

        foldersDb = dbContext.Set<Folder>().FromSqlRaw(queryResolver.GetFolderBelowQuery(), ids[1]).ToList();
        Assert.AreEqual(1, foldersDb.Count);

        foldersDb = dbContext.Set<Folder>().FromSqlRaw(queryResolver.GetFolderBelowQuery(), ids[2]).ToList();
        Assert.AreEqual(5, foldersDb.Count);

        dbContext.Set<Folder>().RemoveRange(dbContext.Set<Folder>()
            .FromSqlRaw(queryResolver.GetFolderBelowQuery(), ids[0]).ToList());
        dbContext.SaveChanges();

        foldersDb = dbContext.Set<Folder>().FromSqlRaw(queryResolver.GetFolderBelowQuery(), ids[0]).ToList();
    }

    //[Test]
    public async Task I002_003VerifyQuery_GetFolders()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var request = new GetFolders(new Guid("6c2738c3-5e3a-44e8-871b-3fc33a438898"));
        IReadOnlyList<FolderDto> result = await mediator.Send(request);

        Assert.AreEqual(1, result.Count);
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