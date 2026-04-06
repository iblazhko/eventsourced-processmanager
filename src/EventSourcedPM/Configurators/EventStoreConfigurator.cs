using EventSourcedPM.Adapters.KurrentDb;
using EventSourcedPM.Adapters.MartenDbEventStore;
using EventSourcedPM.Adapters.MassTransitEventStorePublisher;
using EventSourcedPM.Adapters.WolverineEventStorePublisher;
using EventSourcedPM.Configuration;
using EventSourcedPM.Domain.Aggregates.CollectionBooking;
using EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Messaging;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.EventStore;
using KurrentDB.Client;
using Marten;

namespace EventSourcedPM.Configurators;

public static class EventStoreConfigurator
{
    public static IServiceCollection AddApplicationEventStore(this IServiceCollection services, ShipmentProcessSettings settings)
    {
        var eventStoreClientSettings = KurrentDBClientSettings.Create(settings.EventStore.GetConnectionString());
        eventStoreClientSettings.ConnectionName = nameof(EventSourcedPM);
        services.AddSingleton(new KurrentDBClient(eventStoreClientSettings));

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

        if (settings.MessageBusAdapter == "Wolverine")
            services.AddSingleton<IEventPublisher, WolverineEventStorePublisherAdapter>();
        else
            services.AddSingleton<IEventPublisher, MassTransitEventStorePublisherAdapter>();

        switch (settings.EventStoreAdapter)
        {
            case "KurrentDB":
                services.AddSingleton<IEventTypeResolver, EventTypeResolver<BaseShipmentWithProcessCategoryEvent>>();
                services.AddSingleton<IEventSerializer, EventJsonSerializer>();

                services.AddSingleton<
                    IEventStore<ShipmentProcessState, BaseShipmentProcessEvent>,
                    KurrentDbAdapter<ShipmentProcessState, BaseShipmentProcessEvent>
                >();
                services.AddSingleton<
                    IEventStore<ManifestationAndDocumentsState, BaseShipmentEvent>,
                    KurrentDbAdapter<ManifestationAndDocumentsState, BaseShipmentEvent>
                >();
                services.AddSingleton<
                    IEventStore<CollectionBookingState, BaseCollectionBookingEvent>,
                    KurrentDbAdapter<CollectionBookingState, BaseCollectionBookingEvent>
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
