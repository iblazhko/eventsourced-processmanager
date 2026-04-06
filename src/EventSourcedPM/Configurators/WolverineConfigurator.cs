using EventSourcedPM.Adapter.CarrierIntegrationStub;
using EventSourcedPM.Application.Orchestration;
using EventSourcedPM.Configuration;
using EventSourcedPM.Messaging.CollectionBooking.Commands;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Commands;
using EventSourcedPM.Messaging.Orchestration.Commands;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.CarrierIntegration.Commands;
using Serilog.Events;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;
using CarrierIntegrationEvents = EventSourcedPM.Ports.CarrierIntegration.Events;
using CollectionBookingEvents = EventSourcedPM.Messaging.CollectionBooking.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

namespace EventSourcedPM.Configurators;

public static class WolverineConfigurator
{
    private const string ShipmentProcessTriggersQueue = "shipment-process";
    private const string ManifestationAndDocumentsTriggersQueue = "manifestation-and-documents";
    private const string CollectionBookingTriggersQueue = "collection-booking";
    private const string CarrierIntegrationStubQueue = "carrier-integration";

    public static IHostBuilder AddWolverineMessageBus(this IHostBuilder hostBuilder, ShipmentProcessSettings settings) =>
        hostBuilder.UseWolverine(opts =>
        {
            opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = settings.RabbitMq.Endpoint.Host;
                rabbit.Port = settings.RabbitMq.Endpoint.Port;
                rabbit.UserName = settings.RabbitMq.Username;
                rabbit.Password = settings.RabbitMq.Password;
                rabbit.VirtualHost = settings.RabbitMq.VHost;
            }).AutoProvision();

            opts.ListenToRabbitQueue(ShipmentProcessTriggersQueue);
            opts.ListenToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
            opts.ListenToRabbitQueue(CollectionBookingTriggersQueue);
            opts.ListenToRabbitQueue(CarrierIntegrationStubQueue);

            ConfigureMessageRouting(opts);

            opts.Discovery.IncludeAssembly(typeof(ShipmentProcessTriggersWolverineHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(CarrierIntegrationWolverineStubHandler).Assembly);

            var retryIntervals = BuildRetryIntervals(settings.Wolverine?.Retry);

            opts.Policies.OnException<ConcurrencyException>().RetryWithCooldown(retryIntervals);
            opts.Policies.OnException<Ports.EventStore.ConcurrencyException>().RetryWithCooldown(retryIntervals);

            var concurrencyLimit = settings.Wolverine?.ConcurrencyLimit ?? 0;
            if (concurrencyLimit > 0)
            {
                opts.Policies.AllLocalQueues(x => x.MaximumParallelMessages(concurrencyLimit));
            }

            SetLogMessageStartingPolicy(opts, settings);
        });

    private static void SetLogMessageStartingPolicy(WolverineOptions opts, ShipmentProcessSettings settings)
    {
        var logLevelValue = LoggingConfigurator.GetLevelValue(settings.Logging.Level);
        var logMessageStarting = logLevelValue switch
        {
            LogLevel.Debug or LogLevel.Trace => true,
            _ => false
        };

        if (logMessageStarting)
        {
            opts.Policies.LogMessageStarting(logLevelValue);
        }
    }

    private static void ConfigureMessageRouting(WolverineOptions opts)
    {
        // Shipment process triggers
        opts.PublishMessage<ProcessShipment>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentProcessStarted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsStarted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CustomsInvoiceGenerationStarted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsEvents.CustomsInvoiceGenerated>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CustomsInvoiceGenerationCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CustomsInvoiceGenerationFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentManifestationStarted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsEvents.ShipmentManifested>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsEvents.ShipmentManifestationFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentManifestationCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentManifestationFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentLabelsGenerationStarted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsEvents.ShipmentLabelsGenerated>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentLabelsGenerationCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentLabelsGenerationFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ReceiptGenerationStarted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsEvents.ShipmentReceiptGenerated>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ReceiptGenerationCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ReceiptGenerationFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CombinedDocumentGenerationStarted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsEvents.ShipmentCombinedDocumentGenerated>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CombinedDocumentGenerationCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CombinedDocumentGenerationFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CollectionBookingStarted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CollectionBookingEvents.CollectionBooked>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CollectionBookingEvents.CollectionBookingSubprocessFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CollectionBookingCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<CollectionBookingFailed>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentProcessMaybeCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);
        opts.PublishMessage<ShipmentProcessCompleted>().ToRabbitQueue(ShipmentProcessTriggersQueue);

        // Manifestation and documents triggers
        opts.PublishMessage<CreateShipment>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
        opts.PublishMessage<GenerateCustomsInvoice>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
        opts.PublishMessage<ManifestShipment>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
        opts.PublishMessage<CarrierIntegrationEvents.ShipmentManifestedWithCarrier>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
        opts.PublishMessage<CarrierIntegrationEvents.ShipmentCarrierManifestationFailed>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
        opts.PublishMessage<ManifestationAndDocumentsEvents.ShipmentLegManifested>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
        opts.PublishMessage<GenerateShipmentLabels>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
        opts.PublishMessage<GenerateCombinedDocument>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);
        opts.PublishMessage<GenerateShipmentReceipt>().ToRabbitQueue(ManifestationAndDocumentsTriggersQueue);

        // Collection booking triggers
        opts.PublishMessage<CreateCollectionBooking>().ToRabbitQueue(CollectionBookingTriggersQueue);
        opts.PublishMessage<ScheduleCollectionBooking>().ToRabbitQueue(CollectionBookingTriggersQueue);
        opts.PublishMessage<Messaging.CollectionBooking.Commands.BookCollectionWithCarrier>().ToRabbitQueue(CollectionBookingTriggersQueue);
        opts.PublishMessage<CarrierIntegrationEvents.CollectionBookedWithCarrier>().ToRabbitQueue(CollectionBookingTriggersQueue);
        opts.PublishMessage<CarrierIntegrationEvents.CarrierCollectionBookingFailed>().ToRabbitQueue(CollectionBookingTriggersQueue);
        opts.PublishMessage<CollectionBookingEvents.CollectionBookingScheduled>().ToRabbitQueue(CollectionBookingTriggersQueue);

        // Carrier integration stub
        opts.PublishMessage<ManifestShipmentWithCarrier>().ToRabbitQueue(CarrierIntegrationStubQueue);
        opts.PublishMessage<Ports.CarrierIntegration.Commands.BookCollectionWithCarrier>().ToRabbitQueue(CarrierIntegrationStubQueue);
    }

    private static System.TimeSpan[] BuildRetryIntervals(Configuration.RetrySettings retry)
    {
        if (retry is null)
            return [System.TimeSpan.FromMilliseconds(100), System.TimeSpan.FromSeconds(1), System.TimeSpan.FromSeconds(5)];

        var intervals = new System.TimeSpan[retry.Limit];
        var current = retry.IntervalMin;
        for (var i = 0; i < retry.Limit; i++)
        {
            intervals[i] = current;
            current += retry.IntervalDelta;
            if (current > retry.IntervalMax)
                current = retry.IntervalMax;
        }
        return intervals;
    }
}
