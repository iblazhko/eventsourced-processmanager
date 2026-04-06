using EventSourcedPM.Messaging.Models;

namespace EventSourcedPM.Messaging.Orchestration.Commands;

public class ProcessShipment
{
    public string ShipmentId { get; init; }
    public ShipmentLeg[] Legs { get; init; }
    public string CollectionDate { get; init; }
    public string TimeZone { get; init; }
}
