using EventSourcedPM.Messaging.CollectionBooking.Commands;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using CarrierIntegrationEvents = EventSourcedPM.Ports.CarrierIntegration.Events;

namespace EventSourcedPM.Application.CollectionBooking;

// ReSharper disable once ClassNeverInstantiated.Global
public class CollectionBookingTriggersWolverineHandler(ICollectionBookingSubprocess collectionBookingSubprocess)
{
    public Task Handle(CreateCollectionBooking message) => collectionBookingSubprocess.Handle(message);

    public Task Handle(ScheduleCollectionBooking message) => collectionBookingSubprocess.Handle(message);

    public Task Handle(BookCollectionWithCarrier message) => collectionBookingSubprocess.Handle(message);

    public Task Handle(CarrierIntegrationEvents.CollectionBookedWithCarrier message) => collectionBookingSubprocess.Handle(message);

    public Task Handle(CarrierIntegrationEvents.CarrierCollectionBookingFailed message) => collectionBookingSubprocess.Handle(message);

    public Task Handle(CollectionBookingScheduled message) => collectionBookingSubprocess.Handle(message);
}
