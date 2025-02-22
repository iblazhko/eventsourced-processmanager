namespace EventSourcedPM.Application.CollectionBooking;

using System.Threading.Tasks;
using EventSourcedPM.Messaging.CollectionBooking.Commands;
using MassTransit;
using CarrierIntegrationEvents = EventSourcedPM.Ports.CarrierIntegration.Events;

// ReSharper disable once ClassNeverInstantiated.Global
public class CollectionBookingTriggersConsumer(ICollectionBookingSubprocess collectionBookingSubprocess)
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
