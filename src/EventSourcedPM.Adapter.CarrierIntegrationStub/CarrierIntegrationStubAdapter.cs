namespace EventSourcedPM.Adapter.CarrierIntegrationStub;

using System;
using System.Threading.Tasks;
using EventSourcedPM.Ports.CarrierIntegration.Commands;
using EventSourcedPM.Ports.CarrierIntegration.Events;
using MassTransit;
using Serilog;

// ReSharper disable once ClassNeverInstantiated.Global
public class CarrierIntegrationStubAdapter : IConsumer<ManifestShipmentWithCarrier>, IConsumer<BookCollectionWithCarrier>
{
    public async Task Consume(ConsumeContext<ManifestShipmentWithCarrier> context)
    {
        var message = context.Message;

        Log.Information("In {MessageType} consumer: {@MessagePayload}", message.GetType().FullName, message);

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        await context.Publish(
            message.ShipmentId.EndsWith('1')
                ? new ShipmentCarrierManifestationFailed
                {
                    ShipmentId = message.ShipmentId,
                    CarrierId = message.CarrierId,
                    Failure = Guid.NewGuid().ToString("N"),
                }
                : new ShipmentManifestedWithCarrier
                {
                    ShipmentId = message.ShipmentId,
                    CarrierId = message.CarrierId,
                    TrackingNumber = Guid.NewGuid().ToString("N"),
                }
        );
    }

    public async Task Consume(ConsumeContext<BookCollectionWithCarrier> context)
    {
        var message = context.Message;

        Log.Information("In {MessageType} consumer: {@MessagePayload}", message.GetType().FullName, message);

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        if (message.ShipmentId.EndsWith('2'))
        {
            await context.Publish(
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
            await context.Publish(
                new CarrierCollectionBookingFailed
                {
                    ShipmentId = message.ShipmentId,
                    CarrierId = message.CarrierId,
                    Failure = Guid.NewGuid().ToString("N"),
                }
            );
            await Task.Delay(TimeSpan.FromSeconds(2));
            await context.Publish(
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
            await context.Publish(
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
