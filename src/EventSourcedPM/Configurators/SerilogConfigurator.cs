namespace EventSourcedPM.Configurators;

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

public static class SerilogConfigurator
{
    public static IServiceCollection AddApplicationSerilog(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        services.AddSerilog();

        return services;
    }
}
