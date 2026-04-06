using EventSourcedPM.Ports.CarrierIntegration.Commands;
using EventSourcedPM.Ports.CarrierIntegration.Events;
using Serilog;
using IWolverineBus = Wolverine.IMessageBus;

namespace EventSourcedPM.Adapter.CarrierIntegrationStub;

// ReSharper disable once ClassNeverInstantiated.Global
public class CarrierIntegrationWolverineStubHandler(IWolverineBus bus)
{
    public async Task Handle(ManifestShipmentWithCarrier message)
    {
        Log.Information("In {MessageType} handler: {@MessagePayload}", message.GetType().FullName, message);

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        await bus.PublishAsync(
            message.ShipmentId.EndsWith('1')
                ? new ShipmentCarrierManifestationFailed
                {
                    ShipmentId = message.ShipmentId,
                    CarrierId = message.CarrierId,
                    Failure = Guid.NewGuid().ToString("N"),
                }
                : (object)
                    new ShipmentManifestedWithCarrier
                    {
                        ShipmentId = message.ShipmentId,
                        CarrierId = message.CarrierId,
                        TrackingNumber = Guid.NewGuid().ToString("N"),
                    }
        );
    }

    public async Task Handle(BookCollectionWithCarrier message)
    {
        Log.Information("In {MessageType} handler: {@MessagePayload}", message.GetType().FullName, message);

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        if (message.ShipmentId.EndsWith('2'))
        {
            await bus.PublishAsync(
                new CarrierCollectionBookingFailed
                {
                    ShipmentId = message.ShipmentId,
                    CarrierId = message.CarrierId,
                    Failure = Guid.NewGuid().ToString("N"),
                }
            );
        }
        else if (message.ShipmentId.EndsWith('3'))
        {
            await bus.PublishAsync(
                new CarrierCollectionBookingFailed
                {
                    ShipmentId = message.ShipmentId,
                    CarrierId = message.CarrierId,
                    Failure = Guid.NewGuid().ToString("N"),
                }
            );
            await Task.Delay(TimeSpan.FromSeconds(2));
            await bus.PublishAsync(
                new CollectionBookedWithCarrier
                {
                    ShipmentId = message.ShipmentId,
                    CarrierId = message.CarrierId,
                    BookingReference = Guid.NewGuid().ToString("N"),
                }
            );
        }
        else
        {
            await bus.PublishAsync(
                new CollectionBookedWithCarrier
                {
                    ShipmentId = message.ShipmentId,
                    CarrierId = message.CarrierId,
                    BookingReference = Guid.NewGuid().ToString("N"),
                }
            );
        }
    }
}
