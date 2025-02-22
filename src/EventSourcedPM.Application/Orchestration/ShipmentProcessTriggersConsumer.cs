namespace EventSourcedPM.Application.Orchestration;

using System.Threading.Tasks;
using EventSourcedPM.Messaging.Orchestration.Commands;
using EventSourcedPM.Messaging.Orchestration.Events;
using MassTransit;
using CollectionBookingEvents = EventSourcedPM.Messaging.CollectionBooking.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

// ReSharper disable once ClassNeverInstantiated.Global
public class ShipmentProcessTriggersConsumer(IShipmentProcessManager processManager)
    : IConsumer<ProcessShipment>,
        IConsumer<ShipmentProcessStarted>,
        IConsumer<ManifestationAndDocumentsStarted>,
        IConsumer<ManifestationAndDocumentsCompleted>,
        IConsumer<ManifestationAndDocumentsFailed>,
        IConsumer<CustomsInvoiceGenerationStarted>,
        IConsumer<ManifestationAndDocumentsEvents.CustomsInvoiceGenerated>,
        IConsumer<CustomsInvoiceGenerationCompleted>,
        IConsumer<CustomsInvoiceGenerationFailed>,
        IConsumer<ShipmentManifestationStarted>,
        IConsumer<ManifestationAndDocumentsEvents.ShipmentManifested>,
        IConsumer<ManifestationAndDocumentsEvents.ShipmentManifestationFailed>,
        IConsumer<ShipmentManifestationCompleted>,
        IConsumer<ShipmentManifestationFailed>,
        IConsumer<ShipmentLabelsGenerationStarted>,
        IConsumer<ManifestationAndDocumentsEvents.ShipmentLabelsGenerated>,
        IConsumer<ShipmentLabelsGenerationCompleted>,
        IConsumer<ShipmentLabelsGenerationFailed>,
        IConsumer<ReceiptGenerationStarted>,
        IConsumer<ManifestationAndDocumentsEvents.ShipmentReceiptGenerated>,
        IConsumer<ReceiptGenerationCompleted>,
        IConsumer<ReceiptGenerationFailed>,
        IConsumer<CombinedDocumentGenerationStarted>,
        IConsumer<ManifestationAndDocumentsEvents.ShipmentCombinedDocumentGenerated>,
        IConsumer<CombinedDocumentGenerationCompleted>,
        IConsumer<CombinedDocumentGenerationFailed>,
        IConsumer<CollectionBookingStarted>,
        IConsumer<CollectionBookingEvents.CollectionBooked>,
        IConsumer<CollectionBookingEvents.CollectionBookingSubprocessFailed>,
        IConsumer<CollectionBookingCompleted>,
        IConsumer<CollectionBookingFailed>,
        IConsumer<ShipmentProcessMaybeCompleted>
// a.t.m. there is no real need to consume some of the events above
// e.g. CustomsInvoiceGenerationStarted or ShipmentManifestationStarted
// will not result in any new process manager decisions.
// however we are consuming all events relevant to shipment process for the sake of consistency
// and to allow changing process definition without having to (re-)wire triggers
{
    public Task Consume(ConsumeContext<ProcessShipment> context) =>
        processManager.InitializeProcess(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ShipmentProcessStarted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsStarted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsCompleted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CustomsInvoiceGenerationStarted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsEvents.CustomsInvoiceGenerated> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CustomsInvoiceGenerationCompleted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CustomsInvoiceGenerationFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ShipmentManifestationStarted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsEvents.ShipmentManifested> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsEvents.ShipmentManifestationFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ShipmentManifestationCompleted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ShipmentManifestationFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ShipmentLabelsGenerationStarted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsEvents.ShipmentLabelsGenerated> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ShipmentLabelsGenerationCompleted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ShipmentLabelsGenerationFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ReceiptGenerationStarted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsEvents.ShipmentReceiptGenerated> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ReceiptGenerationCompleted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ReceiptGenerationFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CombinedDocumentGenerationStarted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ManifestationAndDocumentsEvents.ShipmentCombinedDocumentGenerated> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CombinedDocumentGenerationCompleted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CombinedDocumentGenerationFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CollectionBookingStarted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CollectionBookingEvents.CollectionBooked> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CollectionBookingEvents.CollectionBookingSubprocessFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CollectionBookingCompleted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<CollectionBookingFailed> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);

    public Task Consume(ConsumeContext<ShipmentProcessMaybeCompleted> context) =>
        processManager.InvokeProcessTrigger(context.Message, context.CorrelationId, context.MessageId);
}
