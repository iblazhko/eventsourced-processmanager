namespace EventSourcedPM.Application.Orchestration;

using System;
using EventSourcedPM.Messaging;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using EventSourcedPM.Messaging.Orchestration.Events;
using OneOf;

public class ShipmentProcessTrigger
    : OneOfBase<BaseShipmentProcessEvent, BaseShipmentEvent, BaseCollectionBookingEvent>
{
    public static ShipmentProcessTrigger FromEvent(BaseShipmentWithProcessCategoryEvent trigger) =>
        trigger switch
        {
            BaseShipmentProcessEvent processEvent => new(processEvent),
            BaseShipmentEvent manifestationAndDocumentsSubprocessEvent
                => new(manifestationAndDocumentsSubprocessEvent),
            BaseCollectionBookingEvent collectionBookingSubprocessEvent
                => new(collectionBookingSubprocessEvent),
            _ => throw new TriggerNotSupportedException(trigger.GetType().FullName)
        };

    private ShipmentProcessTrigger(
        OneOf<BaseShipmentProcessEvent, BaseShipmentEvent, BaseCollectionBookingEvent> input
    )
        : base(input) { }
}

public class TriggerNotSupportedException(string eventType)
    : Exception($"Trigger event '{eventType}' is not supported");
