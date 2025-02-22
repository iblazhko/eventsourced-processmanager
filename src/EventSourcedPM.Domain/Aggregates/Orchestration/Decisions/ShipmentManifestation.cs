namespace EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;

using System.Collections.Generic;
using EventSourcedPM.Messaging.Orchestration.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public static partial class DecideThat
{
    public static IEnumerable<BaseShipmentProcessEvent> ShipmentManifestationStarted(CustomsInvoiceGenerationCompleted trigger) =>
        [
            new ShipmentManifestationStarted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Delegated = true,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ShipmentManifestationStarted(ManifestationAndDocumentsStarted trigger) =>
        [
            new ShipmentManifestationStarted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Delegated = true,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ShipmentManifestationCompleted(ManifestationAndDocumentsEvents.ShipmentManifested trigger) =>
        [
            new ShipmentManifestationCompleted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                ManifestedLegs = trigger.ManifestedLegs,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ShipmentManifestationFailed(
        ManifestationAndDocumentsEvents.ShipmentManifestationFailed trigger
    ) =>
        [
            new ShipmentManifestationFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure,
            },
        ];
}
