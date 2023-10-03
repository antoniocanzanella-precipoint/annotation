using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Services.Annotation.Application.Interfaces;
using PreciPoint.Ims.Services.Annotation.Database.Queries;
using System.Reflection;

namespace PreciPoint.Ims.Services.Annotation.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AnnotationDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlDbContextOptionsBuilder =>
                {
                    npgsqlDbContextOptionsBuilder.UseNetTopologySuite();
                    npgsqlDbContextOptionsBuilder.MigrationsAssembly(typeof(AnnotationDbContext).GetTypeInfo()
                        .Assembly.GetName().Name);
                }
            );
        });

        services.AddScoped<IDbContext>(provider => provider.GetService<AnnotationDbContext>());
        services.AddScoped<IAnnotationQueries, AnnotationQueries>();
        services.AddScoped<ICountingQueries, CountingQueries>();
        services.AddSingleton(new GeometryFactory());
        services.AddSingleton<IRawQueryResolver, RawQueryResolver>();
        services.AddHostedService<ApplyMigrations>();

        return services;
    }
}