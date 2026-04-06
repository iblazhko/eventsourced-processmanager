using EventSourcedPM.Configuration;
using Serilog;
using Serilog.Events;

namespace EventSourcedPM.Configurators;

public static class SerilogConfigurator
{
    public static IServiceCollection AddApplicationSerilog(this IServiceCollection services, ShipmentProcessSettings settings)
    {
        Log.Logger = new LoggerConfiguration()
            .SetMinimumLevel(settings.Logging.Level)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        services.AddSerilog();

        return services;
    }

    internal static LoggerConfiguration SetMinimumLevel(this LoggerConfiguration configuration, string level)
    {
        var levelValue = GetLevelValue(level);
        return configuration
                .MinimumLevel.Is(levelValue)
                .MinimumLevel.Override("Microsoft", levelValue);
    }

    internal static LogEventLevel GetLevelValue(string level) =>
        LoggingConfigurator.GetLevelValue(level) switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => LogEventLevel.Error
        };
}
