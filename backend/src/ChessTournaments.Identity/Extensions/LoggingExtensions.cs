using Serilog;

namespace ChessTournaments.Identity.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder AddSerilogConfiguration(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog(
            (context, loggerConfig) =>
            {
                loggerConfig
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override(
                        "Microsoft.AspNetCore",
                        Serilog.Events.LogEventLevel.Warning
                    )
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                    );
            }
        );

        return hostBuilder;
    }
}
