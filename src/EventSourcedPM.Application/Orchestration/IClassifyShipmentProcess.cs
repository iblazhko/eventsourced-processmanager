using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Commands;

namespace EventSourcedPM.Application.Orchestration;

public interface IClassifyShipmentProcess
{
    ShipmentProcessCategory ClassifyShipment(ProcessShipment command);
}
