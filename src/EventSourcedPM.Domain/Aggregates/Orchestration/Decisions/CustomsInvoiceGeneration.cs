namespace EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;

using System.Collections.Generic;
using EventSourcedPM.Messaging.Orchestration.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public static partial class DecideThat
{
    public static IEnumerable<BaseShipmentProcessEvent> CustomsInvoiceGenerationStarted(ManifestationAndDocumentsStarted trigger) =>
        [
            new CustomsInvoiceGenerationStarted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Delegated = true,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> CustomsInvoiceGenerationCompleted(
        ManifestationAndDocumentsEvents.CustomsInvoiceGenerated trigger
    ) =>
        [
            new CustomsInvoiceGenerationCompleted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                CustomsInvoice = trigger.DocumentLocation,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> CustomsInvoiceGenerationFailed(
        ManifestationAndDocumentsEvents.CustomsInvoiceGenerationFailed trigger
    ) =>
        [
            new CustomsInvoiceGenerationFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure,
            },
        ];
}
