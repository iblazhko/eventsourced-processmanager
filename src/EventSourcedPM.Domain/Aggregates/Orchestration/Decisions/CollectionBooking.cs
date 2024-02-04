namespace EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;

using System.Collections.Generic;
using EventSourcedPM.Messaging.Orchestration.Events;
using CollectionBookingEvents = EventSourcedPM.Messaging.CollectionBooking.Events;

public static partial class DecideThat
{
    public static IEnumerable<BaseShipmentProcessEvent> CollectionBookingStarted(
        ManifestationAndDocumentsCompleted trigger
    ) =>
        [
            new CollectionBookingStarted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Delegated = true
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> CollectionBookingCompleted(
        CollectionBookingEvents.CollectionBooked trigger
    ) =>
        [
            new CollectionBookingCompleted
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                BookingReference = trigger.BookingReference
            }
        ];

    public static IEnumerable<BaseShipmentProcessEvent> CollectionBookingFailed(
        CollectionBookingEvents.CollectionBookingFailed trigger
    ) =>
        [
            new CollectionBookingFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure
            }
        ];
}
