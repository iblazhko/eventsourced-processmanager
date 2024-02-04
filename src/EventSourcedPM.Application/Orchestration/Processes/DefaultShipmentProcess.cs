namespace EventSourcedPM.Application.Orchestration.Processes;

using EventSourcedPM.Domain.Models;

public class DefaultShipmentProcess : InternationalShipmentProcessV1
{
    public static new readonly ShipmentProcessCategory ShipmentProcessCategory =
        (ShipmentProcessCategory)"default-1.0";

    public new ShipmentProcessCategory Category => ShipmentProcessCategory;
}
