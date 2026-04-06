using EventSourcedPM.Adapter.CarrierIntegrationStub;
using EventSourcedPM.Adapters.MassTransitMessageBus;
using EventSourcedPM.Application.CollectionBooking;
using EventSourcedPM.Application.ManifestationAndDocuments;
using EventSourcedPM.Application.Orchestration;
using EventSourcedPM.Configuration;
using EventSourcedPM.Ports.MessageBus;
using MassTransit;

namespace EventSourcedPM.Configurators;

public static class MassTransitConfigurator
{
    public static IServiceCollection AddMassTransitMessageBus(this IServiceCollection services, ShipmentProcessSettings settings)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ShipmentProcessTriggersMassTransitConsumer>();
            x.AddConsumer<ManifestationAndDocumentsTriggersMassTransitConsumer>();
            x.AddConsumer<CollectionBookingTriggersMassTransitConsumer>();
            x.AddConsumer<CarrierIntegrationMassTransitStubAdapter>();

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
                            .Handle([typeof(Ports.EventStore.ConcurrencyException), typeof(Application.Orchestration.ConcurrencyException)]);
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
