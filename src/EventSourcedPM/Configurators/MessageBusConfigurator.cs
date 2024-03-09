namespace EventSourcedPM.Configurators;

using EventSourcedPM.Adapter.CarrierIntegrationStub;
using EventSourcedPM.Adapters.MassTransitMessageBus;
using EventSourcedPM.Application.CollectionBooking;
using EventSourcedPM.Application.ManifestationAndDocuments;
using EventSourcedPM.Application.Orchestration;
using EventSourcedPM.Configuration;
using EventSourcedPM.Ports.MessageBus;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

public static class MassTransitConfigurator
{
    public static IServiceCollection AddApplicationMessageBus(
        this IServiceCollection services,
        ShipmentProcessSettings settings
    )
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ShipmentProcessTriggersConsumer>();
            x.AddConsumer<ManifestationAndDocumentsTriggersConsumer>();
            x.AddConsumer<CollectionBookingTriggersConsumer>();
            x.AddConsumer<CarrierIntegrationStubAdapter>();

            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    cfg.Host(
                        settings.RabbitMq.Endpoint.Host,
                        settings.RabbitMq.VHost,
                        h =>
                        {
                            h.Username(settings.RabbitMq.Username);
                            h.Password(settings.RabbitMq.Password);
                        }
                    );

                    cfg.UseMessageRetry(retryConfig =>
                    {
                        retryConfig
                            .Exponential(
                                settings.MassTransit.Retry.Limit,
                                settings.MassTransit.Retry.IntervalMin,
                                settings.MassTransit.Retry.IntervalMax,
                                settings.MassTransit.Retry.IntervalDelta
                            )
                            .Handle<Ports.EventStore.ConcurrencyException>();
                    });

                    cfg.PrefetchCount = settings.MassTransit.PrefetchCount;
                    cfg.ConcurrentMessageLimit = settings.MassTransit.ConcurrencyLimit;

                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        services.AddSingleton<IMessageBus, MassTransitMessageBusAdapter>();

        return services;
    }
}
