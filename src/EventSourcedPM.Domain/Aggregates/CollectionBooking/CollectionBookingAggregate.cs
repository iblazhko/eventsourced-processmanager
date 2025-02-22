namespace EventSourcedPM.Domain.Aggregates.CollectionBooking;

using System;
using System.Collections.Generic;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Domain.Services;
using EventSourcedPM.Messaging.CollectionBooking.Events;

public static class CollectionBookingAggregate
{
    public static IEnumerable<BaseCollectionBookingEvent> Create(
        ShipmentProcessCategory processCategory,
        CollectionBookingId shipmentId,
        ManifestedShipmentLeg collectionLeg,
        DateOnly collectionDate,
        TimeZoneId timeZone
    ) =>
        [
            new CollectionBookingInitialized
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                CollectionLeg = collectionLeg.ToDto(),
                CollectionDate = collectionDate.ToIsoDate(),
                TimeZone = (string)timeZone,
            },
        ];

    public static IEnumerable<BaseCollectionBookingEvent> ScheduleCollectionBooking(
        ShipmentProcessCategory processCategory,
        CollectionBookingState state,
        DateOnly collectionDate,
        ICollectionBookingScheduler collectionBookingScheduler
    )
    {
        var schedulingResult = collectionBookingScheduler.ScheduleCollectionBooking(state.CollectionLeg, collectionDate, state.TimeZone);

        return
        [
            schedulingResult.Match(
                scheduled =>
                    new CollectionBookingScheduled
                    {
                        ShipmentId = (string)state.ShipmentId,
                        ProcessCategory = (string)processCategory,
                        CarrierId = (Guid)state.CollectionLeg.CarrierId,
                        CollectionDate = state.CollectionDate.ToIsoDate(),
                        BookAt = scheduled.BookAt.ToIsoTimestamp(),
                    } as BaseCollectionBookingEvent,
                failed => new CollectionBookingSchedulingFailed
                {
                    ShipmentId = (string)state.ShipmentId,
                    ProcessCategory = (string)processCategory,
                    CarrierId = (Guid)state.CollectionLeg.CarrierId,
                    CollectionDate = state.CollectionDate.ToIsoDate(),
                    Failure = failed.Failure,
                }
            ),
        ];
    }

    public static IEnumerable<BaseCollectionBookingEvent> BookCollectionWithCarrier(
        ShipmentProcessCategory processCategory,
        CollectionBookingState state
    ) =>
        [
            new CollectionBookingWithCarrierStarted
            {
                ShipmentId = (string)state.ShipmentId,
                ProcessCategory = (string)processCategory,
                Delegated = true,
            },
        ];

    public static IEnumerable<BaseCollectionBookingEvent> SetAsBookedWithCarrier(
        ShipmentProcessCategory processCategory,
        CollectionBookingState state,
        string bookingReference
    ) =>
        [
            new CollectionBooked
            {
                ShipmentId = (string)state.ShipmentId,
                CarrierId = (Guid)state.CollectionLeg.CarrierId,
                ProcessCategory = (string)processCategory,
                BookingReference = bookingReference,
            },
        ];

    public static IEnumerable<BaseCollectionBookingEvent> SetAsCarrierCollectionBookingFailed(
        ShipmentProcessCategory processCategory,
        CollectionBookingState state,
        string failure
    ) =>
        [
            new CollectionBookingSubprocessFailed
            {
                ShipmentId = (string)state.ShipmentId,
                ProcessCategory = (string)processCategory,
                Failure = failure,
            },
        ];
}
