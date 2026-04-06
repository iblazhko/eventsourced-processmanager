using EventSourcedPM.Messaging.CollectionBooking.Commands;
using MassTransit;
using CarrierIntegrationEvents = EventSourcedPM.Ports.CarrierIntegration.Events;

namespace EventSourcedPM.Application.CollectionBooking;

// ReSharper disable once ClassNeverInstantiated.Global
public class CollectionBookingTriggersMassTransitConsumer(ICollectionBookingSubprocess collectionBookingSubprocess)
    : IConsumer<CreateCollectionBooking>,
        IConsumer<ScheduleCollectionBooking>,
        IConsumer<BookCollectionWithCarrier>,
        IConsumer<CarrierIntegrationEvents.CollectionBookedWithCarrier>,
        IConsumer<CarrierIntegrationEvents.CarrierCollectionBookingFailed>
{
    public Task Consume(ConsumeContext<CreateCollectionBooking> context) => collectionBookingSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<ScheduleCollectionBooking> context) => collectionBookingSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<BookCollectionWithCarrier> context) => collectionBookingSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<CarrierIntegrationEvents.CollectionBookedWithCarrier> context) =>
        collectionBookingSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<CarrierIntegrationEvents.CarrierCollectionBookingFailed> context) =>
        collectionBookingSubprocess.Handle(context.Message);
}
