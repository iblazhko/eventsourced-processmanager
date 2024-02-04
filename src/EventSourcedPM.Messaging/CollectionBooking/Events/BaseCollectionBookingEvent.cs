namespace EventSourcedPM.Messaging.CollectionBooking.Events;

public abstract class BaseCollectionBookingEvent : BaseShipmentWithProcessCategoryEvent
{
    public bool Delegated { get; init; }
}
