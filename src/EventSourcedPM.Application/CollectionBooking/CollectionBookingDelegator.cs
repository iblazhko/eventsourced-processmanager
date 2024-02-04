namespace EventSourcedPM.Application.CollectionBooking;

using System;
using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.CollectionBooking;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using EventSourcedPM.Ports.MessageBus;
using Serilog;
using CarrierIntegrationCommands = EventSourcedPM.Ports.CarrierIntegration.Commands;

public interface ICollectionBookingDelegator
{
    Task DelegateDecision(
        CollectionBookingState collectionBookingState,
        BaseCollectionBookingEvent decision
    );
}

public class CollectionBookingDelegator : ICollectionBookingDelegator
{
    private readonly IMessageBus messageBus;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CollectionBookingDelegator(IMessageBus messageBus)
    {
        this.messageBus = messageBus;
    }

    public async Task DelegateDecision(
        CollectionBookingState collectionBookingState,
        BaseCollectionBookingEvent decision
    )
    {
        switch (decision)
        {
            case CollectionBookingWithCarrierStarted started:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(CollectionBookingWithCarrierStarted).FullName,
                    typeof(CarrierIntegrationCommands.BookCollectionWithCarrier).FullName
                );

                await messageBus.SendCommand(
                    new CarrierIntegrationCommands.BookCollectionWithCarrier
                    {
                        ShipmentId = started.ShipmentId,
                        CarrierId = (Guid)collectionBookingState.CollectionLeg.CarrierId,
                        Sender = collectionBookingState.CollectionLeg.Sender,
                        Receiver = collectionBookingState.CollectionLeg.Receiver,
                        Collection = collectionBookingState.CollectionLeg.Collection
                    }
                );
                break;
        }
    }
}
