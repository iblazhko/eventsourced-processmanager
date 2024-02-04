namespace EventSourcedPM.Messaging.Orchestration.Events;

public abstract class BaseShipmentProcessEvent : BaseShipmentWithProcessCategoryEvent
{
    public bool Delegated { get; init; }
}
