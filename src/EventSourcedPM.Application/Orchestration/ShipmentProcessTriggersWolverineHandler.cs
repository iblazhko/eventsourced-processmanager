using EventSourcedPM.Messaging.Orchestration.Commands;
using EventSourcedPM.Messaging.Orchestration.Events;
using Wolverine;
using CollectionBookingEvents = EventSourcedPM.Messaging.CollectionBooking.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

namespace EventSourcedPM.Application.Orchestration;

// ReSharper disable once ClassNeverInstantiated.Global
public class ShipmentProcessTriggersWolverineHandler(IShipmentProcessManager processManager)
{
    public Task Handle(ProcessShipment message, Envelope envelope) =>
        processManager.InitializeProcess(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentProcessStarted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsStarted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsCompleted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CustomsInvoiceGenerationStarted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsEvents.CustomsInvoiceGenerated message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CustomsInvoiceGenerationCompleted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CustomsInvoiceGenerationFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentManifestationStarted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsEvents.ShipmentManifested message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsEvents.ShipmentManifestationFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentManifestationCompleted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentManifestationFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentLabelsGenerationStarted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsEvents.ShipmentLabelsGenerated message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentLabelsGenerationCompleted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentLabelsGenerationFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ReceiptGenerationStarted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsEvents.ShipmentReceiptGenerated message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ReceiptGenerationCompleted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ReceiptGenerationFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CombinedDocumentGenerationStarted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ManifestationAndDocumentsEvents.ShipmentCombinedDocumentGenerated message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CombinedDocumentGenerationCompleted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CombinedDocumentGenerationFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CollectionBookingStarted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CollectionBookingEvents.CollectionBooked message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CollectionBookingEvents.CollectionBookingSubprocessFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CollectionBookingCompleted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(CollectionBookingFailed message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentProcessMaybeCompleted message, Envelope envelope) =>
        processManager.InvokeProcessTrigger(message, ParseCorrelationId(envelope), envelope.Id);

    public Task Handle(ShipmentProcessCompleted message) => Task.CompletedTask;

    private static Guid? ParseCorrelationId(Envelope envelope) => Guid.TryParse(envelope.CorrelationId, out var id) ? id : null;
}
