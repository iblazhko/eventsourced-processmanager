namespace EventSourcedPM.Application.Orchestration;

using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Commands;

public interface IClassifyShipmentProcess
{
    ShipmentProcessCategory ClassifyShipment(ProcessShipment command);
}
