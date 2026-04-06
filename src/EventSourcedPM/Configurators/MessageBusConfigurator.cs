using EventSourcedPM.Adapters.WolverineMessageBus;
using EventSourcedPM.Configuration;
using EventSourcedPM.Ports.MessageBus;

namespace EventSourcedPM.Configurators;

public static class MessageBusConfigurator
{
    public static IServiceCollection AddApplicationMessageBus(this IServiceCollection services, ShipmentProcessSettings settings) =>
        settings.MessageBusAdapter switch
        {
            "Wolverine" => services.AddSingleton<IMessageBus, WolverineMessageBusAdapter>(),
            "MassTransit" => services.AddMassTransitMessageBus(settings),
            _ => throw new InvalidOperationException($"MessageBus adapter type '{settings.MessageBusAdapter}' is not supported"),
        };

    public static IHostBuilder AddApplicationMessageBus(this IHostBuilder hostBuilder, ShipmentProcessSettings settings) =>
        settings.MessageBusAdapter == "Wolverine"
            ? hostBuilder.AddWolverineMessageBus(settings)
            : hostBuilder;

}
