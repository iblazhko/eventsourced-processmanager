namespace EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;

using System.Collections.Generic;
using EventSourcedPM.Messaging.Orchestration.Events;

public static partial class DecideThat
{
    public static IEnumerable<BaseShipmentProcessEvent> ManifestationAndDocumentsStarted(
        ShipmentProcessStarted trigger
    ) =>
        [
            new ManifestationAndDocumentsStarted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Delegated = true
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ManifestationAndDocumentsCompleted(
        CombinedDocumentGenerationCompleted trigger
    ) =>
        [
            new ManifestationAndDocumentsCompleted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId =  trigger.ShipmentId,
                Delegated = true
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ManifestationAndDocumentsFailed(
        CustomsInvoiceGenerationFailed trigger
    ) =>
        [
            new ManifestationAndDocumentsFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ManifestationAndDocumentsFailed(
        ShipmentManifestationFailed trigger
    ) =>
        [
            new ManifestationAndDocumentsFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ManifestationAndDocumentsFailed(
        ShipmentLabelsGenerationFailed trigger
    ) =>
        [
            new ManifestationAndDocumentsFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ManifestationAndDocumentsFailed(
        ReceiptGenerationFailed trigger
    ) =>
        [
            new ManifestationAndDocumentsFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ManifestationAndDocumentsFailed(
        CombinedDocumentGenerationFailed trigger
    ) =>
        [
            new ManifestationAndDocumentsFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure
            }
        ];
}
