namespace EventSourcedPM.Application.Orchestration.Processes;

using System.Collections.Generic;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Events;
using CollectionBookingEvents = EventSourcedPM.Messaging.CollectionBooking.Events;
using ManifestationAndDocumentsEvents = EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public class InternationalShipmentProcessV1 : IShipmentProcess
{
    public static readonly ShipmentProcessCategory ShipmentProcessCategory =
        (ShipmentProcessCategory)"international-1.0";

    public ShipmentProcessCategory Category => ShipmentProcessCategory;

    /*
    TODO: Add checks to see if the incoming trigger is applicable for the current state

    Example of making process decision based on current process state:

    1) define state-specific deciders:

    private static readonly Dictionary<
        ShipmentProcessState,
        Func<
            ShipmentProcessState,
            BaseShipmentWithProcessCategoryEvent,
            IEnumerable<BaseShipmentProcessEvent>
        >
    > DecideFrom =
        new()
        {
            {
                ShipmentProcessState.ShipmentProcessStarted,
                (shipmentProcessState, trigger) =>
                    trigger switch
                    {
                        ManifestationAndDocumentsStarted x
                            => DecideThat.CustomsInvoiceGenerationStarted(x),
                        _ => TriggerNotSupported(shipmentProcessState, trigger)
                    }
            },
            {
                ShipmentProcessState.ManifestationAndDocumentsStarted,
                (shipmentProcessState, trigger) =>
                    trigger switch
                    {
                        CustomsInvoiceGenerationStarted _ => [],
                        _ => TriggerNotSupported(shipmentProcessState, trigger)
                    }
            },
            {
                ShipmentProcessState.CustomsInvoiceGenerationStarted,
                (shipmentProcessState, trigger) =>
                    trigger switch
                    {
                        ManifestationAndDocumentsEvents.CustomsInvoiceGenerated x
                            => DecideThat.CustomsInvoiceGenerationCompleted(x),
                        ManifestationAndDocumentsEvents.CustomsInvoiceGenerationFailed x
                            => DecideThat.CustomsInvoiceGenerationFailed(x),
                        _ => TriggerNotSupported(shipmentProcessState, trigger)
                    }
            },
            {
                ShipmentProcessState.CustomsInvoiceGenerationCompleted,
                (shipmentProcessState, trigger) =>
                    trigger switch
                    {
                        ShipmentManifestationStarted _ => [],
                        _ => TriggerNotSupported(shipmentProcessState, trigger)
                    }
            }
        };

        private static IEnumerable<BaseShipmentProcessEvent> TriggerNotSupported(
           IHoldShipmentProcessState shipmentProcessState,
           BaseShipmentWithProcessCategoryEvent trigger
           ) =>
               throw new TriggerEventNotSupportedForStateException(
                   trigger.GetType().FullName,
                   shipmentProcessState.ProcessState
               );

    2) then, in MakeDecision use it like this:
        DecideFrom.TryGetValue(shipmentProcessState.ProcessState, out var decideFrom)
          ? decideFrom(shipmentProcessState, trigger)
          : throw new TriggerEventNotSupportedForStateException(trigger.GetType().FullName, shipmentProcessState.ProcessState);
    */

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
                        => DecideThat.CustomsInvoiceGenerationStarted(x),
                    CustomsInvoiceGenerationStarted _ => [],
                    CustomsInvoiceGenerationCompleted x
                        => DecideThat.ShipmentManifestationStarted(x),
                    CustomsInvoiceGenerationFailed x
                        => DecideThat.ManifestationAndDocumentsFailed(x),
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
                    ManifestationAndDocumentsEvents.CustomsInvoiceGenerated x
                        => DecideThat.CustomsInvoiceGenerationCompleted(x),
                    ManifestationAndDocumentsEvents.CustomsInvoiceGenerationFailed x
                        => DecideThat.CustomsInvoiceGenerationFailed(x),
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
                    CollectionBookingEvents.CollectionBookingSubprocessFailed x
                        => DecideThat.CollectionBookingFailed(x),
                    _ => throw new TriggerNotSupportedException(trigger.GetType().FullName)
                }
        );
}
