using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PreciPoint.Ims.Services.Annotation.Database;

public class ApplyMigrations : IHostedService
{
    private readonly ILogger<ApplyMigrations> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ApplyMigrations(IServiceScopeFactory serviceScopeFactory, ILogger<ApplyMigrations> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
            var dbContext = (AnnotationDbContext) serviceScope.ServiceProvider.GetRequiredService<IDbContext>();

            _logger.LogInformation("Check migrations to apply.");

            IReadOnlyList<string> pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pendingMigrations.Count == 0)
            {
                _logger.LogInformation("No database migrations to apply.");
                return;
            }

            var migrator = dbContext.GetInfrastructure().GetService<IMigrator>();
            if (migrator == null)
            {
                throw new NullReferenceException("We need a migrator otherwise no migrations can get applied.");
            }

            foreach (string pendingMigration in pendingMigrations)
            {
                _logger.LogInformation("We will try to apply migration: '{@PendingMigration}'", pendingMigration);
                await migrator.MigrateAsync(pendingMigration, cancellationToken);
            }

            await using var conn = (NpgsqlConnection) dbContext.Database.GetDbConnection();
            await conn.OpenAsync(cancellationToken);
            conn.ReloadTypes();

            dbContext.SaveChanges();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "ApplyMigrations failed.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service stopped.");
        return Task.CompletedTask;
    }
}