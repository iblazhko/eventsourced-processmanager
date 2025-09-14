namespace EventSourcedPM.Configurators;

using System;
using EventSourcedPM.Adapters.EventStoreDb;
using EventSourcedPM.Adapters.MartenDbEventStore;
using EventSourcedPM.Adapters.MassTransitEventStorePublisher;
using EventSourcedPM.Configuration;
using EventSourcedPM.Domain.Aggregates.CollectionBooking;
using EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Messaging;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.EventStore;
using EventStore.Client;
using Marten;
using Microsoft.Extensions.DependencyInjection;

public static class EventStoreConfigurator
{
    public static IServiceCollection AddApplicationEventStore(this IServiceCollection services, ShipmentProcessSettings settings)
    {
        var eventStoreClientSettings = EventStoreClientSettings.Create(settings.EventStore.GetConnectionString());
        eventStoreClientSettings.ConnectionName = nameof(EventSourcedPM);
        services.AddSingleton(new EventStoreClient(eventStoreClientSettings));

        services
            .AddMarten(options =>
            {
                options.Connection(settings.Postgres.GetConnectionString());
                options.Events.StreamIdentity = JasperFx.Events.StreamIdentity.AsString;
                options.Events.MetadataConfig.HeadersEnabled = true;
                options.Events.MetadataConfig.CausationIdEnabled = true;
                options.Events.MetadataConfig.CorrelationIdEnabled = true;

                // Note: AutoCreateSchemaObjects most likely should be turned off in a real deployment
                options.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
            })
            .ApplyAllDatabaseChangesOnStartup();

        services.AddSingleton<IEventPublisher, MassTransitEventStorePublisherAdapter>();

        switch (settings.EventStoreAdapter)
        {
            case "EventStoreDB":
                services.AddSingleton<IEventTypeResolver, EventTypeResolver<BaseShipmentWithProcessCategoryEvent>>();
                services.AddSingleton<IEventSerializer, EventJsonSerializer>();

                services.AddSingleton<
                    IEventStore<ShipmentProcessState, BaseShipmentProcessEvent>,
                    EventStoreDbAdapter<ShipmentProcessState, BaseShipmentProcessEvent>
                >();
                services.AddSingleton<
                    IEventStore<ManifestationAndDocumentsState, BaseShipmentEvent>,
                    EventStoreDbAdapter<ManifestationAndDocumentsState, BaseShipmentEvent>
                >();
                services.AddSingleton<
                    IEventStore<CollectionBookingState, BaseCollectionBookingEvent>,
                    EventStoreDbAdapter<CollectionBookingState, BaseCollectionBookingEvent>
                >();
                break;

            case "MartenDB":
                services.AddSingleton<
                    IEventStore<ShipmentProcessState, BaseShipmentProcessEvent>,
                    MartenDbEventStoreAdapter<ShipmentProcessState, BaseShipmentProcessEvent>
                >();
                services.AddSingleton<
                    IEventStore<ManifestationAndDocumentsState, BaseShipmentEvent>,
                    MartenDbEventStoreAdapter<ManifestationAndDocumentsState, BaseShipmentEvent>
                >();
                services.AddSingleton<
                    IEventStore<CollectionBookingState, BaseCollectionBookingEvent>,
                    MartenDbEventStoreAdapter<CollectionBookingState, BaseCollectionBookingEvent>
                >();
                break;

            default:
                throw new InvalidOperationException($"EventStore adapter type '{settings.EventStoreAdapter}' is not supported");
        }

        services.AddSingleton<IEventStreamProjection<ShipmentProcessState, BaseShipmentProcessEvent>, ShipmentProcessStateProjection>();
        services.AddSingleton<EventSourcedRepository<ShipmentProcessState, BaseShipmentProcessEvent>>();

        services.AddSingleton<IEventStreamProjection<ManifestationAndDocumentsState, BaseShipmentEvent>, ManifestationAndDocumentsStateProjection>();
        services.AddSingleton<EventSourcedRepository<ManifestationAndDocumentsState, BaseShipmentEvent>>();

        services.AddSingleton<IEventStreamProjection<CollectionBookingState, BaseCollectionBookingEvent>, CollectionBookingStateProjection>();
        services.AddSingleton<EventSourcedRepository<CollectionBookingState, BaseCollectionBookingEvent>>();

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
