namespace EventSourcedPM.Configurators;

using EventSourcedPM.Application.CollectionBooking;
using EventSourcedPM.Application.ManifestationAndDocuments;
using EventSourcedPM.Application.Orchestration;
using EventSourcedPM.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

public static class ProcessManagerConfigurator
{
    public static IServiceCollection AddApplicationProcessManager(this IServiceCollection services)
    {
        services.AddSingleton<IShipmentProcessManager, ShipmentProcessManager>();
        services.AddSingleton<IClassifyShipmentProcess, ShipmentProcessClassifier>();
        services.AddSingleton<IShipmentProcessDelegator, ShipmentProcessDelegator>();
        services.AddSingleton<
            IManifestationAndDocumentsDelegator,
            ManifestationAndDocumentsDelegator
        >();
        services.AddSingleton<ICollectionBookingDelegator, CollectionBookingDelegator>();
        services.AddSingleton<IShipmentProcessRegistry, ShipmentProcessRegistry>();
        services.AddSingleton<ICollectionBookingScheduler, CollectionBookingScheduler>();
        services.AddSingleton<
            IManifestationAndDocumentsSubprocess,
            ManifestationAndDocumentsSubprocess
        >();
        services.AddSingleton<ICollectionBookingSubprocess, CollectionBookingSubprocess>();

        return services;
    }
}
