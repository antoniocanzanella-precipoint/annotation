using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PreciPoint.Ims.Services.Annotation.Database.Extensions;
using System;
using System.Linq;

namespace PreciPoint.Ims.Services.Annotation.Database.Factories;

public class SlideImageDbContextFactory : IDesignTimeDbContextFactory<AnnotationDbContext>
{
    public AnnotationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AnnotationDbContext>();

        if (args.Contains("--skip-connection"))
        {
            optionsBuilder.UseNpgsql(npgsqlDbContextOptionsBuilder => { npgsqlDbContextOptionsBuilder.UseNetTopologySuite(); });
        }
        else
        {
            Console.WriteLine(
                @"We assume you want to connect to a database as '--skip-connection' was not provided as argument.");

            string postgreSqlConfig = Environment.GetEnvironmentVariables().ToPostgreSqlConfig()
                .ParametersToConnectionString();

            optionsBuilder.UseNpgsql(postgreSqlConfig,
                npgsqlDbContextOptionsBuilder => { npgsqlDbContextOptionsBuilder.UseNetTopologySuite(); });

            Console.WriteLine(postgreSqlConfig);
        }

        return new AnnotationDbContext(optionsBuilder.Options);
    }
}