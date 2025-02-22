namespace EventSourcedPM.Application.Orchestration;

using EventSourcedPM.Application.Orchestration.Processes;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Commands;

public class ShipmentProcessClassifier : IClassifyShipmentProcess
{
    public ShipmentProcessCategory ClassifyShipment(ProcessShipment command) =>
        command.Legs.Length == 1 ? DomesticShipmentProcessV1.ShipmentProcessCategory : InternationalShipmentProcessV1.ShipmentProcessCategory;
}
