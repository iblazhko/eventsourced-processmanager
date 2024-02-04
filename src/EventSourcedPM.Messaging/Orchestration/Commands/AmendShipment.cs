namespace EventSourcedPM.Messaging.Orchestration.Commands;

using EventSourcedPM.Messaging.Models;

public class AmendShipment
{
    public string ShipmentId { get; init; }
    public ShipmentLeg[] Legs { get; init; }
}
