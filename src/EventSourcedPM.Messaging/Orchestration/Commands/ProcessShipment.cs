namespace EventSourcedPM.Messaging.Orchestration.Commands;

using EventSourcedPM.Messaging.Models;

public class ProcessShipment
{
    public string ShipmentId { get; init; }
    public ShipmentLeg[] Legs { get; init; }
    public string CollectionDate { get; init; }
    public string TimeZone { get; init; }
}
