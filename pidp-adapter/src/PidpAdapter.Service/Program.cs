namespace PidpAdapter;

using PidpAdapter.Infrastructure.Auth;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

public class Program
{

    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args)
            .Build()
            .Run();
    }


    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        return WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddUserSecrets(Assembly.GetExecutingAssembly());
                config.AddEnvironmentVariables();
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddDebug();
                logging.AddEventSourceLogger();
            })
            .UseSerilog()
            .UseOpenShiftIntegration(_ => _.CertificateMountPoint = "/var/run/secrets/service-cert")
            .UseStartup<Startup>()
            .UseKestrel(options =>
            {
                options.AllowSynchronousIO = true;
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
                options.Limits.MaxRequestBodySize = 512 * 1024 * 1024; // allow large transfers
                                                                       // for macOS local dev but don't have env
                                                                       // options.ListenLocalhost(5001, o => {
                                                                       //     o.Protocols = HttpProtocols.Http2;
                                                                       // });
            });
    }
}
 
