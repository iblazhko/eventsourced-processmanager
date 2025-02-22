namespace EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;

using System.Collections.Generic;
using EventSourcedPM.Messaging.Orchestration.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public static partial class DecideThat
{
    public static IEnumerable<BaseShipmentProcessEvent> ShipmentLabelsGenerationStarted(ShipmentManifestationCompleted trigger) =>
        [
            new ShipmentLabelsGenerationStarted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Delegated = true,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ShipmentLabelsGenerationCompleted(
        ManifestationAndDocumentsEvents.ShipmentLabelsGenerated trigger
    ) =>
        [
            new ShipmentLabelsGenerationCompleted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                ShipmentLabels = trigger.DocumentLocation,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ShipmentLabelsGenerationFailed(
        ManifestationAndDocumentsEvents.ShipmentLabelsGenerationFailed trigger
    ) =>
        [
            new ShipmentLabelsGenerationFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure,
            },
        ];
}
