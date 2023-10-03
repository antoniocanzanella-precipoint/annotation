using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PreciPoint.Ims.Core.Extensions.Args;
using Serilog;
using System.IO;

namespace PreciPoint.Ims.Services.Annotation.API;

/// <summary>
/// Primary entry point class for web host application.
/// </summary>
public class Program
{
    /// <summary>
    /// Classical main entry point used to build a web host application.
    /// </summary>
    /// <param name="args">Could be used to further specify application arguments.</param>
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services))
            .ConfigureAppConfiguration((_, config) => config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(args.AppSettingsOverride(), true, true))
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}