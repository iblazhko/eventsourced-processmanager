namespace EventSourcedPM.Domain.Aggregates.CollectionBooking;

using System;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using EventSourcedPM.Ports.EventStore;

// ReSharper disable once UnusedType.Global
public class CollectionBookingStateProjection : IEventStreamProjection<CollectionBookingState, BaseCollectionBookingEvent>
{
    public CollectionBookingState GetInitialState(EventStreamId streamId) => new(default, default, default, default, default, default);

    public CollectionBookingState Apply(CollectionBookingState state, BaseCollectionBookingEvent evt) =>
        evt switch
        {
            CollectionBookingInitialized x => new(
                (ShipmentProcessCategory)x.ProcessCategory,
                (CollectionBookingId)x.ShipmentId,
                x.CollectionLeg.ToDomain(),
                DateOnly.TryParse(x.CollectionDate, out var date) ? date : DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
                (TimeZoneId)x.TimeZone,
                CollectionBookingProcessStatus.Initialized()
            ),
            CollectionBookingScheduled x => state with
            {
                CollectionDate = DateOnly.TryParse(x.CollectionDate, out var collectionDate)
                    ? collectionDate
                    : DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
                Status = CollectionBookingProcessStatus.Scheduled(DateTime.TryParse(x.BookAt, out var bookAt) ? bookAt : DateTime.UtcNow),
            },
            CollectionBookingSchedulingFailed x => state with
            {
                CollectionDate = DateOnly.TryParse(x.CollectionDate, out var date) ? date : DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
                Status = CollectionBookingProcessStatus.SchedulingFailed(x.Failure),
            },
            CollectionBookingWithCarrierStarted => state with { Status = CollectionBookingProcessStatus.BookingStarted() },
            CollectionBooked x => state with { Status = CollectionBookingProcessStatus.Booked(x.BookingReference) },
            CollectionBookingSubprocessFailed x => state with { Status = CollectionBookingProcessStatus.BookingFailed(x.Failure) },
            CollectionBookingCancellationStarted x => state with { Status = CollectionBookingProcessStatus.CancellationStarted(x.BookingReference) },
            CollectionBookingCancelled => state with { Status = CollectionBookingProcessStatus.Cancelled() },
            CollectionBookingCancellationFailed x => state with { Status = CollectionBookingProcessStatus.CancellationFailed(x.Failure) },
            _ => state,
        };
}
