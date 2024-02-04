namespace EventSourcedPM.Messaging;

public abstract class BaseShipmentWithProcessCategoryCommand
{
    public string ShipmentId { get; init; }
    public string ProcessCategory { get; init; }
}
