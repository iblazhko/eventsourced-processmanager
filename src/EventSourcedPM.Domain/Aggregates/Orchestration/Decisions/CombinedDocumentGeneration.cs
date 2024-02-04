namespace EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;

using System.Collections.Generic;
using EventSourcedPM.Messaging.Orchestration.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public static partial class DecideThat
{
    public static IEnumerable<BaseShipmentProcessEvent> CombinedDocumentGenerationStarted(
        ReceiptGenerationCompleted trigger
    ) =>
        [
            new CombinedDocumentGenerationStarted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Delegated = true
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> CombinedDocumentGenerationCompleted(
        ManifestationAndDocumentsEvents.ShipmentCombinedDocumentGenerated trigger
    ) =>
        [
            new CombinedDocumentGenerationCompleted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                CombinedDocument = trigger.DocumentLocation
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> CombinedDocumentGenerationFailed(
        ManifestationAndDocumentsEvents.ShipmentCombinedDocumentGenerationFailed trigger
    ) =>
        [
            new CombinedDocumentGenerationFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure
            }
        ];
}
