namespace EventSourcedPM.Configurators;

using EventSourcedPM.Adapters.MartenDbEventStore;
using EventSourcedPM.Adapters.MassTransitEventStorePublisher;
using EventSourcedPM.Configuration;
using EventSourcedPM.Domain.Aggregates.CollectionBooking;
using EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.EventStore;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

public static class EventStoreConfigurator
{
    public static IServiceCollection AddApplicationEventStore(
        this IServiceCollection services,
        ShipmentProcessSettings settings
    )
    {
        services
            .AddMarten(options =>
            {
                options.Connection(settings.Postgres.GetConnectionString());
                options.Events.StreamIdentity = StreamIdentity.AsString;
                options.Events.MetadataConfig.HeadersEnabled = true;
                options.Events.MetadataConfig.CausationIdEnabled = true;
                options.Events.MetadataConfig.CorrelationIdEnabled = true;

                // Note: AutoCreateSchemaObjects most likely should be turned off in a real deployment
                options.AutoCreateSchemaObjects = AutoCreate.All;
            })
            .ApplyAllDatabaseChangesOnStartup();

        services.AddSingleton<IEventPublisher, MassTransitEventStorePublisherAdapter>();

        services.AddSingleton<
            IEventStore<ShipmentProcessState, BaseShipmentProcessEvent>,
            MartenDbEventStoreAdapter<ShipmentProcessState, BaseShipmentProcessEvent>
        >();
        services.AddSingleton<
            IEventStreamProjection<ShipmentProcessState, BaseShipmentProcessEvent>,
            ShipmentProcessStateProjection
        >();
        services.AddSingleton<
            EventSourcedRepository<ShipmentProcessState, BaseShipmentProcessEvent>
        >();

        services.AddSingleton<
            IEventStore<ManifestationAndDocumentsState, BaseShipmentEvent>,
            MartenDbEventStoreAdapter<ManifestationAndDocumentsState, BaseShipmentEvent>
        >();
        services.AddSingleton<
            IEventStreamProjection<ManifestationAndDocumentsState, BaseShipmentEvent>,
            ManifestationAndDocumentsStateProjection
        >();
        services.AddSingleton<
            EventSourcedRepository<ManifestationAndDocumentsState, BaseShipmentEvent>
        >();

        services.AddSingleton<
            IEventStore<CollectionBookingState, BaseCollectionBookingEvent>,
            MartenDbEventStoreAdapter<CollectionBookingState, BaseCollectionBookingEvent>
        >();
        services.AddSingleton<
            IEventStreamProjection<CollectionBookingState, BaseCollectionBookingEvent>,
            CollectionBookingStateProjection
        >();
        services.AddSingleton<
            EventSourcedRepository<CollectionBookingState, BaseCollectionBookingEvent>
        >();

        return services;
    }
}
