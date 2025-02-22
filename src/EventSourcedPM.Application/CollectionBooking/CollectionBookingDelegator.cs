namespace EventSourcedPM.Application.CollectionBooking;

using System;
using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.CollectionBooking;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using EventSourcedPM.Ports.MessageBus;
using static EventSourcedPM.Application.Orchestration.DelegatorLogger;
using CarrierIntegrationCommands = EventSourcedPM.Ports.CarrierIntegration.Commands;

public interface ICollectionBookingDelegator
{
    Task DelegateDecision(CollectionBookingState collectionBookingState, BaseCollectionBookingEvent decision);
}

public class CollectionBookingDelegator(IMessageBus messageBus) : ICollectionBookingDelegator
{
    private IMessageBus MessageBus { get; } = messageBus;

    public Task DelegateDecision(CollectionBookingState collectionBookingState, BaseCollectionBookingEvent decision) =>
        decision switch
        {
            CollectionBookingWithCarrierStarted started => DelegateBookCollectionWithCarrier(collectionBookingState, started),
            _ => Task.CompletedTask,
        };

    private async Task DelegateBookCollectionWithCarrier(CollectionBookingState collectionBookingState, CollectionBookingWithCarrierStarted started)
    {
        var delegatedMessage = new CarrierIntegrationCommands.BookCollectionWithCarrier
        {
            ShipmentId = started.ShipmentId,
            CarrierId = (Guid)collectionBookingState.CollectionLeg.CarrierId,
            Sender = collectionBookingState.CollectionLeg.Sender,
            Receiver = collectionBookingState.CollectionLeg.Receiver,
            Collection = collectionBookingState.CollectionLeg.Collection,
        };

        LogDelegatingMessage(started, delegatedMessage);

        await MessageBus.SendCommand(delegatedMessage);
    }
}
