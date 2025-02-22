namespace EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;

using System.Collections.Generic;
using EventSourcedPM.Messaging.Orchestration.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public static partial class DecideThat
{
    public static IEnumerable<BaseShipmentProcessEvent> ReceiptGenerationStarted(ShipmentLabelsGenerationCompleted trigger) =>
        [
            new ReceiptGenerationStarted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Delegated = true,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ReceiptGenerationCompleted(
        ManifestationAndDocumentsEvents.ShipmentReceiptGenerated trigger
    ) =>
        [
            new ReceiptGenerationCompleted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Receipt = trigger.DocumentLocation,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ReceiptGenerationFailed(
        ManifestationAndDocumentsEvents.ShipmentReceiptGenerationFailed trigger
    ) =>
        [
            new ReceiptGenerationFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure,
            },
        ];
}
