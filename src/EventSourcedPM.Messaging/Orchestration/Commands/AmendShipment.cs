using EventSourcedPM.Messaging.Models;

namespace EventSourcedPM.Messaging.Orchestration.Commands;

public class AmendShipment
{
    public string ShipmentId { get; init; }
    public ShipmentLeg[] Legs { get; init; }
}
