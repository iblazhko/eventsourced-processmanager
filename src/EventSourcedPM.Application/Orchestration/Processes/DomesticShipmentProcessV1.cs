namespace EventSourcedPM.Application.Orchestration.Processes;

using System.Collections.Generic;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Events;
using CollectionBookingEvents = EventSourcedPM.Messaging.CollectionBooking.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public class DomesticShipmentProcessV1 : IShipmentProcess
{
    public static readonly ShipmentProcessCategory ShipmentProcessCategory =
        (ShipmentProcessCategory)"domestic-1.0";

    public ShipmentProcessCategory Category => ShipmentProcessCategory;

    public IEnumerable<BaseShipmentProcessEvent> MakeDecision(
        ShipmentProcessState shipmentProcessState,
        ShipmentProcessTrigger trigger
    ) =>
        trigger.Match(
            processEvent =>
                processEvent switch
                {
                    ShipmentProcessStarted x => DecideThat.ManifestationAndDocumentsStarted(x),
                    ManifestationAndDocumentsStarted x
                        => DecideThat.ShipmentManifestationStarted(x),
                    ShipmentManifestationStarted _ => [],
                    ShipmentManifestationCompleted x
                        => DecideThat.ShipmentLabelsGenerationStarted(x),
                    ShipmentManifestationFailed x => DecideThat.ManifestationAndDocumentsFailed(x),
                    ShipmentLabelsGenerationStarted _ => [],
                    ShipmentLabelsGenerationCompleted x => DecideThat.ReceiptGenerationStarted(x),
                    ShipmentLabelsGenerationFailed x
                        => DecideThat.ManifestationAndDocumentsFailed(x),
                    ReceiptGenerationStarted _ => [],
                    ReceiptGenerationCompleted x => DecideThat.CombinedDocumentGenerationStarted(x),
                    ReceiptGenerationFailed x => DecideThat.ManifestationAndDocumentsFailed(x),
                    CombinedDocumentGenerationStarted _ => [],
                    CombinedDocumentGenerationCompleted x
                        => DecideThat.ManifestationAndDocumentsCompleted(x),
                    CombinedDocumentGenerationFailed x
                        => DecideThat.ManifestationAndDocumentsFailed(x),
                    ManifestationAndDocumentsCompleted x => DecideThat.CollectionBookingStarted(x),
                    ManifestationAndDocumentsFailed x => DecideThat.ShipmentProcessFailed(x),
                    CollectionBookingStarted _ => [],
                    CollectionBookingCompleted x => DecideThat.ShipmentProcessCompletionChecked(x),
                    CollectionBookingFailed x => DecideThat.ShipmentProcessFailed(x),
                    ShipmentProcessCompletionChecked x
                        => shipmentProcessState.StagesProgress.ManifestationAndDocuments.IsCompleted()
                        && shipmentProcessState.StagesProgress.CollectionBooking.IsCompletedOrNotRequired()
                            ? DecideThat.ShipmentProcessCompleted(x)
                            : [],
                    _ => throw new TriggerNotSupportedException(trigger.GetType().FullName)
                },
            manifestationAndDocumentsSubprocessEvent =>
                manifestationAndDocumentsSubprocessEvent switch
                {
                    ManifestationAndDocumentsEvents.ShipmentManifested x
                        => DecideThat.ShipmentManifestationCompleted(x),
                    ManifestationAndDocumentsEvents.ShipmentManifestationFailed x
                        => DecideThat.ShipmentManifestationFailed(x),
                    ManifestationAndDocumentsEvents.ShipmentLabelsGenerated x
                        => DecideThat.ShipmentLabelsGenerationCompleted(x),
                    ManifestationAndDocumentsEvents.ShipmentLabelsGenerationFailed x
                        => DecideThat.ShipmentLabelsGenerationFailed(x),
                    ManifestationAndDocumentsEvents.ShipmentReceiptGenerated x
                        => DecideThat.ReceiptGenerationCompleted(x),
                    ManifestationAndDocumentsEvents.ShipmentReceiptGenerationFailed x
                        => DecideThat.ReceiptGenerationFailed(x),
                    ManifestationAndDocumentsEvents.ShipmentCombinedDocumentGenerated x
                        => DecideThat.CombinedDocumentGenerationCompleted(x),
                    ManifestationAndDocumentsEvents.ShipmentCombinedDocumentGenerationFailed x
                        => DecideThat.CombinedDocumentGenerationFailed(x),
                    _ => throw new TriggerNotSupportedException(trigger.GetType().FullName)
                },
            collectionBookingSubprocessEvent =>
                collectionBookingSubprocessEvent switch
                {
                    CollectionBookingEvents.CollectionBooked x
                        => DecideThat.CollectionBookingCompleted(x),
                    CollectionBookingEvents.CollectionBookingFailed x
                        => DecideThat.CollectionBookingFailed(x),
                    _ => throw new TriggerNotSupportedException(trigger.GetType().FullName)
                }
        );
}
