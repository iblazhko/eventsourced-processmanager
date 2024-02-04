namespace EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public abstract class BaseShipmentEvent : BaseShipmentWithProcessCategoryEvent
{
    public bool Delegated { get; init; }
}
