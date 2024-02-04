namespace EventSourcedPM.Messaging;

public abstract class BaseShipmentWithProcessCategoryEvent
{
    public string ShipmentId { get; init; }
    public string ProcessCategory { get; init; }
}
